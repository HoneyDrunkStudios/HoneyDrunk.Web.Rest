using HoneyDrunk.Kernel.Abstractions.Context;
using HoneyDrunk.Vault.Abstractions;
using HoneyDrunk.Vault.EventGrid.Extensions;
using HoneyDrunk.Vault.Models;
using HoneyDrunk.Web.Rest.AspNetCore.Extensions;
using HoneyDrunk.Web.Rest.AspNetCore.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HoneyDrunk.Web.Rest.Canary;

/// <summary>
/// Creates an in-process test server for canary verification.
/// Supports optional registration of a fake <see cref="IOperationContextAccessor"/>
/// to simulate Kernel correlation presence without creating Kernel context objects.
/// </summary>
internal sealed class CanaryHostFactory : IDisposable
{
    private IHost? _host;
    private HttpClient? _client;

    /// <summary>
    /// Gets or sets an optional fake <see cref="IOperationContextAccessor"/> to register.
    /// Set before calling <see cref="CreateClient"/>.
    /// </summary>
    public IOperationContextAccessor? OperationContextAccessor { get; set; }

    /// <summary>
    /// Gets captured log entries from the most recent host.
    /// </summary>
    public CapturingLoggerProvider LogCapture { get; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether to register auth services with a protected
    /// endpoint but NO <c>IAuthenticatedIdentityAccessor</c>, forcing the fallback to HttpContext.User.
    /// </summary>
    public bool UseAuthWithoutIdentityAccessor { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to register the Vault invalidation webhook canary endpoint.
    /// </summary>
    public bool EnableVaultInvalidationWebhook { get; set; }

    /// <summary>
    /// Gets the cache invalidator used by the Vault invalidation webhook canary.
    /// </summary>
    public RecordingSecretCacheInvalidator VaultCacheInvalidator { get; } = new();

    /// <summary>
    /// Creates an HTTP client backed by the in-process test server.
    /// </summary>
    public HttpClient CreateClient()
    {
        if (_client is not null)
        {
            return _client;
        }

        _host = Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseTestServer();
                webBuilder.ConfigureServices(services =>
                {
                    services.AddRest(options =>
                    {
                        options.IncludeExceptionDetails = false;
                        options.EnableRequestLoggingScope = true;
                        options.EnableExceptionMapping = true;
                        options.EnableAuthFailureShaping = true;
                    });

                    services.Configure<JsonOptions>(options =>
                    {
                        JsonOptionsDefaults.Configure(options.SerializerOptions);
                    });

                    services.AddRouting();

                    // Register fake operation context accessor if provided
                    if (OperationContextAccessor is not null)
                    {
                        services.AddSingleton(OperationContextAccessor);
                    }

                    // Auth setup
                    if (UseAuthWithoutIdentityAccessor)
                    {
                        services.AddAuthentication("CanaryScheme")
                            .AddScheme<
                                Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions,
                                CanaryAuthHandler>("CanaryScheme", _ => { });

                        services.AddAuthorizationBuilder()
                            .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
                    }

                    if (EnableVaultInvalidationWebhook)
                    {
                        services.AddSingleton<ISecretStore>(new SharedSecretStore());
                        services.AddSingleton<ISecretCacheInvalidator>(VaultCacheInvalidator);
                        services.AddVaultEventGridInvalidation();
                    }
                });

                webBuilder.Configure(app =>
                {
                    app.UseRest();
                    app.UseRouting();

                    if (UseAuthWithoutIdentityAccessor)
                    {
                        app.UseAuthentication();
                        app.UseAuthorization();
                    }

                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapCanaryEndpoints();

                        if (EnableVaultInvalidationWebhook)
                        {
                            endpoints.MapHoneyDrunkWebRestVaultInvalidationWebhook();
                        }

                        if (UseAuthWithoutIdentityAccessor)
                        {
                            endpoints.MapGet("/canary/auth/protected", () => Results.Ok("ok"))
                                .RequireAuthorization("AdminOnly");
                        }
                    });
                });
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddProvider(LogCapture);
                logging.SetMinimumLevel(LogLevel.Trace);
            })
            .Start();

        _client = _host.GetTestClient();
        return _client;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _client?.Dispose();
        _host?.Dispose();
    }

    internal sealed class RecordingSecretCacheInvalidator : ISecretCacheInvalidator
    {
        public List<string> InvalidatedSecrets { get; } = [];

        public void Invalidate(string secretName)
        {
            InvalidatedSecrets.Add(secretName);
        }

        public void InvalidateAll()
        {
            InvalidatedSecrets.Add("*");
        }
    }

    private sealed class SharedSecretStore : ISecretStore
    {
        public const string SharedSecretValue = "expected-shared-secret";

        public Task<SecretValue> GetSecretAsync(
            SecretIdentifier identifier,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new SecretValue(identifier, SharedSecretValue, "v1"));
        }

        public Task<VaultResult<SecretValue>> TryGetSecretAsync(
            SecretIdentifier identifier,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(VaultResult.Success(new SecretValue(identifier, SharedSecretValue, "v1")));
        }

        public Task<IReadOnlyList<SecretVersion>> ListSecretVersionsAsync(
            string secretName,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<SecretVersion>>([]);
        }
    }
}
