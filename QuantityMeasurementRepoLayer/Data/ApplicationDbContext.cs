using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;
using QuantityMeasurementAppModelLayer.Models;
using QuantityMeasurementAppModelLayer.Entities;

namespace QuantityMeasurementAppRepoLayer.Data;

/// <summary>
/// EF Core DbContext for the Quantity Measurement application.
/// <para>
/// UC18 additions:
/// <list type="bullet">
///   <item><c>DbSet&lt;UserEntity&gt; Users</c> — the new Users table.</item>
///   <item><c>UserId</c> column on QuantityMeasurements — FK to Users, indexed.</item>
/// </list>
/// </para>
/// <para>
/// <b>Migration commands (run from solution root):</b><br/>
/// <c>dotnet ef migrations add AddUsersAndUserId
///     --project QuantityMeasurementRepoLayer
///     --startup-project QuantityMeasurementAPI</c><br/>
/// <c>dotnet ef database update
///     --project QuantityMeasurementRepoLayer
///     --startup-project QuantityMeasurementAPI</c>
/// </para>
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    // ── DbSets ────────────────────────────────────────────────────────

    /// <summary>The QuantityMeasurements table.</summary>
    public DbSet<QuantityMeasurementEntity> QuantityMeasurements => Set<QuantityMeasurementEntity>();

    /// <summary>UC18: The Users table.</summary>
    public DbSet<UserEntity> Users => Set<UserEntity>();

    // ── Model configuration ───────────────────────────────────────────

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var jsonOpts = new JsonSerializerOptions { WriteIndented = false };

        // ════════════════════════════════════════════════════════════
        // Users table (UC18)
        // ════════════════════════════════════════════════════════════
        var user = modelBuilder.Entity<UserEntity>();

        user.ToTable("Users");
        user.HasKey(u => u.Id);

        user.Property(u => u.Id)
            .ValueGeneratedOnAdd()
            .HasColumnName("Id")
            .HasColumnType("bigint");

        user.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("Name")
            .HasColumnType("nvarchar(255)");

        user.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("Email")
            .HasColumnType("nvarchar(255)");

        // Email must be unique — enforced at DB level
        user.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("IX_Users_Email");

        // BCrypt hashes are always 60 chars; allow a little headroom
        user.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("PasswordHash")
            .HasColumnType("nvarchar(100)");

        user.Property(u => u.CreatedAt)
            .HasColumnName("CreatedAt")
            .HasColumnType("datetime2")
            .IsRequired();

        user.Property(u => u.LastLoginAt)
            .HasColumnName("LastLoginAt")
            .HasColumnType("datetime2")
            .IsRequired();

        // ════════════════════════════════════════════════════════════
        // QuantityMeasurements table
        // ════════════════════════════════════════════════════════════
        var entity = modelBuilder.Entity<QuantityMeasurementEntity>();

        entity.ToTable("QuantityMeasurements");
        entity.HasKey(e => e.Id);

        entity.Property(e => e.Id)
              .ValueGeneratedOnAdd()
              .HasColumnName("Id")
              .HasColumnType("int");

        // ── UC18: UserId FK ───────────────────────────────────────────
        entity.Property(e => e.UserId)
              .IsRequired()
              .HasColumnName("UserId")
              .HasColumnType("bigint");

        entity.HasIndex(e => e.UserId)
              .HasDatabaseName("IX_QuantityMeasurements_UserId");

        // ── Operation ─────────────────────────────────────────────────
        entity.Property(e => e.Operation)
              .IsRequired()
              .HasMaxLength(50)
              .HasColumnName("Operation")
              .HasColumnType("nvarchar(50)");

        // ── Operand1 → JSON ───────────────────────────────────────────
        var modelComparer = new ValueComparer<QuantityModel<object>?>(
            (a, b) => a != null && b != null
                      && a.Value == b.Value
                      && Equals(a.Unit, b.Unit),
            c => c == null ? 0 : HashCode.Combine(c.Value, c.Unit),
            c => c == null ? null : new QuantityModel<object>(c.Value, c.Unit));

        entity.Property(e => e.Operand1)
              .HasColumnName("Operand1")
              .HasColumnType("nvarchar(max)")
              .HasConversion(
                  v => v == null ? null : JsonSerializer.Serialize(v, jsonOpts),
                  v => v == null ? null : JsonSerializer.Deserialize<QuantityModel<object>>(v, jsonOpts))
              .Metadata.SetValueComparer(modelComparer);

        // ── Operand2 → JSON ───────────────────────────────────────────
        entity.Property(e => e.Operand2)
              .HasColumnName("Operand2")
              .HasColumnType("nvarchar(max)")
              .HasConversion(
                  v => v == null ? null : JsonSerializer.Serialize(v, jsonOpts),
                  v => v == null ? null : JsonSerializer.Deserialize<QuantityModel<object>>(v, jsonOpts))
              .Metadata.SetValueComparer(modelComparer);

        // ── Result → JSON ─────────────────────────────────────────────
        var objectComparer = new ValueComparer<object?>(
            (a, b) => Equals(a, b),
            c => c == null ? 0 : c.GetHashCode(),
            c => c);

        entity.Property(e => e.Result)
              .HasColumnName("Result")
              .HasColumnType("nvarchar(max)")
              .HasConversion(
                  v => v == null ? null : JsonSerializer.Serialize(v, jsonOpts),
                  v => v == null ? null : JsonSerializer.Deserialize<object>(v, jsonOpts))
              .Metadata.SetValueComparer(objectComparer);

        // ── HasError ──────────────────────────────────────────────────
        entity.Property(e => e.HasError)
              .HasColumnName("HasError")
              .HasColumnType("bit")
              .IsRequired();

        // ── ErrorMessage ──────────────────────────────────────────────
        entity.Property(e => e.ErrorMessage)
              .HasColumnName("ErrorMessage")
              .HasColumnType("nvarchar(500)")
              .HasMaxLength(500)
              .IsRequired(false);

        // ── CreatedAt ─────────────────────────────────────────────────
        entity.Property(e => e.CreatedAt)
              .HasColumnName("CreatedAt")
              .HasColumnType("datetime2")
              .IsRequired();

        // ── Indexes ───────────────────────────────────────────────────
        entity.HasIndex(e => e.CreatedAt)
              .HasDatabaseName("IX_QuantityMeasurements_CreatedAt");

        entity.HasIndex(e => e.Operation)
              .HasDatabaseName("IX_QuantityMeasurements_Operation");

        entity.HasIndex(e => e.HasError)
              .HasDatabaseName("IX_QuantityMeasurements_HasError");
    }
}
