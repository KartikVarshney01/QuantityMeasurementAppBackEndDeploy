namespace QuantityMeasurementAppModelLayer.DTOs;

// ─────────────────────────────────────────────────────────────────────────────
// UC17: Response DTOs live in ModelLayer alongside request DTOs so the same
// types can be used by the Web API and the console app without a dependency on
// the API project.
// ─────────────────────────────────────────────────────────────────────────────

// ── Generic API envelope ──────────────────────────────────────────────────────

/// <summary>
/// Standard response wrapper used for every successful API response.
/// Provides a consistent shape: <c>success</c>, <c>message</c>, <c>data</c>, <c>timestamp</c>.
/// </summary>
/// <typeparam name="T">The payload type.</typeparam>
// This is a standard way we send back results to the user.
public class ApiResponse<T>
{
    /// <summary>Indicates whether the operation succeeded.</summary>
    public bool Success { get; set; }

    /// <summary>Human-readable status message.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Operation-specific payload.</summary>
    public T? Data { get; set; }

    /// <summary>UTC timestamp of the response.</summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public ApiResponse() { }

    public ApiResponse(bool success, string message, T? data = default)
    {
        Success = success;
        Message = message;
        Data    = data;
    }
}

// ── Error envelope ────────────────────────────────────────────────────────────

/// <summary>
/// Returned by <c>GlobalExceptionHandlingMiddleware</c> for all unhandled errors.
/// Maps exception type to an appropriate HTTP status code.
/// </summary>
public class ErrorResponse
{
    /// <summary>HTTP status code (e.g. 400, 500).</summary>
    public int StatusCode { get; set; }

    /// <summary>Human-readable error message.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>CLR type name of the exception (for debugging).</summary>
    public string ExceptionType { get; set; } = string.Empty;

    /// <summary>UTC timestamp of the error.</summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

// ── Operation-specific payloads ───────────────────────────────────────────────

/// <summary>Payload for Compare operations.</summary>
public class ComparisonResponse
{
    /// <summary>True when both quantities are equal in base units.</summary>
    public bool AreEqual { get; set; }

    /// <summary>Human-readable equality verdict.</summary>
    public string Message { get; set; } = string.Empty;
}

/// <summary>Payload for Convert operations.</summary>
public class ConversionResponse
{
    /// <summary>Converted numeric value.</summary>
    public double Result { get; set; }

    /// <summary>Target unit name.</summary>
    public string Unit { get; set; } = string.Empty;

    /// <summary>Measurement category of the result.</summary>
    public string Category { get; set; } = string.Empty;
}

/// <summary>Payload for Add and Subtract operations.</summary>
public class ArithmeticOperationResponse
{
    /// <summary>Result value expressed in Q1's unit.</summary>
    public double Result { get; set; }

    /// <summary>Unit of the result (same as Q1's unit).</summary>
    public string Unit { get; set; } = string.Empty;

    /// <summary>Measurement category of the result.</summary>
    public string Category { get; set; } = string.Empty;
}

/// <summary>Payload for Divide operations — returns a dimensionless scalar.</summary>
public class DivisionResponse
{
    /// <summary>Scalar ratio of Q1 to Q2 (base-unit values).</summary>
    public double Result { get; set; }
}

/// <summary>Payload for history list endpoints.</summary>
public class OperationHistoryResponse
{
    public int      Id           { get; set; }
    public string   Operation    { get; set; } = string.Empty;
    public object?  Operand1     { get; set; }
    public object?  Operand2     { get; set; }
    public object?  Result       { get; set; }
    public bool     HasError     { get; set; }
    public string?  ErrorMessage { get; set; }
    public DateTime CreatedAt    { get; set; }
}

/// <summary>Payload for the count endpoint.</summary>
public class CountResponse
{
    /// <summary>Total number of measurement operations persisted so far.</summary>
    public int TotalOperations { get; set; }
}
