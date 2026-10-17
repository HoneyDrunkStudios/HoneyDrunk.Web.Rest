using HoneyDrunk.Web.Rest.AspNetCore.Context;
using Shouldly;

namespace HoneyDrunk.Web.Rest.Tests.AspNetCore;

/// <summary>
/// Tests for <see cref="CorrelationIdAccessor"/>.
/// </summary>
public sealed class CorrelationIdAccessorTests
{
    /// <summary>
    /// Verifies that CorrelationId is null by default.
    /// </summary>
    [Fact]
    public void CorrelationId_IsNullByDefault()
    {
        CorrelationIdAccessor accessor = new();

        accessor.CorrelationId.ShouldBeNull();
    }

    /// <summary>
    /// Verifies that SetCorrelationId sets the value.
    /// </summary>
    [Fact]
    public void SetCorrelationId_SetsValue()
    {
        CorrelationIdAccessor accessor = new();

        accessor.SetCorrelationId("test-correlation-id");

        accessor.CorrelationId.ShouldBe("test-correlation-id");
    }

    /// <summary>
    /// Verifies that SetCorrelationId can be called multiple times.
    /// </summary>
    [Fact]
    public void SetCorrelationId_CanBeCalledMultipleTimes()
    {
        CorrelationIdAccessor accessor = new();

        accessor.SetCorrelationId("first-id");
        accessor.CorrelationId.ShouldBe("first-id");

        accessor.SetCorrelationId("second-id");
        accessor.CorrelationId.ShouldBe("second-id");
    }

    /// <summary>
    /// Verifies that CorrelationId flows across async boundaries.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task CorrelationId_FlowsAcrossAsyncBoundaries()
    {
        CorrelationIdAccessor accessor = new();
        accessor.SetCorrelationId("async-test-id");

        string? capturedId = null;

        await Task.Run(() =>
        {
            capturedId = accessor.CorrelationId;
        });

        capturedId.ShouldBe("async-test-id");
    }

    /// <summary>
    /// Verifies that different async contexts can set correlation IDs.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task CorrelationId_CanBeSetInDifferentContexts()
    {
        CorrelationIdAccessor accessor1 = new();
        CorrelationIdAccessor accessor2 = new();

        TaskCompletionSource bothContextsSet = new(TaskCreationOptions.RunContinuationsAsynchronously);
        int contextsSet = 0;

        Task<string?> task1 = Task.Run(async () =>
        {
            accessor1.SetCorrelationId("context-1-id");
            SignalContextSet();
            await bothContextsSet.Task;
            return accessor1.CorrelationId;
        });

        Task<string?> task2 = Task.Run(async () =>
        {
            accessor2.SetCorrelationId("context-2-id");
            SignalContextSet();
            await bothContextsSet.Task;
            return accessor2.CorrelationId;
        });

        string?[] capturedIds = await Task.WhenAll(task1, task2);

        capturedIds[0].ShouldBe("context-1-id");
        capturedIds[1].ShouldBe("context-2-id");

        void SignalContextSet()
        {
            if (Interlocked.Increment(ref contextsSet) == 2)
            {
                bothContextsSet.SetResult();
            }
        }
    }

    /// <summary>
    /// Verifies that accessor implements ICorrelationIdAccessor.
    /// </summary>
    [Fact]
    public void Accessor_ImplementsInterface()
    {
        CorrelationIdAccessor accessor = new();

        accessor.ShouldBeAssignableTo<ICorrelationIdAccessor>();
    }

    /// <summary>
    /// Verifies that SetCorrelationId with empty string works.
    /// </summary>
    [Fact]
    public void SetCorrelationId_WithEmptyString_Works()
    {
        CorrelationIdAccessor accessor = new();

        accessor.SetCorrelationId(string.Empty);

        accessor.CorrelationId.ShouldBe(string.Empty);
    }
}
