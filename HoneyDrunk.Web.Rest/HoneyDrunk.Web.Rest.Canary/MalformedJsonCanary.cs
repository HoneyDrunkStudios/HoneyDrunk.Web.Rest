using HoneyDrunk.Web.Rest.Abstractions.Errors;
using HoneyDrunk.Web.Rest.AspNetCore.Serialization;
using Shouldly;
using System.Net;
using System.Text;
using System.Text.Json;

namespace HoneyDrunk.Web.Rest.Canary;

/// <summary>
/// Invariant 1: Malformed JSON is 400 with ApiErrorResponse, not 500.
/// </summary>
public sealed class MalformedJsonCanary : IDisposable
{
    private readonly CanaryHostFactory _factory = new();

    /// <summary>
    /// Malformed JSON body returns 400 Bad Request with standard ApiErrorResponse envelope.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test.</returns>
    [Fact]
    public async Task MalformedJson_Returns400_WithApiErrorResponse()
    {
        HttpClient client = _factory.CreateClient();

        using StringContent content = new("{ this is not valid json }", Encoding.UTF8, "application/json");
        HttpResponseMessage response = await client.PostAsync(new Uri("/canary/json-body", UriKind.Relative), content);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        string body = await response.Content.ReadAsStringAsync();
        ApiErrorResponse? error = JsonSerializer.Deserialize<ApiErrorResponse>(body, JsonOptionsDefaults.SerializerOptions);

        error.ShouldNotBeNull();
        error.Error.ShouldNotBeNull();
        error.Error.Code.ShouldBe(ApiErrorCode.BadRequest);
    }

    /// <summary>
    /// Valid JSON body returns 200 OK (control case).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test.</returns>
    [Fact]
    public async Task ValidJson_Returns200()
    {
        HttpClient client = _factory.CreateClient();

        using StringContent content = new("{}", Encoding.UTF8, "application/json");
        HttpResponseMessage response = await client.PostAsync(new Uri("/canary/json-body", UriKind.Relative), content);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    /// <inheritdoc/>
    public void Dispose() => _factory.Dispose();
}
