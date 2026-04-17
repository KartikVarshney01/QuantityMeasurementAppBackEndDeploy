// UC18: ASP.NET Core 8 Web API bootstrap with JWT authentication.
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuantityMeasurementAPI.Middleware;
using QuantityMeasurementAppBusinessLayer.Interfaces;
using QuantityMeasurementAppBusinessLayer.Services;
using QuantityMeasurementAppRepoLayer.Data;
using QuantityMeasurementAppRepoLayer.Implementations;
using QuantityMeasurementAppRepoLayer.Interfaces;

// ── Dependency graph (no circular references) ─────────────────────────────────
//   QuantityMeasurementAPI
//     → QuantityMeasurementAppBusinessLayer   (engines, services)
//     → QuantityMeasurementAppRepoLayer       (owns ApplicationDbContext + EF Core)
//     → QuantityMeasurementAppModelLayer      (owns QuantityModel, DTOs, entities, enums, validations)
// ─────────────────────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);

// ── 1. Controllers + model validation ────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

// ── 2. Swagger / OpenAPI with Bearer auth button ──────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "Quantity Measurement API",
        Version     = "v1",
        Description =
            "UC18 REST API — register / login to receive a JWT token, then click " +
            "the Authorize 🔒 button and paste your token to access protected endpoints."
    });

    // Bearer token input in Swagger UI
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        Scheme       = "bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  =
            "Paste your JWT token here (without the 'Bearer' prefix — Swagger adds it automatically)."
    });

    // Apply Bearer requirement globally to all endpoints
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            new List<string>()
        }
    });

    // Wire up XML doc comments from .csproj GenerateDocumentationFile flag
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

// ── 3. EF Core DbContext + repository selection ───────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (!string.IsNullOrWhiteSpace(connectionString))
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(
            connectionString,
            sqlOpts => {
                sqlOpts.MigrationsAssembly("QuantityMeasurementRepoLayer");
                sqlOpts.EnableRetryOnFailure(
                    maxRetryCount:     3,
                    maxRetryDelay:     TimeSpan.FromSeconds(5),
                    errorNumbersToAdd: null);
            }));

    Console.WriteLine("[Program] SQL Server connection string found — using EF Core repositories.");

    builder.Services.AddScoped<IQuantityMeasurementRepository,
                               EFCoreQuantityMeasurementRepository>();

    // UC18: EF Core user repository (SQL Server only — cache has no user concept)
    builder.Services.AddScoped<IUserRepository, EFCoreUserRepository>();
}
else
{
    Console.WriteLine("[Program] No connection string — using in-memory Cache repository.");

    builder.Services.AddSingleton<IQuantityMeasurementRepository>(
        _ => QuantityMeasurementCacheRepository.GetInstance());

    // Cache repo fallback: register a no-op user repository so DI resolves
    // (auth endpoints will fail gracefully without a real DB)
    builder.Services.AddScoped<IUserRepository, EFCoreUserRepository>();
}

// ── 4. Service layer ──────────────────────────────────────────────────────────
builder.Services.AddScoped<IQuantityMeasurementService, QuantityMeasurementServiceImpl>();

// ── 5. UC18: Auth + security services ────────────────────────────────────────
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<EncryptionService>();
builder.Services.AddScoped<HashingService>();

// ── 6. UC18: JWT authentication middleware ────────────────────────────────────
string jwtSecretKey = builder.Configuration["Jwt:SecretKey"]
    ?? throw new InvalidOperationException("Jwt:SecretKey is missing from configuration.");
string jwtIssuer    = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("Jwt:Issuer is missing from configuration.");
string jwtAudience  = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("Jwt:Audience is missing from configuration.");

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidIssuer              = jwtIssuer,
            ValidateAudience         = true,
            ValidAudience            = jwtAudience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey         = new SymmetricSecurityKey(
                                           Encoding.UTF8.GetBytes(jwtSecretKey)),
            ValidateLifetime         = true,
            ClockSkew                = TimeSpan.Zero   // no grace period on expiry
        };

        // Return a clean JSON 401 instead of ASP.NET Core's default HTML challenge
        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();
                context.Response.StatusCode  = 401;
                context.Response.ContentType = "application/json";

                var body = System.Text.Json.JsonSerializer.Serialize(new
                {
                    statusCode    = 401,
                    message       = "You must be logged in. Register at POST /api/auth/register " +
                                    "or login at POST /api/auth/login.",
                    exceptionType = "UnauthorizedAccessException",
                    timestamp     = DateTime.UtcNow
                });

                await context.Response.WriteAsync(body);
            }
        };
    });

builder.Services.AddAuthorization();

// ── 7. CORS ───────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

// ── Build ─────────────────────────────────────────────────────────────────────
var app = builder.Build();

// ── 8. Auto-apply EF Core migrations on startup ───────────────────────────────
if (!string.IsNullOrWhiteSpace(connectionString))
{
    try
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.Migrate();
        Console.WriteLine("[Program] EF Core migrations applied (Users table + UserId column).");
    }
    catch (Exception ex)
    {
        Console.WriteLine("[Program] WARNING: migrations failed — " + ex.Message);
        Console.WriteLine("[Program] Ensure SQL Server is running and the database exists.");
    }
}

// ── 9. Swagger UI (Development only) ─────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Quantity Measurement API v1");
        c.RoutePrefix = "swagger";
    });
}

// ── 10. Middleware pipeline ───────────────────────────────────────────────────
// Order matters — Authentication must come before Authorization
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
app.UseAuthentication();        // UC18: validate JWT token on every request
app.UseAuthorization();         // UC18: enforce [Authorize] on protected endpoints
app.MapControllers();

Console.WriteLine("[Program] Quantity Measurement API (UC18) is running.");
Console.WriteLine("[Program] Swagger UI  → http://localhost:5000/swagger");
Console.WriteLine("[Program] Register    → POST http://localhost:5000/api/auth/register");
Console.WriteLine("[Program] Login       → POST http://localhost:5000/api/auth/login");

app.Run();
