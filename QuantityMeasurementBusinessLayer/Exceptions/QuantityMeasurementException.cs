namespace QuantityMeasurementAppBusinessLayer.Exceptions;

/// <summary>
/// Domain exception thrown by the service and engine layers when a
/// quantity measurement operation cannot be completed.
/// Mapped to HTTP 400 Bad Request by <c>GlobalExceptionHandlingMiddleware</c>.
/// </summary>
// This is a special error message for any problems related specifically to measurements.
public class QuantityMeasurementException : Exception
{
    public QuantityMeasurementException(string message) : base(message) { }

    public QuantityMeasurementException(string message, Exception inner)
        : base(message, inner) { }
}
