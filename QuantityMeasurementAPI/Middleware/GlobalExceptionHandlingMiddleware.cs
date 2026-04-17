using System.Net;
using System.Text.Json;
using QuantityMeasurementAppBusinessLayer.Exceptions;
using QuantityMeasurementAppModelLayer.DTOs;

namespace QuantityMeasurementAPI.Middleware;

/// <summary>
/// UC18 global exception handler middleware.
/// Catches every unhandled exception that propagates out of the MVC pipeline and
/// returns a standardised <see cref="ErrorResponse"/> JSON payload with the
/// correct HTTP status code.
/// <para>
/// UC18 additions:
/// <list type="bullet">
///   <item><see cref="UnauthorizedAccessException"/> → HTTP 401 (bad credentials)</item>
///   <item><see cref="InvalidOperationException"/>   → HTTP 409 (email already registered)</item>
/// </list>
/// </para>
/// </summary>
public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate                             _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware>  _logger;

    public GlobalExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        _next   = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new ErrorResponse
        {
            Timestamp     = DateTime.UtcNow,
            ExceptionType = exception.GetType().Name
        };

        switch (exception)
        {
            // ── Domain validation error → 400 ─────────────────────────
            case QuantityMeasurementException qEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.StatusCode         = (int)HttpStatusCode.BadRequest;
                response.Message            = qEx.Message;
                break;

            // ── UC18: Bad credentials → 401 ───────────────────────────
            case UnauthorizedAccessException uEx:
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response.StatusCode         = (int)HttpStatusCode.Unauthorized;
                response.Message            = uEx.Message;
                break;

            // ── UC18: Duplicate email on register → 409 ───────────────
            case InvalidOperationException ioEx:
                context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                response.StatusCode         = (int)HttpStatusCode.Conflict;
                response.Message            = ioEx.Message;
                break;

            // ── Null / bad argument → 400 ─────────────────────────────
            case ArgumentNullException anEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.StatusCode         = (int)HttpStatusCode.BadRequest;
                response.Message            = "Invalid input: " + anEx.Message;
                break;

            case ArgumentException aEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.StatusCode         = (int)HttpStatusCode.BadRequest;
                response.Message            = "Invalid argument: " + aEx.Message;
                break;

            case NotSupportedException nsEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.StatusCode         = (int)HttpStatusCode.BadRequest;
                response.Message            = nsEx.Message;
                break;

            // ── Catch-all → 500 ───────────────────────────────────────
            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.StatusCode         = (int)HttpStatusCode.InternalServerError;
                response.Message            = "An unexpected error occurred. Please try again later.";
                break;
        }

        var json = JsonSerializer.Serialize(response,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        return context.Response.WriteAsync(json);
    }
}
