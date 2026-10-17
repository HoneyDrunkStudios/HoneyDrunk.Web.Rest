using HoneyDrunk.Web.Rest.Abstractions.Errors;
using HoneyDrunk.Web.Rest.AspNetCore.Serialization;
using HoneyDrunk.Web.Rest.Tests.TestHost;
using Shouldly;
using System.Net;
using System.Text.Json;

namespace HoneyDrunk.Web.Rest.Tests.AspNetCore;

/// <summary>
/// Tests for the complete REST pipeline behavior.
/// </summary>
public sealed class WebRestPipelineTests : IDisposable
{
    private readonly TestApiFactory _factory;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebRestPipelineTests"/> class.
    /// </summary>
    public WebRestPipelineTests()
    {
        _factory = new TestApiFactory();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _factory.Dispose();
    }

    /// <summary>
    /// Verifies that success endpoint returns OK with ApiResult.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task SuccessEndpoint_ReturnsOkWithApiResult()
    {
        HttpClient client = _factory.CreateClient();
        HttpResponseMessage response = await client.GetAsync(new Uri("/test/success", UriKind.Relative));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        string content = await response.Content.ReadAsStringAsync();
        content.ShouldContain("\"status\":\"success\"");
    }

    /// <summary>
    /// Verifies that success endpoint with data returns data in ApiResult.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task SuccessWithDataEndpoint_ReturnsDataInApiResult()
    {
        HttpClient client = _factory.CreateClient();
        HttpResponseMessage response = await client.GetAsync(new Uri("/test/success-with-data", UriKind.Relative));

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        string content = await response.Content.ReadAsStringAsync();
        content.ShouldContain("\"data\"");
        content.ShouldContain("\"id\":1");
        content.ShouldContain("\"name\":\"Test\"");
    }

    /// <summary>
    /// Verifies that response contains correlation ID header.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task ResponseContainsCorrelationIdHeader()
    {
        HttpClient client = _factory.CreateClient();
        HttpResponseMessage response = await client.GetAsync(new Uri("/test/success", UriKind.Relative));

        response.Headers.TryGetValues("X-Correlation-Id", out IEnumerable<string>? values).ShouldBeTrue();
        values.ShouldNotBeNull();
        values.ShouldHaveSingleItem();
        values.First().ShouldNotBeNullOrWhiteSpace();
    }

    /// <summary>
    /// Verifies that response content type is application/json.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task ResponseContentType_IsApplicationJson()
    {
        HttpClient client = _factory.CreateClient();
        HttpResponseMessage response = await client.GetAsync(new Uri("/test/success", UriKind.Relative));

        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/json");
    }

    /// <summary>
    /// Verifies that error response has consistent shape.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task ErrorResponse_HasConsistentShape()
    {
        HttpClient client = _factory.CreateClient();
        HttpResponseMessage response = await client.GetAsync(new Uri("/test/not-found", UriKind.Relative));

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        string content = await response.Content.ReadAsStringAsync();
        ApiErrorResponse? errorResponse = JsonSerializer.Deserialize<ApiErrorResponse>(content, JsonOptionsDefaults.SerializerOptions);

        errorResponse.ShouldNotBeNull();
        errorResponse.CorrelationId.ShouldNotBeNullOrWhiteSpace();
        errorResponse.Error.ShouldNotBeNull();
        errorResponse.Error.Code.ShouldBe(ApiErrorCode.NotFound);
    }
}
