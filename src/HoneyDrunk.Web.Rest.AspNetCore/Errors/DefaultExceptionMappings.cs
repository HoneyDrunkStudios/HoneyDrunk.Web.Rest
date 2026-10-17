using HoneyDrunk.Web.Rest.Abstractions.Errors;
using System.Net;

namespace HoneyDrunk.Web.Rest.AspNetCore.Errors;

/// <summary>
/// Provides static helper methods for common exception-to-error mappings.
/// </summary>
public static class DefaultExceptionMappings
{
    /// <summary>
    /// Creates an <see cref="ApiErrorResponse"/> for a bad request.
    /// </summary>
    /// <param name="correlationId">The correlation ID.</param>
    /// <param name="message">The error message.</param>
    /// <param name="traceId">The optional trace ID.</param>
    /// <returns>An <see cref="ApiErrorResponse"/> for bad request.</returns>
    public static ApiErrorResponse BadRequest(string correlationId, string message, string? traceId = null)
    {
        return ApiErrorResponse.Create(correlationId, message, ApiErrorCode.BadRequest, traceId);
    }

    /// <summary>
    /// Creates an <see cref="ApiErrorResponse"/> for a not found error.
    /// </summary>
    /// <param name="correlationId">The correlation ID.</param>
    /// <param name="message">The error message.</param>
    /// <param name="traceId">The optional trace ID.</param>
    /// <returns>An <see cref="ApiErrorResponse"/> for not found.</returns>
    public static ApiErrorResponse NotFound(string correlationId, string? message = null, string? traceId = null)
    {
        return ApiErrorResponse.CreateNotFound(correlationId, message ?? "The requested resource was not found.", traceId);
    }

    /// <summary>
    /// Creates an <see cref="ApiErrorResponse"/> for an unauthorized error.
    /// </summary>
    /// <param name="correlationId">The correlation ID.</param>
    /// <param name="message">The error message.</param>
    /// <param name="traceId">The optional trace ID.</param>
    /// <returns>An <see cref="ApiErrorResponse"/> for unauthorized.</returns>
    public static ApiErrorResponse Unauthorized(string correlationId, string? message = null, string? traceId = null)
    {
        return ApiErrorResponse.CreateUnauthorized(correlationId, message ?? "Authentication is required.", traceId);
    }

    /// <summary>
    /// Creates an <see cref="ApiErrorResponse"/> for a forbidden error.
    /// </summary>
    /// <param name="correlationId">The correlation ID.</param>
    /// <param name="message">The error message.</param>
    /// <param name="traceId">The optional trace ID.</param>
    /// <returns>An <see cref="ApiErrorResponse"/> for forbidden.</returns>
    public static ApiErrorResponse Forbidden(string correlationId, string? message = null, string? traceId = null)
    {
        return ApiErrorResponse.CreateForbidden(correlationId, message ?? "You do not have permission to access this resource.", traceId);
    }

    /// <summary>
    /// Creates an <see cref="ApiErrorResponse"/> for a conflict error.
    /// </summary>
    /// <param name="correlationId">The correlation ID.</param>
    /// <param name="message">The error message.</param>
    /// <param name="traceId">The optional trace ID.</param>
    /// <returns>An <see cref="ApiErrorResponse"/> for conflict.</returns>
    public static ApiErrorResponse Conflict(string correlationId, string message, string? traceId = null)
    {
        return ApiErrorResponse.Create(correlationId, message, ApiErrorCode.Conflict, traceId);
    }

    /// <summary>
    /// Creates an <see cref="ApiErrorResponse"/> for an internal server error.
    /// </summary>
    /// <param name="correlationId">The correlation ID.</param>
    /// <param name="message">The error message.</param>
    /// <param name="traceId">The optional trace ID.</param>
    /// <returns>An <see cref="ApiErrorResponse"/> for internal error.</returns>
    public static ApiErrorResponse InternalError(string correlationId, string? message = null, string? traceId = null)
    {
        return ApiErrorResponse.CreateInternalError(correlationId, message ?? "An internal server error occurred.", traceId);
    }

    /// <summary>
    /// Maps an HTTP status code to the appropriate error code.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <returns>The corresponding error code.</returns>
#pragma warning disable IDE0072 // Populate switch - not all status codes need explicit mapping
    public static string StatusCodeToErrorCode(HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            HttpStatusCode.BadRequest => ApiErrorCode.BadRequest,
            HttpStatusCode.Unauthorized => ApiErrorCode.Unauthorized,
            HttpStatusCode.Forbidden => ApiErrorCode.Forbidden,
            HttpStatusCode.NotFound => ApiErrorCode.NotFound,
            HttpStatusCode.Conflict => ApiErrorCode.Conflict,
            HttpStatusCode.NotImplemented => ApiErrorCode.NotImplemented,
            HttpStatusCode.ServiceUnavailable => ApiErrorCode.ServiceUnavailable,
            _ => ApiErrorCode.InternalError,
        };
    }
#pragma warning restore IDE0072
}
