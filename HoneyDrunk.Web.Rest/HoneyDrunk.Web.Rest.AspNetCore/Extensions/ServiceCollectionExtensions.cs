using HoneyDrunk.Web.Rest.AspNetCore.Auth;
using HoneyDrunk.Web.Rest.AspNetCore.Configuration;
using HoneyDrunk.Web.Rest.AspNetCore.Context;
using HoneyDrunk.Web.Rest.AspNetCore.Mvc;
using HoneyDrunk.Web.Rest.AspNetCore.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HoneyDrunk.Web.Rest.AspNetCore.Extensions;

/// <summary>
/// Extension methods for configuring REST services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds REST services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional configuration for REST options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRest(
        this IServiceCollection services,
        Action<RestOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<ICorrelationIdAccessor, CorrelationIdAccessor>();

        if (configure is not null)
        {
            services.Configure(configure);
        }
        else
        {
            services.Configure<RestOptions>(_ => { });
        }

        RestOptions options = new();
        configure?.Invoke(options);

        if (options.ConfigureJsonDefaults)
        {
            services.Configure<JsonOptions>(jsonOptions =>
            {
                JsonOptionsDefaults.Configure(jsonOptions.JsonSerializerOptions);
            });
        }

        if (options.EnableModelStateValidationFilter)
        {
            services.Configure<MvcOptions>(mvcOptions =>
            {
                mvcOptions.Filters.Add<ModelStateValidationFilter>();
            });

            services.Configure<ApiBehaviorOptions>(apiBehaviorOptions =>
            {
                apiBehaviorOptions.SuppressModelStateInvalidFilter = true;
            });
        }

        if (options.EnableAuthFailureShaping)
        {
            services.TryAddSingleton<IAuthorizationMiddlewareResultHandler, RestAuthorizationResultHandler>();
        }

        return services;
    }
}
