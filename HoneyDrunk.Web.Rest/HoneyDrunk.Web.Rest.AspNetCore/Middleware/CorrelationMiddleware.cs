using HoneyDrunk.Kernel.Abstractions.Context;
using HoneyDrunk.Web.Rest.Abstractions.Constants;
using HoneyDrunk.Web.Rest.AspNetCore.Configuration;
using HoneyDrunk.Web.Rest.AspNetCore.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace HoneyDrunk.Web.Rest.AspNetCore.Middleware;

/// <summary>
/// Middleware that handles correlation ID propagation.
/// Reads the correlation ID from the incoming request header or Kernel operation context,
/// generates one if missing, and returns it in the response header.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CorrelationMiddleware"/> class.
/// </remarks>
/// <param name="next">The next middleware in the pipeline.</param>
/// <param name="options">The REST options.</param>
/// <param name="logger">The logger.</param>
public sealed class CorrelationMiddleware(RequestDelegate next, IOptions<RestOptions> options, ILogger<CorrelationMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
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
        // Try to get Kernel operation context accessor if available
        IOperationContextAccessor? operationContextAccessor = context.RequestServices.GetService<IOperationContextAccessor>();

        string correlationId = GetOrCreateCorrelationId(context, operationContextAccessor);

        correlationIdAccessor.SetCorrelationId(correlationId);

        // Use HeaderNames constant for consistency across the codebase
        context.Items[HeaderNames.CorrelationId] = correlationId;

        if (_options.ReturnCorrelationIdInResponseHeader)
        {
            context.Response.OnStarting(() =>
            {
                if (!context.Response.Headers.ContainsKey(_options.CorrelationIdHeaderName))
                {
                    context.Response.Headers[_options.CorrelationIdHeaderName] = correlationId;
                }

                return Task.CompletedTask;
            });
        }

        await _next(context).ConfigureAwait(false);
    }

    private string GetOrCreateCorrelationId(HttpContext context, IOperationContextAccessor? operationContextAccessor)
    {
        string? kernelCorrelationId = null;

        // Priority 1: Kernel operation context (if available and has a correlation ID)
        if (operationContextAccessor?.Current is { } operationContext &&
            !string.IsNullOrWhiteSpace(operationContext.CorrelationId))
        {
            kernelCorrelationId = operationContext.CorrelationId;
        }

        // Check for incoming request header
        string? headerCorrelationId = null;
        if (context.Request.Headers.TryGetValue(_options.CorrelationIdHeaderName, out Microsoft.Extensions.Primitives.StringValues headerValue)
            && !string.IsNullOrWhiteSpace(headerValue.ToString()))
        {
            headerCorrelationId = headerValue.ToString();
        }

        // Log warning if both exist and differ
        if (kernelCorrelationId is not null && headerCorrelationId is not null
            && !string.Equals(kernelCorrelationId, headerCorrelationId, StringComparison.Ordinal))
        {
            logger.LogWarning(
                "Correlation ID mismatch: header '{HeaderCorrelationId}' differs from Kernel context '{KernelCorrelationId}'. "
                + "Kernel correlation takes precedence. Request: {HttpMethod} {RequestPath}",
                headerCorrelationId,
                kernelCorrelationId,
                context.Request.Method,
                context.Request.Path);
        }

        // Priority 1: Kernel wins
        if (kernelCorrelationId is not null)
        {
            return kernelCorrelationId;
        }

        // Priority 2: Incoming request header
        if (headerCorrelationId is not null)
        {
            return headerCorrelationId;
        }

        // Priority 3: Generate a new one if allowed
        if (!_options.GenerateCorrelationIdIfMissing)
        {
            return string.Empty;
        }

        return Activity.Current?.Id ?? Guid.NewGuid().ToString("N");
    }
}
