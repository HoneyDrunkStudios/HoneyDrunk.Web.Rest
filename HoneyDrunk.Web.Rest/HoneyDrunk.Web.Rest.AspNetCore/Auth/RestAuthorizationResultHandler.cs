using HoneyDrunk.Auth.AspNetCore;
using HoneyDrunk.Web.Rest.Abstractions.Constants;
using HoneyDrunk.Web.Rest.Abstractions.Errors;
using HoneyDrunk.Web.Rest.AspNetCore.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using System.Text.Json;

namespace HoneyDrunk.Web.Rest.AspNetCore.Auth;

/// <summary>
/// Handles authorization failures by shaping responses as <see cref="ApiErrorResponse"/>.
/// Uses HoneyDrunk.Auth.AspNetCore's <see cref="IAuthenticatedIdentityAccessor"/> when available.
/// </summary>
/// <remarks>
/// This handler is registered by <c>AddRest()</c> when <c>EnableAuthFailureShaping</c> is true.
/// It intercepts authorization challenges (401) and forbids (403) and writes a consistent
/// JSON response envelope instead of the default framework behavior.
/// </remarks>
internal sealed class RestAuthorizationResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();

    /// <inheritdoc/>
    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        if (authorizeResult.Challenged)
        {
            await WriteUnauthorizedResponseAsync(context).ConfigureAwait(false);
            return;
        }

        if (authorizeResult.Forbidden)
        {
            await WriteForbiddenResponseAsync(context).ConfigureAwait(false);
            return;
        }

        await _defaultHandler.HandleAsync(next, context, policy, authorizeResult).ConfigureAwait(false);
    }

    private static async Task WriteUnauthorizedResponseAsync(HttpContext context)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        string correlationId = RestAuthExtensions.GetCorrelationId(context);
        string? traceId = Activity.Current?.Id;

        ApiErrorResponse response = ApiErrorResponse.CreateUnauthorized(
            correlationId,
            "Authentication is required.",
            traceId);

        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = MediaTypes.Json;

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response, JsonOptionsDefaults.SerializerOptions)).ConfigureAwait(false);
    }

    private static async Task WriteForbiddenResponseAsync(HttpContext context)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        string correlationId = RestAuthExtensions.GetCorrelationId(context);
        string? traceId = Activity.Current?.Id;

        // Use IAuthenticatedIdentityAccessor if available; fall back to HttpContext.User
        IAuthenticatedIdentityAccessor? identityAccessor = context.RequestServices.GetService(typeof(IAuthenticatedIdentityAccessor)) as IAuthenticatedIdentityAccessor;

        bool isAuthenticated = identityAccessor?.IsAuthenticated
            ?? context.User?.Identity?.IsAuthenticated
            ?? false;

        string message = isAuthenticated
            ? "You do not have permission to access this resource."
            : "Authentication is required.";

        ApiErrorResponse response = ApiErrorResponse.CreateForbidden(
            correlationId,
            message,
            traceId);

        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        context.Response.ContentType = MediaTypes.Json;

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response, JsonOptionsDefaults.SerializerOptions)).ConfigureAwait(false);
    }
}
