using QuantityMeasurementAppModelLayer.DTOs;

namespace QuantityMeasurementAppBusinessLayer.Interfaces;

/// <summary>
/// UC18 authentication service contract.
/// Implemented by <c>AuthService</c> which uses BCrypt for password
/// hashing and <c>JwtService</c> for token issuance.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registers a new user account.
    /// Throws <see cref="InvalidOperationException"/> if the email is already registered.
    /// </summary>
    /// <param name="request">Name, email, and plain-text password.</param>
    /// <returns><see cref="AuthResponse"/> containing a signed JWT token.</returns>
    AuthResponse Register(RegisterRequest request);

    /// <summary>
    /// Authenticates an existing user with email and password.
    /// Throws <see cref="UnauthorizedAccessException"/> on bad credentials.
    /// </summary>
    /// <param name="request">Email and plain-text password.</param>
    /// <returns><see cref="AuthResponse"/> containing a signed JWT token.</returns>
    AuthResponse Login(LoginRequest request);
}
