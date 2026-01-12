using HoneyDrunk.Web.Rest.Abstractions.Errors;
using HoneyDrunk.Web.Rest.AspNetCore.Serialization;
using HoneyDrunk.Web.Rest.Tests.TestHost;
using Shouldly;
using System.Net;
using System.Text.Json;

namespace HoneyDrunk.Web.Rest.Tests.AspNetCore;

/// <summary>
/// Tests for authentication/authorization failure response shapes.
/// </summary>
public sealed class AuthFailureShapeTests : IDisposable
{
    private readonly TestApiFactory _factory;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthFailureShapeTests"/> class.
    /// </summary>
    public AuthFailureShapeTests()
    {
        _factory = new TestApiFactory();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _factory.Dispose();
    }

    /// <summary>
    /// Verifies that UnauthorizedAccessException returns Forbidden as ApiErrorResponse.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task UnauthorizedAccessException_ReturnsForbiddenAsApiErrorResponse()
    {
        HttpClient client = _factory.CreateClient();
        HttpResponseMessage response = await client.GetAsync(new Uri("/test/unauthorized", UriKind.Relative));

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/json");

        ApiErrorResponse? errorResponse = await DeserializeErrorAsync(response);
        errorResponse.ShouldNotBeNull();
        errorResponse.Error.Code.ShouldBe(ApiErrorCode.Forbidden);
        errorResponse.CorrelationId.ShouldNotBeNullOrWhiteSpace();
    }

    /// <summary>
    /// Verifies that Forbidden response has the standard error shape.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task ForbiddenResponse_HasStandardErrorShape()
    {
        HttpClient client = _factory.CreateClient();
        HttpResponseMessage response = await client.GetAsync(new Uri("/test/unauthorized", UriKind.Relative));

        string content = await response.Content.ReadAsStringAsync();

        content.ShouldContain("\"correlationId\"");
        content.ShouldContain("\"error\"");
        content.ShouldContain("\"code\"");
        content.ShouldContain("\"message\"");
    }

    /// <summary>
    /// Verifies that Forbidden response has correlation ID in header.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task ForbiddenResponse_HasCorrelationIdInHeader()
    {
        HttpClient client = _factory.CreateClient();
        HttpResponseMessage response = await client.GetAsync(new Uri("/test/unauthorized", UriKind.Relative));

        response.Headers.TryGetValues("X-Correlation-Id", out IEnumerable<string>? values).ShouldBeTrue();
        values.ShouldNotBeNull();
        values.First().ShouldNotBeNullOrWhiteSpace();
    }

    private static async Task<ApiErrorResponse?> DeserializeErrorAsync(HttpResponseMessage response)
    {
        string content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ApiErrorResponse>(content, JsonOptionsDefaults.SerializerOptions);
    }
}
