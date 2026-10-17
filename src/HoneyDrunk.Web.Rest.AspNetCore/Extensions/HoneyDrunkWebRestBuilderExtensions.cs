using HoneyDrunk.Kernel.Abstractions.Hosting;
using HoneyDrunk.Vault.EventGrid.Extensions;
using HoneyDrunk.Vault.Providers.AppConfiguration.Extensions;
using HoneyDrunk.Vault.Providers.AzureKeyVault.Extensions;

namespace HoneyDrunk.Web.Rest.AspNetCore.Extensions;

/// <summary>
/// Extension methods for bootstrapping HoneyDrunk Web.Rest in deployable nodes.
/// </summary>
public static class HoneyDrunkWebRestBuilderExtensions
{
    /// <summary>
    /// Adds ADR-0005 compliant Web.Rest bootstrap wiring.
    /// </summary>
    /// <param name="builder">The HoneyDrunk builder.</param>
    /// <returns>The builder for chaining.</returns>
    /// <remarks>
    /// This wires Key Vault from <c>AZURE_KEYVAULT_URI</c>, App Configuration from
    /// <c>AZURE_APPCONFIG_ENDPOINT</c> using the <c>HONEYDRUNK_NODE_ID</c> label, Web.Rest services,
    /// and the services required by the ADR-0006 Vault invalidation webhook.
    /// </remarks>
    public static IHoneyDrunkBuilder AddWebRestBootstrap(this IHoneyDrunkBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.AddVaultWithAzureKeyVaultBootstrap();
        builder.AddAppConfiguration();
        builder.Services.AddRest();
        builder.Services.AddVaultEventGridInvalidation();

        return builder;
    }
}
