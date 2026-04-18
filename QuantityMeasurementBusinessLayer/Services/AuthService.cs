using QuantityMeasurementAppBusinessLayer.Interfaces;
using QuantityMeasurementAppModelLayer.DTOs;
using QuantityMeasurementAppModelLayer.Entities;
using QuantityMeasurementAppRepoLayer.Interfaces;

namespace QuantityMeasurementAppBusinessLayer.Services;

/// <summary>
/// UC18: Handles user registration and login.
/// <list type="bullet">
///   <item>Registration: hashes the password with BCrypt and saves the user; issues a JWT.</item>
///   <item>Login: verifies the BCrypt hash, updates LastLoginAt, and issues a JWT.</item>
/// </list>
/// All tokens are signed by <see cref="JwtService"/>; passwords never leave this
/// class in plain-text form.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly JwtService _jwtService;

    public AuthService(IUserRepository userRepository, JwtService jwtService)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
    }

    // ── Register ──────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new account.
    /// Throws <see cref="InvalidOperationException"/> when the email is already taken.
    /// </summary>
    public AuthResponse Register(RegisterRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ArgumentException("Email cannot be null");

        if (string.IsNullOrWhiteSpace(request.Password))
            throw new ArgumentException("Password cannot be null");

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Name cannot be null");

        // Reject duplicate email before doing any hashing work
        UserEntity? existing = _userRepository.FindByEmail(request.Email);
        if (existing != null)
            throw new InvalidOperationException(
                "This email is already registered. Please login instead.");

        // BCrypt hash — never store plain-text passwords
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var newUser = new UserEntity
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow
        };

        _userRepository.Save(newUser);

        string token = _jwtService.GenerateToken(newUser);
        return BuildResponse(newUser, token);
    }

    // ── Login ─────────────────────────────────────────────────────────

    /// <summary>
    /// Authenticates an existing user.
    /// Throws <see cref="UnauthorizedAccessException"/> on bad email or password.
    /// </summary>
    public AuthResponse Login(LoginRequest request)
    {
        UserEntity? user = _userRepository.FindByEmail(request.Email);

        // Use the same error message for both "email not found" and "wrong password"
        // to avoid leaking which emails are registered (security best practice)
        if (user == null)
            throw new UnauthorizedAccessException("Invalid email or password.");

        bool passwordMatch = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!passwordMatch)
            throw new UnauthorizedAccessException("Invalid email or password.");

        // Keep last-login timestamp up to date
        user.LastLoginAt = DateTime.UtcNow;
        _userRepository.Update(user);

        string token = _jwtService.GenerateToken(user);
        return BuildResponse(user, token);
    }

    // ── Private ───────────────────────────────────────────────────────

    private AuthResponse BuildResponse(UserEntity user, string token) =>
        new AuthResponse
        {
            Token = token,
            TokenType = "Bearer",
            ExpiresIn = _jwtService.GetExpirySeconds(),
            UserId = user.Id,
            Email = user.Email,
            Name = user.Name,
            IssuedAt = DateTime.UtcNow.ToString("o")
        };
}
