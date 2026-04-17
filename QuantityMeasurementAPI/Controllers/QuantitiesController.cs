using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuantityMeasurementAppBusinessLayer.Interfaces;
using QuantityMeasurementAppModelLayer.DTOs;

namespace QuantityMeasurementAPI.Controllers;

/// <summary>
/// Exposes all Quantity Measurement operations as RESTful HTTP endpoints.
/// <para>
/// UC18: ALL endpoints now require a valid JWT token in the Authorization header.
/// Format: <c>Authorization: Bearer {your_token}</c>
/// Obtain a token from <c>POST /api/auth/register</c> or <c>POST /api/auth/login</c>.
/// </para>
/// <para>
/// The <c>userId</c> is extracted from the JWT claims on every request — it is
/// never read from the request body. This means each user can only read and write
/// their own measurement records.
/// </para>
/// </summary>
[ApiController]
[Route("api/quantities")]
[Produces("application/json")]
[Authorize]
public class QuantitiesController : ControllerBase
{
    private readonly IQuantityMeasurementService   _service;
    private readonly ILogger<QuantitiesController> _logger;

    public QuantitiesController(
        IQuantityMeasurementService service,
        ILogger<QuantitiesController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger  = logger  ?? throw new ArgumentNullException(nameof(logger));
    }

    // ── JWT claim helpers ─────────────────────────────────────────────

    /// <summary>
    /// Reads the authenticated user's ID from the JWT "userId" claim.
    /// This claim is set by <c>JwtService.GenerateToken</c>.
    /// </summary>
    private long GetCurrentUserId()
    {
        string? claim = User.FindFirstValue("userId");
        return claim is not null && long.TryParse(claim, out long id) ? id : 0;
    }

    /// <summary>Reads the authenticated user's email from the JWT "email" claim.</summary>
    private string GetCurrentUserEmail()
        => User.FindFirstValue(JwtRegisteredClaimNames.Email) ?? "unknown";

    /// <summary>Reads the authenticated user's name from the JWT "name" claim.</summary>
    private string GetCurrentUserName()
        => User.FindFirstValue("name") ?? "unknown";

    // ── POST /api/quantities/compare ──────────────────────────────────

