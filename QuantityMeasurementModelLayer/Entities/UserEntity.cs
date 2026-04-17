using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuantityMeasurementAppModelLayer.Entities;

/// <summary>
/// EF Core entity that maps to the <c>Users</c> table.
/// Passwords are NEVER stored as plain text — only the BCrypt hash is persisted.
/// </summary>
[Table("Users")]
public class UserEntity
{
    // ── Primary Key ───────────────────────────────────────────────────

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("Id")]
    public long Id { get; set; }

    // ── Identity fields ───────────────────────────────────────────────

    /// <summary>Display name provided at registration.</summary>
    [Required]
    [MaxLength(255)]
    [Column("Name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Email address — must be unique across all users.
    /// Used as the login identifier.
    /// </summary>
    [Required]
    [MaxLength(255)]
    [Column("Email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// BCrypt hash of the user's password.
    /// BCrypt hashes are always 60 characters long.
    /// </summary>
    [Required]
    [MaxLength(100)]
    [Column("PasswordHash")]
    public string PasswordHash { get; set; } = string.Empty;

    // ── Audit ─────────────────────────────────────────────────────────

    /// <summary>UTC timestamp when the account was created.</summary>
    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>UTC timestamp of the most recent successful login.</summary>
    [Column("LastLoginAt")]
    public DateTime LastLoginAt { get; set; } = DateTime.UtcNow;
}
