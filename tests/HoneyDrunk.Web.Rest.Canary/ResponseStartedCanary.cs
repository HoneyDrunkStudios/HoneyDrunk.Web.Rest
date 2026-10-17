using Microsoft.Extensions.Logging;
using Shouldly;
using System.Net;

namespace HoneyDrunk.Web.Rest.Canary;

/// <summary>
/// Invariant 5: Response.HasStarted safety — ExceptionMappingMiddleware must not
/// throw a secondary exception when attempting to write after the response has started.
/// </summary>
public sealed class ResponseStartedCanary : IDisposable
{
    private readonly CanaryHostFactory _factory = new();

    /// <summary>
    /// An endpoint that throws after writing partial response content does not crash the server.
    /// The original exception is logged, but no secondary exception occurs from the middleware.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test.</returns>
    [Fact]
    public async Task ExceptionAfterResponseStarted_DoesNotCrash()
    {
        HttpClient client = _factory.CreateClient();

        // This should not throw — the server should handle it gracefully
        HttpResponseMessage response = await client.GetAsync(new Uri("/canary/response-started", UriKind.Relative));

        // The response started with 200 before the exception was thrown,
        // so the status code should be 200 (cannot be changed after HasStarted)
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        // The body should contain the partial content that was written before the throw
        string body = await response.Content.ReadAsStringAsync();
        body.ShouldContain("partial response content");
    }

    /// <summary>
    /// The original exception is still logged even though the response cannot be shaped.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test.</returns>
    [Fact]
    public async Task ExceptionAfterResponseStarted_ExceptionIsStillLogged()
    {
        HttpClient client = _factory.CreateClient();

        await client.GetAsync(new Uri("/canary/response-started", UriKind.Relative));

        // The exception should have been logged at Error level
        _factory.LogCapture.Entries.ShouldContain(e =>
            e.LogLevel == LogLevel.Error
            && e.Message.Contains("unhandled exception", StringComparison.OrdinalIgnoreCase));
    }

    /// <inheritdoc/>
    public void Dispose() => _factory.Dispose();
}
