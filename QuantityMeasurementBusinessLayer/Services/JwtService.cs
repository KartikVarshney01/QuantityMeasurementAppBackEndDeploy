using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using QuantityMeasurementAppModelLayer.Entities;

namespace QuantityMeasurementAppBusinessLayer.Services;

/// <summary>
/// UC18: Generates and reads signed JWT tokens.
/// Configuration is read from appsettings.json under the <c>Jwt</c> section:
/// <list type="bullet">
///   <item><c>Jwt:SecretKey</c> — HMAC-SHA256 signing key (keep secret, min 32 chars)</item>
///   <item><c>Jwt:Issuer</c>    — token issuer claim</item>
///   <item><c>Jwt:Audience</c>  — token audience claim</item>
///   <item><c>Jwt:ExpiryMinutes</c> — token lifetime in minutes</item>
/// </list>
/// </summary>
public class JwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    // ── Generate ──────────────────────────────────────────────────────

    /// <summary>
    /// Builds and signs a JWT token for the given user.
    /// Claims included: sub (userId), email, jti (unique token ID), name, userId.
    /// </summary>
    // public string GenerateToken(UserEntity user)
    // {
    //     string secretKey      = _configuration["Jwt:SecretKey"]!;
    //     string issuer         = _configuration["Jwt:Issuer"]!;
    //     string audience       = _configuration["Jwt:Audience"]!;
    //     int    expiryMinutes  = int.Parse(_configuration["Jwt:ExpiryMinutes"]!);

    //     // Build the signing key from the secret string
    //     var signingKey  = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
    //     var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

    //     // Claims embedded in the token payload
    //     // userId claim is read by the controller to scope history queries
    //     var claims = new List<Claim>
    //     {
    //         new(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
    //         new(JwtRegisteredClaimNames.Email, user.Email),
    //         new(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
    //         new("name",   user.Name),
    //         new("userId", user.Id.ToString())
    //     };

    //     var token = new JwtSecurityToken(
    //         issuer:             issuer,
    //         audience:           audience,
    //         claims:             claims,
    //         expires:            DateTime.UtcNow.AddMinutes(expiryMinutes),
    //         signingCredentials: credentials);

    //     return new JwtSecurityTokenHandler().WriteToken(token);
    // }

    public string GenerateToken(UserEntity user)
    {
        var secretKey = _configuration.GetValue<string>("Jwt:SecretKey");
        var issuer = _configuration.GetValue<string>("Jwt:Issuer");
        var audience = _configuration.GetValue<string>("Jwt:Audience");
        var expiryStr = _configuration.GetValue<string>("Jwt:ExpiryMinutes");

        // ✅ SAFETY CHECK (CRITICAL)
        if (string.IsNullOrEmpty(secretKey))
            throw new Exception("JWT SecretKey is missing!");

        if (string.IsNullOrEmpty(issuer))
            throw new Exception("JWT Issuer is missing!");

        if (string.IsNullOrEmpty(audience))
            throw new Exception("JWT Audience is missing!");

        if (string.IsNullOrEmpty(expiryStr))
            throw new Exception("JWT ExpiryMinutes is missing!");

        int expiryMinutes = int.Parse(expiryStr);

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
    {
        new(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
        new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
        new(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
        new("name",   user.Name ?? ""),
        new("userId", user.Id.ToString())
    };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    // ── Helpers ───────────────────────────────────────────────────────

    /// <summary>Returns the configured token lifetime converted to seconds.</summary>
    public int GetExpirySeconds()
        => int.Parse(_configuration["Jwt:ExpiryMinutes"]!) * 60;
}
