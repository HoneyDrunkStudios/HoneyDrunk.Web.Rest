using HoneyDrunk.Kernel.Abstractions.Context;
using HoneyDrunk.Web.Rest.AspNetCore.Context;
using HoneyDrunk.Web.Rest.AspNetCore.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace HoneyDrunk.Web.Rest.Tests.AspNetCore;

/// <summary>
/// Tests for REST service registration requirements.
/// </summary>
public sealed class ServiceCollectionExtensionsTests
{
    /// <summary>
    /// Verifies that AddRest fails fast when Kernel request context services are missing.
    /// </summary>
    [Fact]
    public void AddRest_WithoutKernelOperationContextAccessor_Throws()
    {
        ServiceCollection services = new();

        InvalidOperationException exception = Should.Throw<InvalidOperationException>(() => services.AddRest());

        exception.Message.ShouldContain("AddHoneyDrunkNode()");
        exception.Message.ShouldContain("UseGridContext()");
    }

    /// <summary>
    /// Verifies that AddRest registers Web.Rest services when Kernel request context services are present.
    /// </summary>
    [Fact]
    public void AddRest_WithKernelOperationContextAccessor_RegistersRestServices()
    {
        ServiceCollection services = new();
        services.AddSingleton<IOperationContextAccessor>(new TestOperationContextAccessor());

        services.AddRest();

        using ServiceProvider provider = services.BuildServiceProvider();
        provider.GetRequiredService<ICorrelationIdAccessor>().ShouldNotBeNull();
    }

    private sealed class TestOperationContextAccessor : IOperationContextAccessor
    {
        public IOperationContext? Current { get; set; }
    }
}
