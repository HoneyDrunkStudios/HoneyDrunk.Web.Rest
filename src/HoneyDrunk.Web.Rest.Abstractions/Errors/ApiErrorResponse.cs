using System.Text.Json.Serialization;

namespace HoneyDrunk.Web.Rest.Abstractions.Errors;

/// <summary>
/// Represents the standard error response envelope for all non-2xx responses.
/// </summary>
public sealed record ApiErrorResponse
{
    /// <summary>
    /// Gets the correlation ID for this request.
    /// </summary>
    public required string CorrelationId { get; init; }

    /// <summary>
    /// Gets the primary error information.
    /// </summary>
    public required ApiError Error { get; init; }

    /// <summary>
    /// Gets the list of validation errors, if any.
    /// This is populated when the error is a validation failure.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<ValidationError>? ValidationErrors { get; init; }

    /// <summary>
    /// Gets the trace ID from distributed tracing, if available.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TraceId { get; init; }

    /// <summary>
    /// Gets the timestamp when the error occurred.
    /// </summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Creates an <see cref="ApiErrorResponse"/> for a general error.
    /// </summary>
    /// <param name="correlationId">The correlation ID.</param>
    /// <param name="message">The error message.</param>
    /// <param name="code">The error code.</param>
    /// <param name="traceId">The optional trace ID.</param>
    /// <returns>A new <see cref="ApiErrorResponse"/> instance.</returns>
    public static ApiErrorResponse Create(
        string correlationId,
        string message,
        string code = ApiErrorCode.GeneralError,
        string? traceId = null)
    {
        return new ApiErrorResponse
        {
            CorrelationId = correlationId,
            Error = new ApiError { Code = code, Message = message },
            TraceId = traceId,
        };
    }

    /// <summary>
    /// Creates an <see cref="ApiErrorResponse"/> for validation failures.
    /// </summary>
    /// <param name="correlationId">The correlation ID.</param>
    /// <param name="validationErrors">The list of validation errors.</param>
    /// <param name="traceId">The optional trace ID.</param>
    /// <returns>A new <see cref="ApiErrorResponse"/> instance.</returns>
    public static ApiErrorResponse CreateValidationError(
        string correlationId,
        IReadOnlyList<ValidationError> validationErrors,
        string? traceId = null)
    {
        return new ApiErrorResponse
        {
            CorrelationId = correlationId,
            Error = new ApiError
            {
                Code = ApiErrorCode.ValidationFailed,
                Message = "One or more validation errors occurred.",
            },
            ValidationErrors = validationErrors,
            TraceId = traceId,
        };
    }

    /// <summary>
    /// Creates an <see cref="ApiErrorResponse"/> for not found errors.
    /// </summary>
    /// <param name="correlationId">The correlation ID.</param>
    /// <param name="message">The error message.</param>
    /// <param name="traceId">The optional trace ID.</param>
    /// <returns>A new <see cref="ApiErrorResponse"/> instance.</returns>
    public static ApiErrorResponse CreateNotFound(
        string correlationId,
        string message = "The requested resource was not found.",
        string? traceId = null)
    {
        return Create(correlationId, message, ApiErrorCode.NotFound, traceId);
    }

    /// <summary>
    /// Creates an <see cref="ApiErrorResponse"/> for unauthorized errors.
    /// </summary>
    /// <param name="correlationId">The correlation ID.</param>
    /// <param name="message">The error message.</param>
    /// <param name="traceId">The optional trace ID.</param>
    /// <returns>A new <see cref="ApiErrorResponse"/> instance.</returns>
    public static ApiErrorResponse CreateUnauthorized(
        string correlationId,
        string message = "Authentication is required.",
        string? traceId = null)
    {
        return Create(correlationId, message, ApiErrorCode.Unauthorized, traceId);
    }

    /// <summary>
    /// Creates an <see cref="ApiErrorResponse"/> for forbidden errors.
    /// </summary>
    /// <param name="correlationId">The correlation ID.</param>
    /// <param name="message">The error message.</param>
    /// <param name="traceId">The optional trace ID.</param>
    /// <returns>A new <see cref="ApiErrorResponse"/> instance.</returns>
    public static ApiErrorResponse CreateForbidden(
        string correlationId,
        string message = "You do not have permission to access this resource.",
        string? traceId = null)
    {
        return Create(correlationId, message, ApiErrorCode.Forbidden, traceId);
    }

    /// <summary>
    /// Creates an <see cref="ApiErrorResponse"/> for internal server errors.
    /// </summary>
    /// <param name="correlationId">The correlation ID.</param>
    /// <param name="message">The error message.</param>
    /// <param name="traceId">The optional trace ID.</param>
    /// <returns>A new <see cref="ApiErrorResponse"/> instance.</returns>
    public static ApiErrorResponse CreateInternalError(
        string correlationId,
        string message = "An internal server error occurred.",
        string? traceId = null)
    {
        return Create(correlationId, message, ApiErrorCode.InternalError, traceId);
    }
}
