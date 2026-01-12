using HoneyDrunk.Kernel.Abstractions.Context;
using HoneyDrunk.Web.Rest.Abstractions.Telemetry;
using HoneyDrunk.Web.Rest.AspNetCore.Configuration;
using HoneyDrunk.Web.Rest.AspNetCore.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace HoneyDrunk.Web.Rest.AspNetCore.Middleware;

/// <summary>
/// Middleware that creates a structured logging scope for the request.
/// Adds correlation ID, request path, method, trace ID, and Kernel context values to the logging context.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="RequestLoggingScopeMiddleware"/> class.
/// </remarks>
/// <param name="next">The next middleware in the pipeline.</param>
/// <param name="logger">The logger.</param>
/// <param name="options">The REST options.</param>
public sealed class RequestLoggingScopeMiddleware(
    RequestDelegate next,
    ILogger<RequestLoggingScopeMiddleware> logger,
    IOptions<RestOptions> options)
{
    private readonly RestOptions _options = options.Value;

    /// <summary>
    /// Invokes the middleware.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="correlationIdAccessor">The correlation ID accessor.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(
        HttpContext context,
        ICorrelationIdAccessor correlationIdAccessor)
    {
        if (!_options.EnableRequestLoggingScope)
        {
            await next(context).ConfigureAwait(false);
            return;
        }

        // Try to get Kernel operation context accessor if available
        IOperationContextAccessor? operationContextAccessor = context.RequestServices.GetService<IOperationContextAccessor>();

        Dictionary<string, object?> scopeState = new()
        {
            [RestTelemetryTags.CorrelationId] = correlationIdAccessor.CorrelationId,
            [RestTelemetryTags.HttpMethod] = context.Request.Method,
            [RestTelemetryTags.HttpPath] = context.Request.Path.Value,
            [RestTelemetryTags.RequestId] = context.TraceIdentifier,
        };

        // Add trace ID if available
        if (_options.IncludeTraceId && Activity.Current?.Id is not null)
        {
            scopeState["TraceId"] = Activity.Current.Id;
        }

        // Enrich with Kernel context values if available
        EnrichWithKernelContext(scopeState, operationContextAccessor);

        using (logger.BeginScope(scopeState))
        {
            await next(context).ConfigureAwait(false);
        }
    }

    private static void EnrichWithKernelContext(Dictionary<string, object?> scopeState, IOperationContextAccessor? operationContextAccessor)
    {
        if (operationContextAccessor?.Current is not { } operationContext)
        {
            return;
        }

        // Add operation-level context
        if (!string.IsNullOrWhiteSpace(operationContext.OperationId))
        {
            scopeState["OperationId"] = operationContext.OperationId;
        }

        if (!string.IsNullOrWhiteSpace(operationContext.OperationName))
        {
            scopeState["OperationName"] = operationContext.OperationName;
        }

        if (!string.IsNullOrWhiteSpace(operationContext.CausationId))
        {
            scopeState["CausationId"] = operationContext.CausationId;
        }

        // Add tenant/project context
        if (!string.IsNullOrWhiteSpace(operationContext.TenantId))
        {
            scopeState[RestTelemetryTags.TenantId] = operationContext.TenantId;
        }

        if (!string.IsNullOrWhiteSpace(operationContext.ProjectId))
        {
            scopeState["ProjectId"] = operationContext.ProjectId;
        }

        // Add Grid context values
        IGridContext gridContext = operationContext.GridContext;

        if (!string.IsNullOrWhiteSpace(gridContext.NodeId))
        {
            scopeState["NodeId"] = gridContext.NodeId;
        }

        if (!string.IsNullOrWhiteSpace(gridContext.StudioId))
        {
            scopeState["StudioId"] = gridContext.StudioId;
        }

        if (!string.IsNullOrWhiteSpace(gridContext.Environment))
        {
            scopeState["Environment"] = gridContext.Environment;
        }
    }
}
