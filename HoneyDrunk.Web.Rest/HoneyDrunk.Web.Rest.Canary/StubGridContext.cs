using HoneyDrunk.Kernel.Abstractions.Context;

namespace HoneyDrunk.Web.Rest.Canary;

/// <summary>
/// A minimal <see cref="IGridContext"/> stub that returns empty/default values.
/// Used by <see cref="StubOperationContext"/> to satisfy RequestLoggingScopeMiddleware
/// which accesses GridContext properties for telemetry enrichment.
/// </summary>
internal sealed class StubGridContext : IGridContext
{
    /// <inheritdoc/>
    public bool IsInitialized => true;

    /// <inheritdoc/>
    public string CorrelationId => string.Empty;

    /// <inheritdoc/>
    public string? CausationId => null;

    /// <inheritdoc/>
    public string NodeId => "canary-stub";

    /// <inheritdoc/>
    public string StudioId => "canary-studio";

    /// <inheritdoc/>
    public string Environment => "test";

    /// <inheritdoc/>
    public string? TenantId => null;

    /// <inheritdoc/>
    public string? ProjectId => null;

    /// <inheritdoc/>
    public CancellationToken Cancellation => CancellationToken.None;

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, string> Baggage => new Dictionary<string, string>();

    /// <inheritdoc/>
    public DateTimeOffset CreatedAtUtc => DateTimeOffset.UtcNow;

    /// <inheritdoc/>
    public void AddBaggage(string key, string value)
    {
    }
}
