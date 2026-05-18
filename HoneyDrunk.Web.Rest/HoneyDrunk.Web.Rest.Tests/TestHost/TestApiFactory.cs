using HoneyDrunk.Kernel.Abstractions.Context;
using HoneyDrunk.Kernel.Abstractions.Identity;
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
                    services.AddSingleton<IOperationContextAccessor>(new TestOperationContextAccessor());

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
                    app.Use(async (context, next) =>
                    {
                        IOperationContextAccessor accessor = context.RequestServices.GetRequiredService<IOperationContextAccessor>();
                        string correlationId = context.Request.Headers.TryGetValue("X-Correlation-Id", out Microsoft.Extensions.Primitives.StringValues headerValue)
                            && !string.IsNullOrWhiteSpace(headerValue.ToString())
                            ? headerValue.ToString()
                            : Guid.NewGuid().ToString("N");

                        accessor.Current = new TestOperationContext(correlationId);

                        try
                        {
                            await next(context).ConfigureAwait(false);
                        }
                        finally
                        {
                            accessor.Current = null;
                        }
                    });

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

    private sealed class TestOperationContextAccessor : IOperationContextAccessor
    {
        public IOperationContext? Current { get; set; }
    }

    private sealed class TestOperationContext(string correlationId) : IOperationContext
    {
        public IGridContext GridContext { get; } = new TestGridContext(correlationId);

        public string OperationName => "test-request";

        public string OperationId => "test-operation-id";

        public string CorrelationId => GridContext.CorrelationId;

        public string? CausationId => null;

        public TenantId TenantId => TenantId.Internal;

        public string? ProjectId => null;

        public DateTimeOffset StartedAtUtc { get; } = DateTimeOffset.UtcNow;

        public DateTimeOffset? CompletedAtUtc => null;

        public bool? IsSuccess => null;

        public string? ErrorMessage => null;

        public IReadOnlyDictionary<string, object?> Metadata { get; } = new Dictionary<string, object?>();

        public void Complete()
        {
        }

        public void Fail(string errorMessage, Exception? exception = null)
        {
        }

        public void AddMetadata(string key, object? value)
        {
        }

        public void Dispose()
        {
        }
    }

    private sealed class TestGridContext(string correlationId) : IGridContext
    {
        public bool IsInitialized => true;

        public string CorrelationId => correlationId;

        public string? CausationId => null;

        public string NodeId => "test-node";

        public string StudioId => "test-studio";

        public string Environment => "test";

        public TenantId TenantId => TenantId.Internal;

        public string? ProjectId => null;

        public CancellationToken Cancellation => CancellationToken.None;

        public IReadOnlyDictionary<string, string> Baggage { get; } = new Dictionary<string, string>();

        public DateTimeOffset CreatedAtUtc { get; } = DateTimeOffset.UtcNow;

        public void AddBaggage(string key, string value)
        {
        }
    }
}
