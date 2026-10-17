using HoneyDrunk.Vault.EventGrid.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace HoneyDrunk.Web.Rest.AspNetCore.Extensions;

/// <summary>
/// Extension methods for mapping HoneyDrunk Web.Rest internal endpoints.
/// </summary>
public static class HoneyDrunkWebRestEndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps the internal Vault invalidation webhook used by Event Grid secret rotation notifications.
    /// </summary>
    /// <param name="endpoints">The route builder.</param>
    /// <returns>The endpoint convention builder.</returns>
    public static IEndpointConventionBuilder MapHoneyDrunkWebRestVaultInvalidationWebhook(
        this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        return endpoints.MapVaultInvalidationWebhook("/internal/vault/invalidate");
    }
}
