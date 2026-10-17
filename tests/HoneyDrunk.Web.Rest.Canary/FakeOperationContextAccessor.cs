using HoneyDrunk.Kernel.Abstractions.Context;

namespace HoneyDrunk.Web.Rest.Canary;

/// <summary>
/// A fake <see cref="IOperationContextAccessor"/> that returns a mock operation context
/// with a configurable correlation ID. Does NOT create a real Kernel context —
/// uses interface-only mocking to satisfy the DI contract.
/// </summary>
internal sealed class FakeOperationContextAccessor : IOperationContextAccessor
{
    /// <inheritdoc/>
    public IOperationContext? Current { get; set; }
}
