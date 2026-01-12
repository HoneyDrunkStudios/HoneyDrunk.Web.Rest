using HoneyDrunk.Web.Rest.Abstractions.Errors;
using System.Net;

namespace HoneyDrunk.Web.Rest.AspNetCore.Errors;

/// <summary>
/// Maps exceptions to API error responses with appropriate HTTP status codes.
/// </summary>
internal static class ExceptionToApiErrorMapper
{
    /// <summary>
    /// Maps an exception to an appropriate HTTP status code and error code.
    /// </summary>
    /// <param name="exception">The exception to map.</param>
    /// <returns>The mapping result containing status code, error code, and message.</returns>
    /// <remarks>
    /// Mapping rules:
    /// <list type="bullet">
    /// <item><description>ArgumentException, ArgumentNullException returns 400 Bad Request.</description></item>
    /// <item><description>InvalidOperationException returns 409 Conflict (indicates invalid state).</description></item>
    /// <item><description>KeyNotFoundException returns 404 Not Found.</description></item>
    /// <item><description>UnauthorizedAccessException returns 401 or 403 (depends on auth state, defaults to 403).</description></item>
    /// <item><description>NotImplementedException returns 501 Not Implemented.</description></item>
    /// <item><description>OperationCanceledException returns 499 Client Closed Request.</description></item>
    /// <item><description>All others return 500 Internal Server Error.</description></item>
    /// </list>
    /// </remarks>
    public static Middleware.ExceptionMappingResult Map(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return exception switch
        {
            ArgumentNullException ex => new Middleware.ExceptionMappingResult(
                HttpStatusCode.BadRequest,
                ApiErrorCode.BadRequest,
                $"A required parameter was null: {ex.ParamName}"),

            ArgumentException ex => new Middleware.ExceptionMappingResult(
                HttpStatusCode.BadRequest,
                ApiErrorCode.BadRequest,
                ex.Message),

            InvalidOperationException ex => new Middleware.ExceptionMappingResult(
                HttpStatusCode.Conflict,
                ApiErrorCode.Conflict,
                ex.Message),

            KeyNotFoundException => new Middleware.ExceptionMappingResult(
                HttpStatusCode.NotFound,
                ApiErrorCode.NotFound,
                "The requested resource was not found."),

            UnauthorizedAccessException => new Middleware.ExceptionMappingResult(
                HttpStatusCode.Forbidden,
                ApiErrorCode.Forbidden,
                "You do not have permission to access this resource."),

            NotImplementedException => new Middleware.ExceptionMappingResult(
                HttpStatusCode.NotImplemented,
                ApiErrorCode.NotImplemented,
                "This operation is not implemented."),

            OperationCanceledException => new Middleware.ExceptionMappingResult(
                (HttpStatusCode)499,
                ApiErrorCode.GeneralError,
                "The request was cancelled."),

            _ => new Middleware.ExceptionMappingResult(
                HttpStatusCode.InternalServerError,
                ApiErrorCode.InternalError,
                "An internal server error occurred."),
        };
    }

    /// <summary>
    /// Determines the HTTP status code for an unauthorized access exception
    /// based on whether the user is authenticated.
    /// </summary>
    /// <param name="isAuthenticated">Whether the user is authenticated.</param>
    /// <returns>401 if not authenticated, 403 if authenticated but forbidden.</returns>
    public static HttpStatusCode GetUnauthorizedStatusCode(bool isAuthenticated)
    {
        return isAuthenticated ? HttpStatusCode.Forbidden : HttpStatusCode.Unauthorized;
    }

    /// <summary>
    /// Gets the error code for an unauthorized access exception
    /// based on whether the user is authenticated.
    /// </summary>
    /// <param name="isAuthenticated">Whether the user is authenticated.</param>
    /// <returns>The appropriate error code.</returns>
    public static string GetUnauthorizedErrorCode(bool isAuthenticated)
    {
        return isAuthenticated ? ApiErrorCode.Forbidden : ApiErrorCode.Unauthorized;
    }
}
