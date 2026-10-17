namespace HoneyDrunk.Web.Rest.AspNetCore.Context;

/// <summary>
/// Default implementation of <see cref="ICorrelationIdAccessor"/> using AsyncLocal.
/// </summary>
public sealed class CorrelationIdAccessor : ICorrelationIdAccessor
{
    private static readonly AsyncLocal<string?> CurrentCorrelationId = new();

    /// <inheritdoc/>
    public string? CorrelationId => CurrentCorrelationId.Value;

    /// <inheritdoc/>
    public void SetCorrelationId(string correlationId)
    {
        CurrentCorrelationId.Value = correlationId;
    }
}
