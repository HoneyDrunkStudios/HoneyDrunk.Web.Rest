using HoneyDrunk.Transport.Abstractions;
using HoneyDrunk.Web.Rest.Abstractions.Results;

namespace HoneyDrunk.Web.Rest.AspNetCore.Transport;

/// <summary>
/// Extension methods for mapping Transport operations to REST API results.
/// </summary>
public static class TransportResultExtensions
{
    /// <summary>
    /// Creates an <see cref="ApiResult"/> from a transport envelope.
    /// Uses the envelope's CorrelationId for the result.
    /// </summary>
    /// <param name="envelope">The transport envelope.</param>
    /// <returns>A successful API result with the envelope's correlation ID.</returns>
    public static ApiResult ToApiResult(this ITransportEnvelope envelope)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        return ApiResult.Success(envelope.CorrelationId);
    }

    /// <summary>
    /// Creates an <see cref="ApiResult{T}"/> from a transport envelope with data.
    /// Uses the envelope's CorrelationId for the result.
    /// </summary>
    /// <typeparam name="T">The type of data.</typeparam>
    /// <param name="envelope">The transport envelope.</param>
    /// <param name="data">The data payload.</param>
    /// <returns>A successful API result with data and the envelope's correlation ID.</returns>
    public static ApiResult<T> ToApiResult<T>(this ITransportEnvelope envelope, T data)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        return ApiResult<T>.Success(data, envelope.CorrelationId);
    }

    /// <summary>
    /// Creates an <see cref="ApiResult"/> from an operation that succeeded.
    /// </summary>
    /// <param name="correlationId">The correlation ID from the transport operation.</param>
    /// <returns>A successful API result with the correlation ID.</returns>
    public static ApiResult ToSuccessResult(string? correlationId)
    {
        return ApiResult.Success(correlationId);
    }

    /// <summary>
    /// Creates an <see cref="ApiResult{T}"/> with data from a transport operation.
    /// </summary>
    /// <typeparam name="T">The type of data.</typeparam>
    /// <param name="data">The data payload.</param>
    /// <param name="correlationId">The correlation ID from the transport operation.</param>
    /// <returns>A successful API result with data and the correlation ID.</returns>
    public static ApiResult<T> ToSuccessResult<T>(T data, string? correlationId)
    {
        return ApiResult<T>.Success(data, correlationId);
    }

    /// <summary>
    /// Creates an <see cref="ApiResult"/> from a failed operation.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    /// <param name="correlationId">The optional correlation ID.</param>
    /// <returns>A failed API result.</returns>
    public static ApiResult ToFailureResult(string errorMessage, string? correlationId = null)
    {
        return ApiResult.Fail(errorMessage, correlationId: correlationId);
    }

    /// <summary>
    /// Creates an <see cref="ApiResult{T}"/> from a failed operation.
    /// </summary>
    /// <typeparam name="T">The type of data (not used for failures).</typeparam>
    /// <param name="errorMessage">The error message.</param>
    /// <param name="correlationId">The optional correlation ID.</param>
    /// <returns>A failed API result.</returns>
    public static ApiResult<T> ToFailureResult<T>(string errorMessage, string? correlationId = null)
    {
        return ApiResult<T>.Fail(errorMessage, correlationId: correlationId);
    }

    /// <summary>
    /// Creates an <see cref="ApiResult"/> based on an operation outcome.
    /// </summary>
    /// <param name="isSuccess">Whether the operation succeeded.</param>
    /// <param name="errorMessage">The error message if failed.</param>
    /// <param name="correlationId">The optional correlation ID.</param>
    /// <returns>A success or failure API result.</returns>
    public static ApiResult FromOutcome(bool isSuccess, string? errorMessage = null, string? correlationId = null)
    {
        return isSuccess
            ? ApiResult.Success(correlationId)
            : ApiResult.Fail(errorMessage ?? "Operation failed.", correlationId: correlationId);
    }

    /// <summary>
    /// Creates an <see cref="ApiResult{T}"/> based on an operation outcome.
    /// </summary>
    /// <typeparam name="T">The type of data.</typeparam>
    /// <param name="isSuccess">Whether the operation succeeded.</param>
    /// <param name="data">The data payload to include on success.</param>
    /// <param name="errorMessage">The error message if failed.</param>
    /// <param name="correlationId">The optional correlation ID.</param>
    /// <returns>A success or failure API result.</returns>
    public static ApiResult<T> FromOutcome<T>(bool isSuccess, T? data, string? errorMessage = null, string? correlationId = null)
    {
        return isSuccess
            ? ApiResult<T>.Success(data!, correlationId)
            : ApiResult<T>.Fail(errorMessage ?? "Operation failed.", correlationId: correlationId);
    }
}
