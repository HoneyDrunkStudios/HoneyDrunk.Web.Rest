using HoneyDrunk.Vault.EventGrid.Constants;
using Shouldly;
using System.Net;
using System.Text;

namespace HoneyDrunk.Web.Rest.Canary;

/// <summary>
/// ADR-0006: Vault invalidation webhook registration and Event Grid cache invalidation behavior.
/// </summary>
public sealed class VaultInvalidationWebhookCanary
{
    /// <summary>
    /// Verifies that the Web.Rest invalidation endpoint accepts Event Grid events and invalidates the named secret.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test.</returns>
    [Fact]
    public async Task VaultInvalidationWebhook_InvalidatesSecret_ForKeyVaultSecretNewVersionEvent()
    {
        using CanaryHostFactory factory = new()
        {
            EnableVaultInvalidationWebhook = true,
        };

        HttpClient client = factory.CreateClient();
        const string secretName = "DownstreamApi--ApiKey";
        const string requestBody =
            """
            [
              {
                "id": "evt-secret-1",
                "eventType": "Microsoft.KeyVault.SecretNewVersionCreated",
                "subject": "DownstreamApi--ApiKey",
                "eventTime": "2026-04-12T12:00:00Z",
                "data": {
                  "id": "https://kv-hd-webrest-dev.vault.azure.net/secrets/DownstreamApi--ApiKey/version1",
                  "vaultName": "kv-hd-webrest-dev",
                  "objectType": "Secret",
                  "objectName": "DownstreamApi--ApiKey"
                },
                "dataVersion": "1",
                "metadataVersion": "1"
              }
            ]
            """;

        using HttpRequestMessage request = new(
            HttpMethod.Post,
            new Uri("/internal/vault/invalidate", UriKind.Relative));
        request.Headers.Add(
            VaultInvalidationWebhookConstants.SharedSecretHeaderName,
            "expected-shared-secret");
        request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await client.SendAsync(request);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        factory.VaultCacheInvalidator.InvalidatedSecrets.ShouldContain(secretName);
    }
}
