using HoneyDrunk.Kernel.Abstractions.Context;
using HoneyDrunk.Web.Rest.Abstractions.Constants;
using HoneyDrunk.Web.Rest.AspNetCore.Configuration;
using HoneyDrunk.Web.Rest.AspNetCore.Context;
using HoneyDrunk.Web.Rest.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HoneyDrunk.Web.Rest.AspNetCore.Middleware;

/// <summary>
/// Middleware that handles correlation ID propagation.
/// Requires a live Kernel operation context, mirrors its correlation ID into Web.Rest accessors,
/// and returns it in the response header.
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
    /// <param name="operationContextAccessor">The Kernel operation context accessor.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(
        HttpContext context,
        ICorrelationIdAccessor correlationIdAccessor,
        IOperationContextAccessor operationContextAccessor)
    {
        string correlationId = GetCorrelationId(context, operationContextAccessor);

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

    private string GetCorrelationId(HttpContext context, IOperationContextAccessor operationContextAccessor)
    {
        IOperationContext operationContext = operationContextAccessor.Current
            ?? throw new InvalidOperationException(
                "HoneyDrunk.Web.Rest requires a live Kernel IOperationContext for each request. "
                + "Register HoneyDrunk.Kernel with AddHoneyDrunkNode() and place UseGridContext() before UseRest().");

        string kernelCorrelationId = !string.IsNullOrWhiteSpace(operationContext.CorrelationId)
            ? operationContext.CorrelationId
            : throw new InvalidOperationException("HoneyDrunk.Web.Rest requires Kernel IOperationContext.CorrelationId to be populated.");

        string? headerCorrelationId = null;
        if (context.Request.Headers.TryGetValue(_options.CorrelationIdHeaderName, out Microsoft.Extensions.Primitives.StringValues headerValue)
            && !string.IsNullOrWhiteSpace(headerValue.ToString()))
        {
            headerCorrelationId = headerValue.ToString();
        }

        if (headerCorrelationId is not null
            && !string.Equals(kernelCorrelationId, headerCorrelationId, StringComparison.Ordinal))
        {
            logger.LogWarning(
                "Correlation ID mismatch: header '{HeaderCorrelationId}' differs from Kernel context '{KernelCorrelationId}'. "
                + "Kernel correlation takes precedence. Request: {HttpMethod} {RequestPath}",
                LogValueSanitizer.Sanitize(headerCorrelationId),
                LogValueSanitizer.Sanitize(kernelCorrelationId),
                LogValueSanitizer.Sanitize(context.Request.Method),
                LogValueSanitizer.Sanitize(context.Request.Path.Value));
        }

        return kernelCorrelationId;
    }
}
