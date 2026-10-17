using HoneyDrunk.Kernel.Abstractions.Context;
using HoneyDrunk.Web.Rest.AspNetCore.Configuration;
using HoneyDrunk.Web.Rest.AspNetCore.Context;
using HoneyDrunk.Web.Rest.AspNetCore.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Shouldly;

namespace HoneyDrunk.Web.Rest.Tests.AspNetCore;

/// <summary>
/// Tests for Kernel-backed correlation middleware behavior.
/// </summary>
public sealed class CorrelationMiddlewareTests
{
    /// <summary>
    /// Verifies that correlation middleware fails fast when Kernel did not establish an operation context.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task InvokeAsync_WithoutCurrentOperationContext_Throws()
    {
        CorrelationMiddleware middleware = new(
            _ => Task.CompletedTask,
            Options.Create(new RestOptions()),
            NullLogger<CorrelationMiddleware>.Instance);

        InvalidOperationException exception = await Should.ThrowAsync<InvalidOperationException>(() =>
            middleware.InvokeAsync(
                new DefaultHttpContext(),
                new CorrelationIdAccessor(),
                new TestOperationContextAccessor()))
;

        exception.Message.ShouldContain("live Kernel IOperationContext");
        exception.Message.ShouldContain("UseGridContext()");
    }

    private sealed class TestOperationContextAccessor : IOperationContextAccessor
    {
        public IOperationContext? Current { get; set; }
    }
}
