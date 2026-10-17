using HoneyDrunk.Kernel.Abstractions.Errors;
using HoneyDrunk.Web.Rest.Abstractions.Results;
using HoneyDrunk.Web.Rest.AspNetCore.Context;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace HoneyDrunk.Web.Rest.Canary;

/// <summary>
/// Minimal endpoints that trigger specific edge behaviors for canary verification.
/// </summary>
internal static class CanaryEndpoints
{
    /// <summary>
    /// Maps all canary endpoints to the endpoint route builder.
    /// </summary>
    public static IEndpointRouteBuilder MapCanaryEndpoints(this IEndpointRouteBuilder endpoints)
    {
        // Invariant 1: Malformed JSON
        endpoints.MapPost("/canary/json-body", (Delegate)JsonBodyHandler);

        // Invariant 2: Kernel typed exceptions
        endpoints.MapGet("/canary/kernel/not-found", KernelNotFoundHandler);
        endpoints.MapGet("/canary/kernel/validation", KernelValidationHandler);
        endpoints.MapGet("/canary/kernel/concurrency", KernelConcurrencyHandler);
        endpoints.MapGet("/canary/kernel/dependency-failure", KernelDependencyFailureHandler);
        endpoints.MapGet("/canary/kernel/security", KernelSecurityHandler);

        // Invariant 4: Correlation round-trip
        endpoints.MapGet("/canary/correlation", CorrelationHandler);

        // Invariant 5: Response.HasStarted safety
        endpoints.MapGet("/canary/response-started", ResponseStartedHandler);

        return endpoints;
    }

    private static async Task<IResult> JsonBodyHandler(HttpContext context)
    {
        // Manually read and parse the body so that JsonException flows through
        // ExceptionMappingMiddleware rather than being swallowed by minimal API binding.
        using StreamReader reader = new(context.Request.Body);
        string raw = await reader.ReadToEndAsync().ConfigureAwait(false);
        _ = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(raw);
        return Results.Ok(ApiResult.Success());
    }

    private static IResult KernelNotFoundHandler()
    {
        throw new NotFoundException("Canary: resource was not found.");
    }

    private static IResult KernelValidationHandler()
    {
        throw new ValidationException("Canary: validation failed.");
    }

    private static IResult KernelConcurrencyHandler()
    {
        throw new ConcurrencyException("Canary: concurrency conflict.");
    }

    private static IResult KernelDependencyFailureHandler()
    {
        throw new DependencyFailureException("Canary: downstream dependency failed.");
    }

    private static IResult KernelSecurityHandler()
    {
        throw new Kernel.Abstractions.Errors.SecurityException("Canary: access denied.");
    }

    private static IResult CorrelationHandler(ICorrelationIdAccessor correlationAccessor)
    {
        return Results.Ok(new { correlationAccessor.CorrelationId });
    }

    private static async Task ResponseStartedHandler(HttpContext context)
    {
        // Begin writing the response body — this sets Response.HasStarted = true
        context.Response.StatusCode = 200;
        context.Response.ContentType = "text/plain";
        await context.Response.WriteAsync("partial response content").ConfigureAwait(false);
        await context.Response.Body.FlushAsync().ConfigureAwait(false);

        // Now throw — ExceptionMappingMiddleware must not attempt to write again
#pragma warning disable CA2201 // Exception type System.Exception is not sufficiently specific
        throw new Exception("Canary: exception after response started.");
#pragma warning restore CA2201
    }
}
