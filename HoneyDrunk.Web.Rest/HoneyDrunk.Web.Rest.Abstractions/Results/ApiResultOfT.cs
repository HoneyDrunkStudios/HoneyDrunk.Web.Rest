using HoneyDrunk.Web.Rest.Abstractions.Errors;
using System.Text.Json.Serialization;

#pragma warning disable SA1649 // File name should match first type name - generic type uses backtick notation
namespace HoneyDrunk.Web.Rest.Abstractions.Results;

/// <summary>
/// Represents a generic API result with data, status, and correlation metadata.
/// </summary>
/// <typeparam name="T">The type of the data.</typeparam>
#pragma warning disable CA1000 // Do not declare static members on generic types - factory methods are idiomatic for records
public record ApiResult<T> : ApiResult
{
    /// <summary>
    /// Gets the data payload.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public T? Data { get; init; }

    /// <summary>
    /// Creates a successful <see cref="ApiResult{T}"/> with data.
    /// </summary>
    /// <param name="data">The data payload.</param>
    /// <param name="correlationId">The optional correlation ID.</param>
    /// <returns>A successful result with data.</returns>
    public static ApiResult<T> Success(T data, string? correlationId = null) => new()
    {
        Status = ApiResultStatus.Success,
        Data = data,
        CorrelationId = correlationId,
    };

    /// <summary>
    /// Creates a failed <see cref="ApiResult{T}"/>.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="code">The error code.</param>
    /// <param name="correlationId">The optional correlation ID.</param>
    /// <returns>A failed result.</returns>
    public static new ApiResult<T> Fail(
        string message,
        string code = ApiErrorCode.GeneralError,
        string? correlationId = null) => new()
    {
        Status = ApiResultStatus.Failed,
        Error = new ApiError { Code = code, Message = message },
        CorrelationId = correlationId,
    };

    /// <summary>
    /// Creates a not found <see cref="ApiResult{T}"/>.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="correlationId">The optional correlation ID.</param>
    /// <returns>A not found result.</returns>
    public static new ApiResult<T> NotFound(
        string message = "The requested resource was not found.",
        string? correlationId = null) => new()
    {
        Status = ApiResultStatus.NotFound,
        Error = new ApiError { Code = ApiErrorCode.NotFound, Message = message },
        CorrelationId = correlationId,
    };

    /// <summary>
    /// Creates an unauthorized <see cref="ApiResult{T}"/>.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="correlationId">The optional correlation ID.</param>
    /// <returns>An unauthorized result.</returns>
    public static new ApiResult<T> Unauthorized(
        string message = "Authentication is required.",
        string? correlationId = null) => new()
    {
        Status = ApiResultStatus.Unauthorized,
        Error = new ApiError { Code = ApiErrorCode.Unauthorized, Message = message },
        CorrelationId = correlationId,
    };

    /// <summary>
    /// Creates a forbidden <see cref="ApiResult{T}"/>.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="correlationId">The optional correlation ID.</param>
    /// <returns>A forbidden result.</returns>
    public static new ApiResult<T> Forbidden(
        string message = "You do not have permission to access this resource.",
        string? correlationId = null) => new()
    {
        Status = ApiResultStatus.Forbidden,
        Error = new ApiError { Code = ApiErrorCode.Forbidden, Message = message },
        CorrelationId = correlationId,
    };

    /// <summary>
    /// Creates a conflict <see cref="ApiResult{T}"/>.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="correlationId">The optional correlation ID.</param>
    /// <returns>A conflict result.</returns>
    public static new ApiResult<T> Conflict(
        string message,
        string? correlationId = null) => new()
    {
        Status = ApiResultStatus.Conflict,
        Error = new ApiError { Code = ApiErrorCode.Conflict, Message = message },
        CorrelationId = correlationId,
    };

    /// <summary>
    /// Creates an error <see cref="ApiResult{T}"/>.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="correlationId">The optional correlation ID.</param>
    /// <returns>An error result.</returns>
    public static new ApiResult<T> InternalError(
        string message = "An internal server error occurred.",
        string? correlationId = null) => new()
    {
        Status = ApiResultStatus.Error,
        Error = new ApiError { Code = ApiErrorCode.InternalError, Message = message },
        CorrelationId = correlationId,
    };

    /// <summary>
    /// Converts from a non-generic <see cref="ApiResult"/> to a generic <see cref="ApiResult{T}"/>.
    /// </summary>
    /// <param name="result">The source result.</param>
    /// <returns>A generic result without data.</returns>
    public static ApiResult<T> FromResult(ApiResult result) => new()
    {
        Status = result.Status,
        Error = result.Error,
        CorrelationId = result.CorrelationId,
        TraceId = result.TraceId,
        Timestamp = result.Timestamp,
    };
}
#pragma warning restore CA1000
#pragma warning restore SA1649
