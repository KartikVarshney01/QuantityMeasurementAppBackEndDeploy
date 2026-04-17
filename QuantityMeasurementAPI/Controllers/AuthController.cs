using Microsoft.AspNetCore.Mvc;
using QuantityMeasurementAppBusinessLayer.Interfaces;
using QuantityMeasurementAppModelLayer.DTOs;

namespace QuantityMeasurementAPI.Controllers;

/// <summary>
/// UC18: Authentication endpoints.
/// Register a new account or login with an existing one to receive a JWT token.
/// Use the returned token in the <c>Authorization: Bearer {token}</c> header
/// for all protected endpoints under <c>/api/quantities</c>.
/// </summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService              _authService;
    private readonly ILogger<AuthController>   _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _logger      = logger      ?? throw new ArgumentNullException(nameof(logger));
    }

    // ── POST /api/auth/register ───────────────────────────────────────

    /// <summary>
    /// Register a new user account with name, email, and password.
    /// </summary>
    /// <remarks>
    /// Example request:
    ///
    ///     POST /api/auth/register
    ///     {
    ///         "name":     "Jane Smith",
    ///         "email":    "jane@example.com",
    ///         "password": "secret123"
    ///     }
    ///
    /// On success, copy the <c>token</c> from the response and click the
    /// Swagger <b>Authorize 🔒</b> button to authenticate all further requests.
    /// </remarks>
    /// <param name="request">Name, email, and plain-text password (min 6 chars).</param>
    /// <returns><see cref="AuthResponse"/> containing the signed JWT token.</returns>
    /// <response code="200">Account created — JWT token returned.</response>
    /// <response code="400">Validation failed (missing fields, bad email format, short password).</response>
    /// <response code="409">Email is already registered.</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public IActionResult Register([FromBody] RegisterRequest request)
    {
        _logger.LogInformation("Register called — email: {Email}", request.Email);
        AuthResponse response = _authService.Register(request);
        return Ok(response);
    }

    // ── POST /api/auth/login ──────────────────────────────────────────

    /// <summary>
    /// Login with an existing email and password to receive a fresh JWT token.
    /// </summary>
    /// <remarks>
    /// Example request:
    ///
    ///     POST /api/auth/login
    ///     {
    ///         "email":    "jane@example.com",
    ///         "password": "secret123"
    ///     }
    ///
    /// On success, copy the <c>token</c> and use it in
    /// <c>Authorization: Bearer {token}</c> for all protected endpoints.
    /// </remarks>
    /// <param name="request">Registered email and plain-text password.</param>
    /// <returns><see cref="AuthResponse"/> containing the signed JWT token.</returns>
    /// <response code="200">Login successful — JWT token returned.</response>
    /// <response code="400">Validation failed (missing fields, bad email format).</response>
    /// <response code="401">Invalid email or password.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        _logger.LogInformation("Login called — email: {Email}", request.Email);
        AuthResponse response = _authService.Login(request);
        return Ok(response);
    }

    // ── GET /api/auth/ping ────────────────────────────────────────────

    /// <summary>
    /// Health-check for the auth controller. No token required.
    /// </summary>
    /// <response code="200">Auth controller is reachable.</response>
    [HttpGet("ping")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Ping()
    {
        _logger.LogInformation("Auth ping called.");
        return Ok(new
        {
            message   = "Auth controller is running.",
            register  = "POST /api/auth/register",
            login     = "POST /api/auth/login",
            timestamp = DateTime.UtcNow
        });
    }
}
