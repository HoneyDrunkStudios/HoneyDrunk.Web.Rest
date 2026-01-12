using HoneyDrunk.Web.Rest.Abstractions.Constants;
using HoneyDrunk.Web.Rest.Abstractions.Errors;
using HoneyDrunk.Web.Rest.Abstractions.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace HoneyDrunk.Web.Rest.AspNetCore.MinimalApi;

/// <summary>
/// Extension methods for applying REST conventions to minimal API endpoints.
/// </summary>
public static class RestEndpointConventions
{
    /// <summary>
    /// Applies REST conventions to the endpoint, including standard produces/consumes and metadata.
    /// </summary>
    /// <param name="builder">The route handler builder.</param>
    /// <returns>The route handler builder for chaining.</returns>
    public static RouteHandlerBuilder WithRest(this RouteHandlerBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder
            .Accepts<object>(MediaTypes.Json)
            .Produces<ApiResult>(StatusCodes.Status200OK, MediaTypes.Json)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest, MediaTypes.Json)
            .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized, MediaTypes.Json)
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden, MediaTypes.Json)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound, MediaTypes.Json)
            .Produces<ApiErrorResponse>(StatusCodes.Status500InternalServerError, MediaTypes.Json);
    }

    /// <summary>
    /// Applies REST conventions with a typed success response.
    /// </summary>
    /// <typeparam name="TResponse">The type of the success response.</typeparam>
    /// <param name="builder">The route handler builder.</param>
    /// <returns>The route handler builder for chaining.</returns>
    public static RouteHandlerBuilder WithRest<TResponse>(this RouteHandlerBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder
            .Accepts<object>(MediaTypes.Json)
            .Produces<ApiResult<TResponse>>(StatusCodes.Status200OK, MediaTypes.Json)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest, MediaTypes.Json)
            .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized, MediaTypes.Json)
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden, MediaTypes.Json)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound, MediaTypes.Json)
            .Produces<ApiErrorResponse>(StatusCodes.Status500InternalServerError, MediaTypes.Json);
    }

    /// <summary>
    /// Applies REST conventions for create operations (201 Created).
    /// </summary>
    /// <typeparam name="TResponse">The type of the success response.</typeparam>
    /// <param name="builder">The route handler builder.</param>
    /// <returns>The route handler builder for chaining.</returns>
    public static RouteHandlerBuilder WithRestCreate<TResponse>(this RouteHandlerBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder
            .Accepts<object>(MediaTypes.Json)
            .Produces<ApiResult<TResponse>>(StatusCodes.Status201Created, MediaTypes.Json)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest, MediaTypes.Json)
            .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized, MediaTypes.Json)
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden, MediaTypes.Json)
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict, MediaTypes.Json)
            .Produces<ApiErrorResponse>(StatusCodes.Status500InternalServerError, MediaTypes.Json);
    }

    /// <summary>
    /// Applies REST conventions for delete operations (204 No Content).
    /// </summary>
    /// <param name="builder">The route handler builder.</param>
    /// <returns>The route handler builder for chaining.</returns>
    public static RouteHandlerBuilder WithRestDelete(this RouteHandlerBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest, MediaTypes.Json)
            .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized, MediaTypes.Json)
            .Produces<ApiErrorResponse>(StatusCodes.Status403Forbidden, MediaTypes.Json)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound, MediaTypes.Json)
            .Produces<ApiErrorResponse>(StatusCodes.Status500InternalServerError, MediaTypes.Json);
    }

    /// <summary>
    /// Adds authorization requirement with REST error shaping.
    /// </summary>
    /// <param name="builder">The route handler builder.</param>
    /// <param name="policy">The optional authorization policy name.</param>
    /// <returns>The route handler builder for chaining.</returns>
    public static RouteHandlerBuilder RequireRestAuth(this RouteHandlerBuilder builder, string? policy = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (string.IsNullOrWhiteSpace(policy))
        {
            return builder.RequireAuthorization();
        }

        return builder.RequireAuthorization(policy);
    }
}
