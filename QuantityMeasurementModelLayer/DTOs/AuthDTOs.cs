using System.ComponentModel.DataAnnotations;

namespace QuantityMeasurementAppModelLayer.DTOs;

// ─────────────────────────────────────────────────────────────────────────────
// UC18: Authentication request and response DTOs.
// Shared in ModelLayer so they are available to both the Web API and any
// future consumer without taking a dependency on the business or API layer.
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Request body for POST /api/auth/register.
/// </summary>
public class RegisterRequest
{
    /// <summary>Display name shown in JWT claims and history.</summary>
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(255, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 255 characters.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Email address — must be unique across all accounts.</summary>
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters.")]
    public string Email { get; set; } = string.Empty;

    /// <summary>Plain-text password — BCrypt-hashed before storage, never logged.</summary>
    [Required(ErrorMessage = "Password is required.")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    public string Password { get; set; } = string.Empty;
}

// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Request body for POST /api/auth/login.
/// </summary>
public class LoginRequest
{
    /// <summary>Registered email address.</summary>
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; } = string.Empty;

    /// <summary>Plain-text password — verified against the stored BCrypt hash.</summary>
    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; } = string.Empty;
}

// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Returned by both register and login on success.
/// The <c>Token</c> field must be included in the
/// <c>Authorization: Bearer {Token}</c> header for all protected endpoints.
/// </summary>
public class AuthResponse
{
    /// <summary>Signed JWT token — include this in Authorization: Bearer header.</summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>Always "Bearer".</summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>Token lifetime in seconds (from config Jwt:ExpiryMinutes × 60).</summary>
    public int ExpiresIn { get; set; }

    /// <summary>Database ID of the authenticated user.</summary>
    public long UserId { get; set; }

    /// <summary>Email of the authenticated user.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Display name of the authenticated user.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>ISO-8601 UTC timestamp of token issuance.</summary>
    public string IssuedAt { get; set; } = string.Empty;
}
