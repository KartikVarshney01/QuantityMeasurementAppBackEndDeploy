namespace QuantityMeasurementAppBusinessLayer.Exceptions;

/// <summary>
/// Thrown when EF Core or any database-layer operation fails unexpectedly.
/// Mapped to HTTP 500 Internal Server Error by <c>GlobalExceptionHandlingMiddleware</c>.
/// </summary>
// This is a special error message we use if something goes wrong with our database.
public class DatabaseException : Exception
{
    public DatabaseException(string message) : base(message) { }

    public DatabaseException(string message, Exception inner)
        : base(message, inner) { }
}