    /// <summary>
    /// Compares two quantities of the same measurement category.
    /// Both values are converted to the category's base unit before comparison,
    /// so 1 Feet and 12 Inches are considered equal.
    /// </summary>
    /// <response code="200">Comparison performed successfully.</response>
    /// <response code="400">Validation error or mismatched categories.</response>
    /// <response code="401">Missing or invalid JWT token.</response>
    [HttpPost("compare")]
    [ProducesResponseType(typeof(ApiResponse<ComparisonResponse>),   StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse),                      StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse),                      StatusCodes.Status401Unauthorized)]
    public IActionResult Compare([FromBody] BinaryOperationRequest request)
    {
        long userId = GetCurrentUserId();
        _logger.LogInformation("Compare — user: {UserId}, Q1: {Q1}, Q2: {Q2}",
            userId, request.Q1, request.Q2);

        bool equal = _service.Compare(MapToDTO(request.Q1), MapToDTO(request.Q2), userId);

        var payload = new ComparisonResponse
        {
            AreEqual = equal,
            Message  = equal ? "Quantities are equal." : "Quantities are not equal."
        };

        return Ok(new ApiResponse<ComparisonResponse>(true, "Comparison successful.", payload));
    }

    // ── POST /api/quantities/convert ──────────────────────────────────

    /// <summary>
    /// Converts a quantity from its current unit to the specified target unit.
    /// </summary>
    /// <response code="200">Conversion performed successfully.</response>
    /// <response code="400">Validation error, unknown unit, or category mismatch.</response>
    /// <response code="401">Missing or invalid JWT token.</response>
    [HttpPost("convert")]
    [ProducesResponseType(typeof(ApiResponse<ConversionResponse>),    StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse),                       StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse),                       StatusCodes.Status401Unauthorized)]
    public IActionResult Convert([FromBody] ConversionRequest request)
    {
        long userId = GetCurrentUserId();
        _logger.LogInformation("Convert — user: {UserId}, {Qty} → {Target}",
            userId, request.Quantity, request.TargetUnit);

        var result = _service.Convert(MapToDTO(request.Quantity), request.TargetUnit, userId);

        var payload = new ConversionResponse
        {
            Result   = result.Value,
            Unit     = result.UnitName,
            Category = result.Category
        };

        return Ok(new ApiResponse<ConversionResponse>(true, "Conversion successful.", payload));
    }

    // ── POST /api/quantities/add ──────────────────────────────────────

    /// <summary>
    /// Adds two quantities of the same category. Result is in Q1's unit.
    /// Temperature addition is not supported.
    /// </summary>
    /// <response code="200">Addition performed successfully.</response>
    /// <response code="400">Validation error, mismatched categories, or TEMPERATURE operand.</response>
    /// <response code="401">Missing or invalid JWT token.</response>
    [HttpPost("add")]
    [ProducesResponseType(typeof(ApiResponse<ArithmeticOperationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse),                             StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse),                             StatusCodes.Status401Unauthorized)]
    public IActionResult Add([FromBody] BinaryOperationRequest request)
    {
        long userId = GetCurrentUserId();
        _logger.LogInformation("Add — user: {UserId}, Q1: {Q1}, Q2: {Q2}",
            userId, request.Q1, request.Q2);

        var result  = _service.Add(MapToDTO(request.Q1), MapToDTO(request.Q2), userId);
        var payload = new ArithmeticOperationResponse
        {
            Result   = result.Value,
            Unit     = result.UnitName,
            Category = result.Category
        };

        return Ok(new ApiResponse<ArithmeticOperationResponse>(true, "Addition successful.", payload));
    }

    // ── POST /api/quantities/subtract ─────────────────────────────────

    /// <summary>
    /// Subtracts Q2 from Q1. Result is in Q1's unit.
    /// Temperature subtraction is not supported.
    /// </summary>
    /// <response code="200">Subtraction performed successfully.</response>
    /// <response code="400">Validation error, mismatched categories, or TEMPERATURE operand.</response>
    /// <response code="401">Missing or invalid JWT token.</response>
    [HttpPost("subtract")]
    [ProducesResponseType(typeof(ApiResponse<ArithmeticOperationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse),                             StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse),                             StatusCodes.Status401Unauthorized)]
    public IActionResult Subtract([FromBody] BinaryOperationRequest request)
    {
        long userId = GetCurrentUserId();
        _logger.LogInformation("Subtract — user: {UserId}, Q1: {Q1}, Q2: {Q2}",
            userId, request.Q1, request.Q2);

        var result  = _service.Subtract(MapToDTO(request.Q1), MapToDTO(request.Q2), userId);
        var payload = new ArithmeticOperationResponse
        {
            Result   = result.Value,
            Unit     = result.UnitName,
            Category = result.Category
        };

        return Ok(new ApiResponse<ArithmeticOperationResponse>(
            true, "Subtraction successful.", payload));
    }

    // ── POST /api/quantities/divide ───────────────────────────────────

    /// <summary>
    /// Divides Q1 by Q2. Returns a dimensionless scalar ratio.
    /// Temperature division is not supported. Q2 must not be zero.
    /// </summary>
    /// <response code="200">Division performed successfully.</response>
    /// <response code="400">Validation error, mismatched categories, TEMPERATURE operand, or division by zero.</response>
    /// <response code="401">Missing or invalid JWT token.</response>
    [HttpPost("divide")]
    [ProducesResponseType(typeof(ApiResponse<DivisionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse),                  StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse),                  StatusCodes.Status401Unauthorized)]
    public IActionResult Divide([FromBody] BinaryOperationRequest request)
    {
        long userId = GetCurrentUserId();
        _logger.LogInformation("Divide — user: {UserId}, Q1: {Q1}, Q2: {Q2}",
            userId, request.Q1, request.Q2);

        double result = _service.Divide(MapToDTO(request.Q1), MapToDTO(request.Q2), userId);

        return Ok(new ApiResponse<DivisionResponse>(
            true, "Division successful.", new DivisionResponse { Result = result }));
    }

    // ── GET /api/quantities/history ───────────────────────────────────

    /// <summary>
    /// Returns the full operation history for the authenticated user, newest first.
    /// </summary>
    /// <response code="200">History retrieved successfully.</response>
    /// <response code="401">Missing or invalid JWT token.</response>
    [HttpGet("history")]
    [ProducesResponseType(typeof(ApiResponse<List<OperationHistoryResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse),                                StatusCodes.Status401Unauthorized)]
    public IActionResult GetHistory()
    {
        long userId = GetCurrentUserId();
        _logger.LogInformation("GetHistory — user: {UserId}", userId);

        var history = MapHistory(_service.GetHistory(userId));
        return Ok(new ApiResponse<List<OperationHistoryResponse>>(
            true, "History retrieved successfully.", history));
    }

    // ── GET /api/quantities/history/operation/{operation} ─────────────

    /// <summary>
    /// Returns history filtered by operation type for the authenticated user.
    /// Valid values: Compare, Convert, Add, Subtract, Divide (case-insensitive).
    /// </summary>
    /// <response code="200">History retrieved successfully.</response>
    /// <response code="401">Missing or invalid JWT token.</response>
    [HttpGet("history/operation/{operation}")]
    [ProducesResponseType(typeof(ApiResponse<List<OperationHistoryResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse),                                StatusCodes.Status401Unauthorized)]
    public IActionResult GetByOperation(string operation)
    {
        long userId = GetCurrentUserId();
        _logger.LogInformation("GetByOperation — user: {UserId}, operation: {Op}", userId, operation);

        var history = MapHistory(_service.GetByOperation(operation, userId));
        return Ok(new ApiResponse<List<OperationHistoryResponse>>(
            true, $"History for operation '{operation}' retrieved.", history));
    }

    // ── GET /api/quantities/history/type/{measurementType} ────────────

    /// <summary>
    /// Returns history filtered by measurement category for the authenticated user.
    /// Valid values: LENGTH, WEIGHT, VOLUME, TEMPERATURE (case-insensitive).
    /// </summary>
    /// <response code="200">History retrieved successfully.</response>
    /// <response code="401">Missing or invalid JWT token.</response>
    [HttpGet("history/type/{measurementType}")]
    [ProducesResponseType(typeof(ApiResponse<List<OperationHistoryResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse),                                StatusCodes.Status401Unauthorized)]
    public IActionResult GetByMeasurementType(string measurementType)
    {
        long userId = GetCurrentUserId();
        _logger.LogInformation("GetByMeasurementType — user: {UserId}, type: {Type}",
            userId, measurementType);

        var history = MapHistory(_service.GetByMeasurementType(measurementType, userId));
        return Ok(new ApiResponse<List<OperationHistoryResponse>>(
            true, $"History for type '{measurementType}' retrieved.", history));
    }

    // ── GET /api/quantities/count ─────────────────────────────────────

    /// <summary>
    /// Returns the total number of operations performed by the authenticated user.
    /// </summary>
    /// <response code="200">Count retrieved successfully.</response>
    /// <response code="401">Missing or invalid JWT token.</response>
    [HttpGet("count")]
    [ProducesResponseType(typeof(ApiResponse<CountResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse),               StatusCodes.Status401Unauthorized)]
    public IActionResult GetCount()
    {
        long userId = GetCurrentUserId();
        _logger.LogInformation("GetCount — user: {UserId}", userId);

        int count = _service.GetCount(userId);
        return Ok(new ApiResponse<CountResponse>(
            true, "Count retrieved successfully.",
            new CountResponse { TotalOperations = count }));
    }

    // ── GET /api/quantities/me ────────────────────────────────────────

    /// <summary>
    /// UC18: Returns the identity of the currently authenticated user,
    /// extracted directly from the JWT claims. Useful for verifying your token.
    /// </summary>
    /// <response code="200">User info returned.</response>
    /// <response code="401">Missing or invalid JWT token.</response>
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public IActionResult WhoAmI()
    {
        _logger.LogInformation("WhoAmI called — user: {UserId}", GetCurrentUserId());
        return Ok(new
        {
            userId    = GetCurrentUserId(),
            email     = GetCurrentUserEmail(),
            name      = GetCurrentUserName(),
            timestamp = DateTime.UtcNow
        });
    }

    // ── GET /api/quantities/health ────────────────────────────────────

    /// <summary>
    /// Simple health-check. No token required.
    /// </summary>
    /// <response code="200">API is running.</response>
    [HttpGet("health")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        _logger.LogInformation("Health check called.");
        return Ok(new { status = "Quantity Measurement API is running.", timestamp = DateTime.UtcNow });
    }

    // ── Private helpers ───────────────────────────────────────────────

    private static QuantityDTO MapToDTO(QuantityRequest r)
        => new(r.Value, r.UnitName, r.Category);

    private static List<OperationHistoryResponse> MapHistory(
        List<QuantityMeasurementAppModelLayer.Entities.QuantityMeasurementEntity> entities)
        => entities.Select(h => new OperationHistoryResponse
        {
            Id           = h.Id,
            Operation    = h.Operation,
            Operand1     = h.Operand1,
            Operand2     = h.Operand2,
            Result       = h.Result,
            HasError     = h.HasError,
            ErrorMessage = h.ErrorMessage,
            CreatedAt    = h.CreatedAt
        }).ToList();
}
