using HoneyDrunk.Web.Rest.Abstractions.Constants;
using HoneyDrunk.Web.Rest.Abstractions.Errors;
using HoneyDrunk.Web.Rest.AspNetCore.Context;
using HoneyDrunk.Web.Rest.AspNetCore.Serialization;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using System.Text.Json;

namespace HoneyDrunk.Web.Rest.AspNetCore.Auth;

/// <summary>
/// Helper methods for writing authentication/authorization error responses in the <see cref="ApiErrorResponse"/> format.
/// </summary>
/// <remarks>
/// These methods are used by <see cref="RestAuthorizationResultHandler"/> to shape 401/403 responses.
/// The handler is automatically registered by <c>AddRest()</c> when <c>EnableAuthFailureShaping</c> is true.
/// </remarks>
public static class RestAuthExtensions
{
    /// <summary>
    /// Writes an unauthorized (401) response in the <see cref="ApiErrorResponse"/> format.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="message">The optional error message.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task WriteUnauthorizedResponseAsync(
        HttpContext context,
        string? message = null,
        CancellationToken cancellationToken = default)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        string correlationId = GetCorrelationId(context);
        string? traceId = Activity.Current?.Id;

        ApiErrorResponse response = ApiErrorResponse.CreateUnauthorized(
            correlationId,
            message ?? "Authentication is required.",
            traceId);

        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = MediaTypes.Json;

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response, JsonOptionsDefaults.SerializerOptions),
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Writes a forbidden (403) response in the <see cref="ApiErrorResponse"/> format.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="message">The optional error message.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task WriteForbiddenResponseAsync(
        HttpContext context,
        string? message = null,
        CancellationToken cancellationToken = default)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        string correlationId = GetCorrelationId(context);
        string? traceId = Activity.Current?.Id;

        ApiErrorResponse response = ApiErrorResponse.CreateForbidden(
            correlationId,
            message ?? "You do not have permission to access this resource.",
            traceId);

        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        context.Response.ContentType = MediaTypes.Json;

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response, JsonOptionsDefaults.SerializerOptions),
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the correlation ID from the current request context.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>The correlation ID, or an empty string if not available.</returns>
    /// <remarks>
    /// Priority order:
    /// 1. <see cref="ICorrelationIdAccessor"/> (set by CorrelationMiddleware)
    /// 2. HttpContext.Items[HeaderNames.CorrelationId]
    /// 3. Request header
    /// Does not generate a new ID - if correlation is missing, returns empty string
    /// to indicate the correlation middleware has not run.
    /// </remarks>
    internal static string GetCorrelationId(HttpContext context)
    {
        // Priority 1: Accessor (set by CorrelationMiddleware)
        ICorrelationIdAccessor? accessor = context.RequestServices.GetService(typeof(ICorrelationIdAccessor)) as ICorrelationIdAccessor;
        if (!string.IsNullOrWhiteSpace(accessor?.CorrelationId))
        {
            return accessor.CorrelationId;
        }

        // Priority 2: HttpContext.Items (set by CorrelationMiddleware)
        if (context.Items.TryGetValue(HeaderNames.CorrelationId, out object? value) && value is string correlationId)
        {
            return correlationId;
        }

        // Priority 3: Request header (if middleware hasn't run yet)
        if (context.Request.Headers.TryGetValue(HeaderNames.CorrelationId, out Microsoft.Extensions.Primitives.StringValues headerValue)
            && !string.IsNullOrWhiteSpace(headerValue.ToString()))
        {
            return headerValue.ToString();
        }

        // Do not generate - if we get here, correlation middleware hasn't run
        // Return empty rather than generating to avoid mismatch
        return string.Empty;
    }
}
