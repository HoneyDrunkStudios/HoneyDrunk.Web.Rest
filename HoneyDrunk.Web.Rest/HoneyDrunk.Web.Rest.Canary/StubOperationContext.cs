using HoneyDrunk.Kernel.Abstractions.Context;

namespace HoneyDrunk.Web.Rest.Canary;

/// <summary>
/// A minimal <see cref="IOperationContext"/> stub that provides only a correlation ID.
/// Does not implement real Kernel behavior — used exclusively for canary correlation mismatch testing.
/// </summary>
internal sealed class StubOperationContext(string correlationId) : IOperationContext
{
    /// <inheritdoc/>
    public IGridContext GridContext { get; } = new StubGridContext();

    /// <inheritdoc/>
    public string OperationName => "canary-stub";

    /// <inheritdoc/>
    public string OperationId => "canary-op-id";

    /// <inheritdoc/>
    public string CorrelationId => correlationId;

    /// <inheritdoc/>
    public string? CausationId => null;

    /// <inheritdoc/>
    public string? TenantId => null;

    /// <inheritdoc/>
    public string? ProjectId => null;

    /// <inheritdoc/>
    public DateTimeOffset StartedAtUtc => DateTimeOffset.UtcNow;

    /// <inheritdoc/>
    public DateTimeOffset? CompletedAtUtc => null;

    /// <inheritdoc/>
    public bool? IsSuccess => null;

    /// <inheritdoc/>
    public string? ErrorMessage => null;

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, object?> Metadata => new Dictionary<string, object?>();

    /// <inheritdoc/>
    public void Complete()
    {
    }

    /// <inheritdoc/>
    public void Fail(string errorMessage, Exception? exception = null)
    {
    }

    /// <inheritdoc/>
    public void AddMetadata(string key, object? value)
    {
    }

    /// <inheritdoc/>
    public void Dispose()
    {
    }
}
