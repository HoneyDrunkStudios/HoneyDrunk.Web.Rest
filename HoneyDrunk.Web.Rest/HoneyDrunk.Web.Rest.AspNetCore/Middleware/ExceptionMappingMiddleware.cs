using HoneyDrunk.Web.Rest.Abstractions.Constants;
using HoneyDrunk.Web.Rest.Abstractions.Errors;
using HoneyDrunk.Web.Rest.AspNetCore.Configuration;
using HoneyDrunk.Web.Rest.AspNetCore.Context;
using HoneyDrunk.Web.Rest.AspNetCore.Errors;
using HoneyDrunk.Web.Rest.AspNetCore.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text.Json;

namespace HoneyDrunk.Web.Rest.AspNetCore.Middleware;

/// <summary>
/// Middleware that catches unhandled exceptions and maps them to <see cref="ApiErrorResponse"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ExceptionMappingMiddleware"/> class.
/// </remarks>
/// <param name="next">The next middleware in the pipeline.</param>
/// <param name="options">The REST options.</param>
/// <param name="environment">The host environment.</param>
/// <param name="logger">The logger.</param>
public sealed class ExceptionMappingMiddleware(
    RequestDelegate next,
    IOptions<RestOptions> options,
    IHostEnvironment environment,
    ILogger<ExceptionMappingMiddleware> logger)
{
    private readonly RestOptions _options = options.Value;

    /// <summary>
    /// Invokes the middleware.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="correlationIdAccessor">The correlation ID accessor.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context, ICorrelationIdAccessor correlationIdAccessor)
    {
        try
        {
            await next(context).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex, correlationIdAccessor).ConfigureAwait(false);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception, ICorrelationIdAccessor correlationIdAccessor)
    {
        // Try to get correlation ID from multiple sources in order of preference:
        // 1. The accessor (set by CorrelationMiddleware)
        // 2. HttpContext.Items (set by CorrelationMiddleware)
        // 3. Generate a new one
        string? correlationId = correlationIdAccessor.CorrelationId;

        if (string.IsNullOrEmpty(correlationId) &&
            context.Items.TryGetValue(HeaderNames.CorrelationId, out object? contextCorrelationId))
        {
            correlationId = contextCorrelationId?.ToString();
        }

        correlationId ??= Guid.NewGuid().ToString("N");

        string? traceId = _options.IncludeTraceId ? Activity.Current?.Id : null;

        logger.LogError(
            exception,
            "An unhandled exception occurred. CorrelationId: {CorrelationId}, TraceId: {TraceId}",
            correlationId,
            traceId);

        ExceptionMappingResult mapping = ExceptionToApiErrorMapper.Map(exception);

        bool includeDetails = _options.IncludeExceptionDetails && environment.IsDevelopment();
        string? details = includeDetails ? exception.ToString() : null;

        ApiErrorResponse errorResponse = new()
        {
            CorrelationId = correlationId,
            TraceId = traceId,
            Error = new ApiError
            {
                Code = mapping.ErrorCode,
                Message = mapping.Message,
                Details = details,
            },
        };

        context.Response.StatusCode = (int)mapping.StatusCode;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(errorResponse, JsonOptionsDefaults.SerializerOptions),
            context.RequestAborted).ConfigureAwait(false);
    }
}
