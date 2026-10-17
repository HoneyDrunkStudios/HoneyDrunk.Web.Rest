using HoneyDrunk.Web.Rest.Abstractions.Errors;
using HoneyDrunk.Web.Rest.AspNetCore.Serialization;
using HoneyDrunk.Web.Rest.Tests.TestHost;
using Shouldly;
using System.Net;
using System.Text.Json;

namespace HoneyDrunk.Web.Rest.Tests.AspNetCore;

/// <summary>
/// Tests for exception-to-HTTP-error mapping.
/// </summary>
public sealed class ExceptionMappingTests : IDisposable
{
    private readonly TestApiFactory _factory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionMappingTests"/> class.
    /// </summary>
    public ExceptionMappingTests()
    {
        _factory = new TestApiFactory();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _factory.Dispose();
    }

    /// <summary>
    /// Verifies that KeyNotFoundException returns 404 Not Found.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task KeyNotFoundException_Returns404NotFound()
    {
        HttpClient client = _factory.CreateClient();
        HttpResponseMessage response = await client.GetAsync(new Uri("/test/not-found", UriKind.Relative));

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        ApiErrorResponse? errorResponse = await DeserializeErrorAsync(response);
        errorResponse.ShouldNotBeNull();
        errorResponse.Error.Code.ShouldBe(ApiErrorCode.NotFound);
    }

    /// <summary>
    /// Verifies that ArgumentException returns 400 Bad Request.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task ArgumentException_Returns400BadRequest()
    {
        HttpClient client = _factory.CreateClient();
        HttpResponseMessage response = await client.GetAsync(new Uri("/test/argument-exception", UriKind.Relative));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        ApiErrorResponse? errorResponse = await DeserializeErrorAsync(response);
        errorResponse.ShouldNotBeNull();
        errorResponse.Error.Code.ShouldBe(ApiErrorCode.BadRequest);
    }

    /// <summary>
    /// Verifies that ArgumentNullException returns 400 Bad Request.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task ArgumentNullException_Returns400BadRequest()
    {
        HttpClient client = _factory.CreateClient();
        HttpResponseMessage response = await client.GetAsync(new Uri("/test/argument-null-exception", UriKind.Relative));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        ApiErrorResponse? errorResponse = await DeserializeErrorAsync(response);
        errorResponse.ShouldNotBeNull();
        errorResponse.Error.Code.ShouldBe(ApiErrorCode.BadRequest);
        errorResponse.Error.Message.ShouldContain("requiredParam");
    }

    /// <summary>
    /// Verifies that InvalidOperationException returns 409 Conflict.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task InvalidOperationException_Returns409Conflict()
    {
        HttpClient client = _factory.CreateClient();
        HttpResponseMessage response = await client.GetAsync(new Uri("/test/invalid-operation", UriKind.Relative));

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);

        ApiErrorResponse? errorResponse = await DeserializeErrorAsync(response);
        errorResponse.ShouldNotBeNull();
        errorResponse.Error.Code.ShouldBe(ApiErrorCode.Conflict);
    }

    /// <summary>
    /// Verifies that UnauthorizedAccessException returns 403 Forbidden.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task UnauthorizedAccessException_Returns403Forbidden()
    {
        HttpClient client = _factory.CreateClient();
        HttpResponseMessage response = await client.GetAsync(new Uri("/test/unauthorized", UriKind.Relative));

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        ApiErrorResponse? errorResponse = await DeserializeErrorAsync(response);
        errorResponse.ShouldNotBeNull();
        errorResponse.Error.Code.ShouldBe(ApiErrorCode.Forbidden);
    }

    /// <summary>
    /// Verifies that NotImplementedException returns 501 Not Implemented.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task NotImplementedException_Returns501NotImplemented()
    {
        HttpClient client = _factory.CreateClient();
        HttpResponseMessage response = await client.GetAsync(new Uri("/test/not-implemented", UriKind.Relative));

        response.StatusCode.ShouldBe(HttpStatusCode.NotImplemented);

        ApiErrorResponse? errorResponse = await DeserializeErrorAsync(response);
        errorResponse.ShouldNotBeNull();
        errorResponse.Error.Code.ShouldBe(ApiErrorCode.NotImplemented);
    }

    /// <summary>
    /// Verifies that NotSupportedException returns 500 Internal Server Error (not mapped).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task NotSupportedException_Returns500InternalServerError()
    {
        HttpClient client = _factory.CreateClient();
        HttpResponseMessage response = await client.GetAsync(new Uri("/test/not-supported", UriKind.Relative));

        response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);

        ApiErrorResponse? errorResponse = await DeserializeErrorAsync(response);
        errorResponse.ShouldNotBeNull();
        errorResponse.Error.Code.ShouldBe(ApiErrorCode.InternalError);
    }

    /// <summary>
    /// Verifies that generic Exception returns 500 Internal Server Error.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GenericException_Returns500InternalServerError()
    {
        HttpClient client = _factory.CreateClient();
        HttpResponseMessage response = await client.GetAsync(new Uri("/test/internal-error", UriKind.Relative));

        response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);

        ApiErrorResponse? errorResponse = await DeserializeErrorAsync(response);
        errorResponse.ShouldNotBeNull();
        errorResponse.Error.Code.ShouldBe(ApiErrorCode.InternalError);
    }

    /// <summary>
    /// Verifies that exception details are not included by default.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task ExceptionDetails_NotIncludedByDefault()
    {
        HttpClient client = _factory.CreateClient();
        HttpResponseMessage response = await client.GetAsync(new Uri("/test/internal-error", UriKind.Relative));

        string content = await response.Content.ReadAsStringAsync();

        content.ShouldNotContain("StackTrace");
        content.ShouldNotContain("at HoneyDrunk");
    }

    /// <summary>
    /// Verifies that error response always has a correlation ID.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task ErrorResponse_AlwaysHasCorrelationId()
    {
        HttpClient client = _factory.CreateClient();
        HttpResponseMessage response = await client.GetAsync(new Uri("/test/internal-error", UriKind.Relative));

        ApiErrorResponse? errorResponse = await DeserializeErrorAsync(response);
        errorResponse.ShouldNotBeNull();
        errorResponse.CorrelationId.ShouldNotBeNullOrWhiteSpace();
    }

    /// <summary>
    /// Verifies that error response has a timestamp.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task ErrorResponse_HasTimestamp()
    {
        HttpClient client = _factory.CreateClient();
        HttpResponseMessage response = await client.GetAsync(new Uri("/test/not-found", UriKind.Relative));

        string content = await response.Content.ReadAsStringAsync();
        content.ShouldContain("\"timestamp\"");
    }

    /// <summary>
    /// Verifies that error response content type is JSON.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task ErrorResponse_ContentTypeIsJson()
    {
        HttpClient client = _factory.CreateClient();
        HttpResponseMessage response = await client.GetAsync(new Uri("/test/not-found", UriKind.Relative));

        response.Content.Headers.ContentType.ShouldNotBeNull();
        response.Content.Headers.ContentType.MediaType.ShouldBe("application/json");
    }

    private static async Task<ApiErrorResponse?> DeserializeErrorAsync(HttpResponseMessage response)
    {
        string content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ApiErrorResponse>(content, JsonOptionsDefaults.SerializerOptions);
    }
}
