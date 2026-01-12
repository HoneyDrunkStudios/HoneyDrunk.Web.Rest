using HoneyDrunk.Web.Rest.AspNetCore.Extensions;
using HoneyDrunk.Web.Rest.AspNetCore.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HoneyDrunk.Web.Rest.Tests.TestHost;

/// <summary>
/// Test host factory for testing REST middleware and filters.
/// </summary>
public sealed class TestApiFactory : IDisposable
{
    private IHost? _host;
    private HttpClient? _client;

    /// <summary>
    /// Gets or sets a value indicating whether to include exception details in responses.
    /// </summary>
    public bool IncludeExceptionDetails { get; set; }

    /// <summary>
    /// Creates an HTTP client for testing.
    /// </summary>
    /// <returns>An HTTP client configured for the test server.</returns>
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
                        options.IncludeExceptionDetails = IncludeExceptionDetails;
                        options.EnableRequestLoggingScope = true;
                        options.EnableExceptionMapping = true;
                        options.EnableModelStateValidationFilter = true;
                    });

                    // Configure JSON options for minimal APIs
                    services.Configure<JsonOptions>(options =>
                    {
                        JsonOptionsDefaults.Configure(options.SerializerOptions);
                    });

                    services.AddControllers();
                    services.AddRouting();
                });

                webBuilder.Configure(app =>
                {
                    app.UseRest();
                    app.UseRouting();

                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapTestEndpoints();
                        endpoints.MapControllers();
                    });
                });
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
}
