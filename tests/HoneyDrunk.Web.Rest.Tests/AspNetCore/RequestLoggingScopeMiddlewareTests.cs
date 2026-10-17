using HoneyDrunk.Kernel.Abstractions.Context;
using HoneyDrunk.Kernel.Abstractions.Identity;
using HoneyDrunk.Web.Rest.Abstractions.Telemetry;
using HoneyDrunk.Web.Rest.AspNetCore.Configuration;
using HoneyDrunk.Web.Rest.AspNetCore.Context;
using HoneyDrunk.Web.Rest.AspNetCore.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shouldly;

namespace HoneyDrunk.Web.Rest.Tests.AspNetCore;

/// <summary>
/// Tests for request logging scope enrichment.
/// </summary>
public sealed class RequestLoggingScopeMiddlewareTests
{
    /// <summary>
    /// Verifies that Internal tenant contexts omit the TenantId scope entry.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task InvokeAsync_WithInternalTenant_OmitsTenantIdScopeEntry()
    {
        TestOperationContext operationContext = new(TenantId.Internal);

        Dictionary<string, object?> scopeState = await InvokeAndCaptureScopeAsync(operationContext);

        scopeState.ContainsKey(RestTelemetryTags.TenantId).ShouldBeFalse();
    }

    /// <summary>
    /// Verifies that non-internal tenant contexts emit the TenantId scope entry as a ULID string.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task InvokeAsync_WithNonInternalTenant_EmitsTenantIdScopeEntryAsUlidString()
    {
        TenantId tenantId = TenantId.NewId();
        TestOperationContext operationContext = new(tenantId);

        Dictionary<string, object?> scopeState = await InvokeAndCaptureScopeAsync(operationContext);

        scopeState[RestTelemetryTags.TenantId].ShouldBe(tenantId.ToString());
    }

    /// <summary>
    /// Verifies that ProjectId scope behavior remains unchanged.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task InvokeAsync_WithProjectId_PreservesProjectIdBehavior()
    {
        TestOperationContext withProject = new(TenantId.NewId()) { ProjectIdValue = "project-123" };
        TestOperationContext withoutProject = new(TenantId.NewId()) { ProjectIdValue = null };

        Dictionary<string, object?> withProjectScope = await InvokeAndCaptureScopeAsync(withProject);
        Dictionary<string, object?> withoutProjectScope = await InvokeAndCaptureScopeAsync(withoutProject);

        withProjectScope["ProjectId"].ShouldBe("project-123");
        withoutProjectScope.ContainsKey("ProjectId").ShouldBeFalse();
    }

    /// <summary>
    /// Verifies that existing scope properties continue to be emitted.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task InvokeAsync_WithKernelContext_PreservesOtherScopeProperties()
    {
        TestOperationContext operationContext = new(TenantId.NewId())
        {
            OperationIdValue = "operation-123",
            OperationNameValue = "request-test",
            CausationIdValue = "causation-123",
            GridContextValue = new TestGridContext(TenantId.NewId())
            {
                NodeIdValue = "node-123",
                StudioIdValue = "studio-123",
                EnvironmentValue = "test",
            },
        };

        Dictionary<string, object?> scopeState = await InvokeAndCaptureScopeAsync(operationContext);

        scopeState[RestTelemetryTags.CorrelationId].ShouldBe("correlation-123");
        scopeState[RestTelemetryTags.HttpMethod].ShouldBe("POST");
        scopeState[RestTelemetryTags.HttpPath].ShouldBe("/scope-test");
        scopeState[RestTelemetryTags.RequestId].ShouldBe("trace-123");
        scopeState["OperationId"].ShouldBe("operation-123");
        scopeState["OperationName"].ShouldBe("request-test");
        scopeState["CausationId"].ShouldBe("causation-123");
        scopeState["NodeId"].ShouldBe("node-123");
        scopeState["StudioId"].ShouldBe("studio-123");
        scopeState["Environment"].ShouldBe("test");
    }

    private static async Task<Dictionary<string, object?>> InvokeAndCaptureScopeAsync(TestOperationContext operationContext)
    {
        CapturingLogger<RequestLoggingScopeMiddleware> logger = new();
        RequestLoggingScopeMiddleware middleware = new(
            _ => Task.CompletedTask,
            logger,
            Options.Create(new RestOptions { IncludeTraceId = false }));

        CorrelationIdAccessor correlationIdAccessor = new();
        correlationIdAccessor.SetCorrelationId("correlation-123");

        ServiceProvider services = new ServiceCollection()
            .AddSingleton<IOperationContextAccessor>(new TestOperationContextAccessor { Current = operationContext })
            .BuildServiceProvider();

        try
        {
            DefaultHttpContext context = new() { RequestServices = services, TraceIdentifier = "trace-123" };
            context.Request.Method = "POST";
            context.Request.Path = "/scope-test";

            await middleware.InvokeAsync(context, correlationIdAccessor).ConfigureAwait(false);
        }
        finally
        {
            await services.DisposeAsync().ConfigureAwait(false);
        }

        return logger.ScopeState.ShouldNotBeNull();
    }

    private sealed class TestOperationContextAccessor : IOperationContextAccessor
    {
        public IOperationContext? Current { get; set; }
    }

    private sealed class TestOperationContext(TenantId tenantId) : IOperationContext
    {
        public IGridContext GridContextValue { get; init; } = new TestGridContext(tenantId);

        public string OperationNameValue { get; init; } = "operation-name";

        public string OperationIdValue { get; init; } = "operation-id";

        public string? CausationIdValue { get; init; }

        public string? ProjectIdValue { get; init; }

        public IGridContext GridContext => GridContextValue;

        public string OperationName => OperationNameValue;

        public string OperationId => OperationIdValue;

        public string CorrelationId => GridContext.CorrelationId;

        public string? CausationId => CausationIdValue;

        public TenantId TenantId => tenantId;

        public string? ProjectId => ProjectIdValue;

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

    private sealed class TestGridContext(TenantId tenantId) : IGridContext
    {
        public bool IsInitialized => true;

        public string CorrelationId => "correlation-123";

        public string? CausationId => null;

        public string NodeIdValue { get; init; } = "node-id";

        public string StudioIdValue { get; init; } = "studio-id";

        public string EnvironmentValue { get; init; } = "test";

        public string NodeId => NodeIdValue;

        public string StudioId => StudioIdValue;

        public string Environment => EnvironmentValue;

        public TenantId TenantId => tenantId;

        public string? ProjectId => null;

        public CancellationToken Cancellation => CancellationToken.None;

        public IReadOnlyDictionary<string, string> Baggage { get; } = new Dictionary<string, string>();

        public DateTimeOffset CreatedAtUtc { get; } = DateTimeOffset.UtcNow;

        public void AddBaggage(string key, string value)
        {
        }
    }

    private sealed class CapturingLogger<T> : ILogger<T>
    {
        public Dictionary<string, object?>? ScopeState { get; private set; }

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
        {
            ScopeState = state as Dictionary<string, object?>;
            return NullDisposable.Instance;
        }

        public bool IsEnabled(LogLevel logLevel) => false;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
        }
    }

    private sealed class NullDisposable : IDisposable
    {
        public static readonly NullDisposable Instance = new();

        public void Dispose()
        {
        }
    }
}
