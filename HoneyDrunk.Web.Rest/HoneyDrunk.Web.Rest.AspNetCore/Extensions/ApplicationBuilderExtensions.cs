using HoneyDrunk.Web.Rest.AspNetCore.Configuration;
using HoneyDrunk.Web.Rest.AspNetCore.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HoneyDrunk.Web.Rest.AspNetCore.Extensions;

/// <summary>
/// Extension methods for configuring REST middleware.
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Adds REST middleware to the application pipeline.
    /// This should be called early in the pipeline, before routing.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    /// <remarks>
    /// The middleware is added in the following order:
    /// <list type="number">
    /// <item><description>Correlation ID handling.</description></item>
    /// <item><description>Exception mapping (catches all exceptions).</description></item>
    /// <item><description>Request logging scope.</description></item>
    /// </list>
    /// </remarks>
    public static IApplicationBuilder UseRest(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        RestOptions options = app.ApplicationServices
            .GetService<IOptions<RestOptions>>()?.Value ?? new RestOptions();

        // Correlation middleware first to ensure correlation ID is available for all subsequent middleware
        app.UseMiddleware<CorrelationMiddleware>();

        if (options.EnableExceptionMapping)
        {
            app.UseMiddleware<ExceptionMappingMiddleware>();
        }

        if (options.EnableRequestLoggingScope)
        {
            app.UseMiddleware<RequestLoggingScopeMiddleware>();
        }

        return app;
    }
}
