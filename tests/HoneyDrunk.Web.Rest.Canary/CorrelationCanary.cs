using Microsoft.Extensions.Logging;
using Shouldly;
using System.Net;
using System.Text.Json;

namespace HoneyDrunk.Web.Rest.Canary;

/// <summary>
/// Invariant 4: Correlation mismatch warning visibility and round-trip behavior.
/// </summary>
public sealed class CorrelationCanary : IDisposable
{
    /// <summary>
    /// Header correlation round-trips into response header when no Kernel context is present.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test.</returns>
    [Fact]
    public async Task HeaderCorrelation_RoundTripsToResponseHeader()
    {
        using CanaryHostFactory factory = new();
        HttpClient client = factory.CreateClient();

        using HttpRequestMessage request = new(HttpMethod.Get, new Uri("/canary/correlation", UriKind.Relative));
        request.Headers.Add("X-Correlation-Id", "header-123");

        HttpResponseMessage response = await client.SendAsync(request);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Verify response header echoes the correlation
        response.Headers.TryGetValues("X-Correlation-Id", out System.Collections.Generic.IEnumerable<string>? values).ShouldBeTrue();
        values!.ShouldContain("header-123");

        // Verify response body has the same correlation
        string body = await response.Content.ReadAsStringAsync();
        using JsonDocument doc = JsonDocument.Parse(body);
        doc.RootElement.GetProperty("correlationId").GetString().ShouldBe("header-123");
    }

    /// <summary>
    /// When Kernel operation context has a different correlation ID than the header,
    /// Kernel wins and a warning is logged about the mismatch.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test.</returns>
    [Fact]
    public async Task KernelCorrelationOverridesHeader_AndWarningIsLogged()
    {
        FakeOperationContextAccessor fakeAccessor = new()
        {
            Current = new StubOperationContext("kernel-456"),
        };

        using CanaryHostFactory factory = new()
        {
            OperationContextAccessor = fakeAccessor,
        };

        HttpClient client = factory.CreateClient();

        using HttpRequestMessage request = new(HttpMethod.Get, new Uri("/canary/correlation", UriKind.Relative));
        request.Headers.Add("X-Correlation-Id", "header-123");

        HttpResponseMessage response = await client.SendAsync(request);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Kernel correlation should win
        response.Headers.TryGetValues("X-Correlation-Id", out System.Collections.Generic.IEnumerable<string>? values).ShouldBeTrue();
        values!.ShouldContain("kernel-456");

        // Response body should have kernel correlation
        string body = await response.Content.ReadAsStringAsync();
        using JsonDocument doc = JsonDocument.Parse(body);
        doc.RootElement.GetProperty("correlationId").GetString().ShouldBe("kernel-456");

        // A warning should have been logged about the mismatch
        factory.LogCapture.Entries.ShouldContain(e =>
            e.LogLevel == LogLevel.Warning
            && e.Message.Contains("header-123")
            && e.Message.Contains("kernel-456")
            && e.Message.Contains("mismatch", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// When Kernel and header correlation are the same, no mismatch warning is logged.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test.</returns>
    [Fact]
    public async Task MatchingCorrelation_NoWarningLogged()
    {
        FakeOperationContextAccessor fakeAccessor = new()
        {
            Current = new StubOperationContext("same-id"),
        };

        using CanaryHostFactory factory = new()
        {
            OperationContextAccessor = fakeAccessor,
        };

        HttpClient client = factory.CreateClient();

        using HttpRequestMessage request = new(HttpMethod.Get, new Uri("/canary/correlation", UriKind.Relative));
        request.Headers.Add("X-Correlation-Id", "same-id");

        HttpResponseMessage response = await client.SendAsync(request);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        // No mismatch warning should exist
        factory.LogCapture.Entries.ShouldNotContain(e =>
            e.LogLevel == LogLevel.Warning
            && e.Message.Contains("mismatch", StringComparison.OrdinalIgnoreCase));
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        // Individual factories are disposed inline.
    }
}
