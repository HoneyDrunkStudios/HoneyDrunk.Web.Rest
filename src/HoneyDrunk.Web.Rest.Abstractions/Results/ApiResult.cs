using HoneyDrunk.Web.Rest.Abstractions.Errors;
using System.Text.Json.Serialization;

namespace HoneyDrunk.Web.Rest.Abstractions.Results;

/// <summary>
/// Represents a non-generic API result with status and correlation metadata.
/// </summary>
public record ApiResult
{
    /// <summary>
    /// Gets the status of the result.
    /// </summary>
    public ApiResultStatus Status { get; init; } = ApiResultStatus.Success;

    /// <summary>
    /// Gets a value indicating whether the result represents success.
    /// </summary>
    [JsonIgnore]
    public bool IsSuccess => Status == ApiResultStatus.Success;

    /// <summary>
    /// Gets the correlation ID for this result.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CorrelationId { get; init; }

    /// <summary>
    /// Gets the trace ID from distributed tracing, if available.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TraceId { get; init; }

    /// <summary>
    /// Gets the error information, if any.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ApiError? Error { get; init; }

    /// <summary>
    /// Gets the timestamp of the result.
    /// </summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Creates a successful <see cref="ApiResult"/>.
    /// </summary>
    /// <param name="correlationId">The optional correlation ID.</param>
    /// <returns>A successful result.</returns>
    public static ApiResult Success(string? correlationId = null) => new()
    {
        Status = ApiResultStatus.Success,
        CorrelationId = correlationId,
    };

    /// <summary>
    /// Creates a failed <see cref="ApiResult"/>.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="code">The error code.</param>
    /// <param name="correlationId">The optional correlation ID.</param>
    /// <returns>A failed result.</returns>
    public static ApiResult Fail(
        string message,
        string code = ApiErrorCode.GeneralError,
        string? correlationId = null) => Failure(ApiResultStatus.Failed, code, message, correlationId);

    /// <summary>
    /// Creates a not found <see cref="ApiResult"/>.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="correlationId">The optional correlation ID.</param>
    /// <returns>A not found result.</returns>
    public static ApiResult NotFound(
        string message = "The requested resource was not found.",
        string? correlationId = null) => Failure(ApiResultStatus.NotFound, ApiErrorCode.NotFound, message, correlationId);

    /// <summary>
    /// Creates an unauthorized <see cref="ApiResult"/>.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="correlationId">The optional correlation ID.</param>
    /// <returns>An unauthorized result.</returns>
    public static ApiResult Unauthorized(
        string message = "Authentication is required.",
        string? correlationId = null) => Failure(ApiResultStatus.Unauthorized, ApiErrorCode.Unauthorized, message, correlationId);

    /// <summary>
    /// Creates a forbidden <see cref="ApiResult"/>.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="correlationId">The optional correlation ID.</param>
    /// <returns>A forbidden result.</returns>
    public static ApiResult Forbidden(
        string message = "You do not have permission to access this resource.",
        string? correlationId = null) => Failure(ApiResultStatus.Forbidden, ApiErrorCode.Forbidden, message, correlationId);

    /// <summary>
    /// Creates a conflict <see cref="ApiResult"/>.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="correlationId">The optional correlation ID.</param>
    /// <returns>A conflict result.</returns>
    public static ApiResult Conflict(
        string message,
        string? correlationId = null) => Failure(ApiResultStatus.Conflict, ApiErrorCode.Conflict, message, correlationId);

    /// <summary>
    /// Creates an error <see cref="ApiResult"/>.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="correlationId">The optional correlation ID.</param>
    /// <returns>An error result.</returns>
    public static ApiResult InternalError(
        string message = "An internal server error occurred.",
        string? correlationId = null) => Failure(ApiResultStatus.Error, ApiErrorCode.InternalError, message, correlationId);

    private static ApiResult Failure(ApiResultStatus status, string code, string message, string? correlationId) => new()
    {
        Status = status,
        Error = new ApiError { Code = code, Message = message },
        CorrelationId = correlationId,
    };
}
