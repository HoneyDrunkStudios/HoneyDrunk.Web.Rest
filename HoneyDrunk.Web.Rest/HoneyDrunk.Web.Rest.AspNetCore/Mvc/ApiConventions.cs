using HoneyDrunk.Web.Rest.Abstractions.Errors;
using HoneyDrunk.Web.Rest.Abstractions.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HoneyDrunk.Web.Rest.AspNetCore.Mvc;

/// <summary>
/// Provides centralized API conventions for action result rules.
/// </summary>
public static class ApiConventions
{
    /// <summary>
    /// Produces an OK (200) result with the specified data wrapped in <see cref="ApiResult{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the data.</typeparam>
    /// <param name="data">The data to return.</param>
    /// <param name="correlationId">The optional correlation ID.</param>
    /// <returns>An OK object result.</returns>
    public static IActionResult Ok<T>(T data, string? correlationId = null)
    {
        ApiResult<T> result = ApiResult<T>.Success(data, correlationId);
        return new OkObjectResult(result);
    }

    /// <summary>
    /// Produces a Created (201) result with the specified data.
    /// </summary>
    /// <typeparam name="T">The type of the data.</typeparam>
    /// <param name="location">The location URI of the created resource.</param>
    /// <param name="data">The data to return.</param>
    /// <param name="correlationId">The optional correlation ID.</param>
    /// <returns>A created object result.</returns>
    public static IActionResult Created<T>(string location, T data, string? correlationId = null)
    {
        ApiResult<T> result = ApiResult<T>.Success(data, correlationId);
        return new CreatedResult(location, result);
    }

    /// <summary>
    /// Produces a NoContent (204) result.
    /// </summary>
    /// <returns>A no content result.</returns>
    public static IActionResult NoContent()
    {
        return new NoContentResult();
    }

    /// <summary>
    /// Produces a BadRequest (400) result with an error response.
    /// </summary>
    /// <param name="correlationId">The correlation ID.</param>
    /// <param name="message">The error message.</param>
    /// <param name="traceId">The optional trace ID.</param>
    /// <returns>A bad request object result.</returns>
    public static IActionResult BadRequest(string correlationId, string message, string? traceId = null)
    {
        ApiErrorResponse response = ApiErrorResponse.Create(correlationId, message, ApiErrorCode.BadRequest, traceId);
        return new BadRequestObjectResult(response);
    }

    /// <summary>
    /// Produces a NotFound (404) result with an error response.
    /// </summary>
    /// <param name="correlationId">The correlation ID.</param>
    /// <param name="message">The error message.</param>
    /// <param name="traceId">The optional trace ID.</param>
    /// <returns>A not found object result.</returns>
    public static IActionResult NotFound(string correlationId, string? message = null, string? traceId = null)
    {
        ApiErrorResponse response = ApiErrorResponse.CreateNotFound(correlationId, message ?? "The requested resource was not found.", traceId);
        return new NotFoundObjectResult(response);
    }

    /// <summary>
    /// Produces an Unauthorized (401) result with an error response.
    /// </summary>
    /// <param name="correlationId">The correlation ID.</param>
    /// <param name="message">The error message.</param>
    /// <param name="traceId">The optional trace ID.</param>
    /// <returns>An unauthorized object result.</returns>
    public static IActionResult Unauthorized(string correlationId, string? message = null, string? traceId = null)
    {
        ApiErrorResponse response = ApiErrorResponse.CreateUnauthorized(correlationId, message ?? "Authentication is required.", traceId);
        return new UnauthorizedObjectResult(response);
    }

    /// <summary>
    /// Produces a Forbidden (403) result with an error response.
    /// </summary>
    /// <param name="correlationId">The correlation ID.</param>
    /// <param name="message">The error message.</param>
    /// <param name="traceId">The optional trace ID.</param>
    /// <returns>An object result with 403 status.</returns>
    public static IActionResult Forbidden(string correlationId, string? message = null, string? traceId = null)
    {
        ApiErrorResponse response = ApiErrorResponse.CreateForbidden(correlationId, message ?? "You do not have permission to access this resource.", traceId);
        return new ObjectResult(response) { StatusCode = StatusCodes.Status403Forbidden };
    }

    /// <summary>
    /// Produces a Conflict (409) result with an error response.
    /// </summary>
    /// <param name="correlationId">The correlation ID.</param>
    /// <param name="message">The error message.</param>
    /// <param name="traceId">The optional trace ID.</param>
    /// <returns>A conflict object result.</returns>
    public static IActionResult Conflict(string correlationId, string message, string? traceId = null)
    {
        ApiErrorResponse response = ApiErrorResponse.Create(correlationId, message, ApiErrorCode.Conflict, traceId);
        return new ConflictObjectResult(response);
    }

    /// <summary>
    /// Produces an InternalServerError (500) result with an error response.
    /// </summary>
    /// <param name="correlationId">The correlation ID.</param>
    /// <param name="message">The error message.</param>
    /// <param name="traceId">The optional trace ID.</param>
    /// <returns>An object result with 500 status.</returns>
    public static IActionResult InternalError(string correlationId, string? message = null, string? traceId = null)
    {
        ApiErrorResponse response = ApiErrorResponse.CreateInternalError(correlationId, message ?? "An internal server error occurred.", traceId);
        return new ObjectResult(response) { StatusCode = StatusCodes.Status500InternalServerError };
    }
}
