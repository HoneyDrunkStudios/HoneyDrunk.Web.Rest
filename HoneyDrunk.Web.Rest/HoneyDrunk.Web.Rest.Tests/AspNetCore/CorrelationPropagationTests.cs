using HoneyDrunk.Web.Rest.Abstractions.Constants;
using HoneyDrunk.Web.Rest.Abstractions.Errors;
using HoneyDrunk.Web.Rest.AspNetCore.Serialization;
using HoneyDrunk.Web.Rest.Tests.TestHost;
using Shouldly;
using System.Net;
using System.Text.Json;

namespace HoneyDrunk.Web.Rest.Tests.AspNetCore;

/// <summary>
/// Tests for correlation ID propagation.
/// </summary>
public sealed class CorrelationPropagationTests : IDisposable
{
    private readonly TestApiFactory _factory;

    /// <summary>
    /// Initializes a new instance of the <see cref="CorrelationPropagationTests"/> class.
    /// </summary>
    public CorrelationPropagationTests()
    {
        _factory = new TestApiFactory();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _factory.Dispose();
    }

    /// <summary>
    /// Verifies that when correlation ID is provided, the same ID is returned.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task WhenCorrelationIdProvided_ThenSameIdReturned()
    {
        HttpClient client = _factory.CreateClient();
        string correlationId = "test-correlation-12345";

        using HttpRequestMessage request = new(HttpMethod.Get, new Uri("/test/success", UriKind.Relative));
        request.Headers.Add(HeaderNames.CorrelationId, correlationId);

        HttpResponseMessage response = await client.SendAsync(request);

        response.Headers.TryGetValues(HeaderNames.CorrelationId, out IEnumerable<string>? values).ShouldBeTrue();
        values.ShouldNotBeNull();
        values.First().ShouldBe(correlationId);
    }

    /// <summary>
    /// Verifies that when no correlation ID is provided, a new one is generated.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task WhenNoCorrelationIdProvided_ThenNewIdGenerated()
    {
        HttpClient client = _factory.CreateClient();
        HttpResponseMessage response = await client.GetAsync(new Uri("/test/success", UriKind.Relative));

        response.Headers.TryGetValues(HeaderNames.CorrelationId, out IEnumerable<string>? values).ShouldBeTrue();
        values.ShouldNotBeNull();
        values.First().ShouldNotBeNullOrWhiteSpace();
    }

    /// <summary>
    /// Verifies that CorrelationIdAccessor returns the current correlation ID.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task CorrelationIdAccessor_ReturnsCurrentCorrelationId()
    {
        HttpClient client = _factory.CreateClient();
        string correlationId = "accessor-test-id";

        using HttpRequestMessage request = new(HttpMethod.Get, new Uri("/test/correlation", UriKind.Relative));
        request.Headers.Add(HeaderNames.CorrelationId, correlationId);

        HttpResponseMessage response = await client.SendAsync(request);
        string content = await response.Content.ReadAsStringAsync();

        content.ShouldContain(correlationId);
    }

    /// <summary>
    /// Verifies that error response contains the correlation ID.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task ErrorResponse_ContainsCorrelationId()
    {
        HttpClient client = _factory.CreateClient();
        string correlationId = "error-correlation-id";

        using HttpRequestMessage request = new(HttpMethod.Get, new Uri("/test/not-found", UriKind.Relative));
        request.Headers.Add(HeaderNames.CorrelationId, correlationId);

        HttpResponseMessage response = await client.SendAsync(request);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        string content = await response.Content.ReadAsStringAsync();
        ApiErrorResponse? errorResponse = JsonSerializer.Deserialize<ApiErrorResponse>(content, JsonOptionsDefaults.SerializerOptions);

        errorResponse.ShouldNotBeNull();
        errorResponse.CorrelationId.ShouldBe(correlationId);
    }

    /// <summary>
    /// Verifies that correlation ID in header matches the one in body.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task CorrelationIdInHeader_MatchesCorrelationIdInBody()
    {
        HttpClient client = _factory.CreateClient();
        HttpResponseMessage response = await client.GetAsync(new Uri("/test/not-found", UriKind.Relative));

        response.Headers.TryGetValues(HeaderNames.CorrelationId, out IEnumerable<string>? headerValues).ShouldBeTrue();
        string headerCorrelationId = headerValues!.First();

        string content = await response.Content.ReadAsStringAsync();
        ApiErrorResponse? errorResponse = JsonSerializer.Deserialize<ApiErrorResponse>(content, JsonOptionsDefaults.SerializerOptions);

        errorResponse.ShouldNotBeNull();
        errorResponse.CorrelationId.ShouldBe(headerCorrelationId);
    }
}
