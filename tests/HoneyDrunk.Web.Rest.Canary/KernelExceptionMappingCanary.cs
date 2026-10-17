using HoneyDrunk.Web.Rest.Abstractions.Errors;
using HoneyDrunk.Web.Rest.AspNetCore.Serialization;
using Shouldly;
using System.Net;
using System.Text.Json;

namespace HoneyDrunk.Web.Rest.Canary;

/// <summary>
/// Invariant 2: Kernel typed exceptions map to correct HTTP status codes.
/// </summary>
public sealed class KernelExceptionMappingCanary : IDisposable
{
    private readonly CanaryHostFactory _factory = new();

    /// <inheritdoc/>
    public void Dispose() => _factory.Dispose();

    /// <summary>
    /// Kernel NotFoundException maps to 404 Not Found.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test.</returns>
    [Fact]
    public async Task KernelNotFoundException_Returns404()
    {
        HttpClient client = _factory.CreateClient();
        HttpResponseMessage response = await client.GetAsync(new Uri("/canary/kernel/not-found", UriKind.Relative));

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        ApiErrorResponse? error = await DeserializeError(response);
        error.ShouldNotBeNull();
        error.Error!.Code.ShouldBe(ApiErrorCode.NotFound);
    }

    /// <summary>
    /// Kernel ValidationException maps to 400 Bad Request.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test.</returns>
    [Fact]
    public async Task KernelValidationException_Returns400()
    {
        HttpClient client = _factory.CreateClient();
        HttpResponseMessage response = await client.GetAsync(new Uri("/canary/kernel/validation", UriKind.Relative));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        ApiErrorResponse? error = await DeserializeError(response);
        error.ShouldNotBeNull();
        error.Error!.Code.ShouldBe(ApiErrorCode.BadRequest);
    }

    /// <summary>
    /// Kernel ConcurrencyException maps to 409 Conflict.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test.</returns>
    [Fact]
    public async Task KernelConcurrencyException_Returns409()
    {
        HttpClient client = _factory.CreateClient();
        HttpResponseMessage response = await client.GetAsync(new Uri("/canary/kernel/concurrency", UriKind.Relative));

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        ApiErrorResponse? error = await DeserializeError(response);
        error.ShouldNotBeNull();
        error.Error!.Code.ShouldBe(ApiErrorCode.Conflict);
    }

    /// <summary>
    /// Kernel DependencyFailureException maps to 503 Service Unavailable.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test.</returns>
    [Fact]
    public async Task KernelDependencyFailureException_Returns503()
    {
        HttpClient client = _factory.CreateClient();
        HttpResponseMessage response = await client.GetAsync(new Uri("/canary/kernel/dependency-failure", UriKind.Relative));

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
        ApiErrorResponse? error = await DeserializeError(response);
        error.ShouldNotBeNull();
        error.Error!.Code.ShouldBe(ApiErrorCode.ServiceUnavailable);
    }

    /// <summary>
    /// Kernel SecurityException maps to 403 Forbidden.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test.</returns>
    [Fact]
    public async Task KernelSecurityException_Returns403()
    {
        HttpClient client = _factory.CreateClient();
        HttpResponseMessage response = await client.GetAsync(new Uri("/canary/kernel/security", UriKind.Relative));

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        ApiErrorResponse? error = await DeserializeError(response);
        error.ShouldNotBeNull();
        error.Error!.Code.ShouldBe(ApiErrorCode.Forbidden);
    }

    /// <summary>
    /// Every Kernel exception response includes a non-null error message (no leaky internals).
    /// </summary>
    /// <param name="path">The endpoint path that triggers the Kernel exception.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous test.</returns>
    [Theory]
    [InlineData("/canary/kernel/not-found")]
    [InlineData("/canary/kernel/validation")]
    [InlineData("/canary/kernel/concurrency")]
    [InlineData("/canary/kernel/dependency-failure")]
    [InlineData("/canary/kernel/security")]
    public async Task KernelExceptions_HaveNonEmptyMessage(string path)
    {
        HttpClient client = _factory.CreateClient();
        HttpResponseMessage response = await client.GetAsync(new Uri(path, UriKind.Relative));

        ApiErrorResponse? error = await DeserializeError(response);
        error.ShouldNotBeNull();
        error.Error!.Message.ShouldNotBeNullOrWhiteSpace();
    }

    /// <summary>
    /// No Kernel exception leaks internal exception details in the response.
    /// </summary>
    /// <param name="path">The endpoint path that triggers the Kernel exception.</param>
    /// <param name="internalFragment">A fragment from the internal exception message that must not appear.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous test.</returns>
    [Theory]
    [InlineData("/canary/kernel/not-found", "Canary:")]
    [InlineData("/canary/kernel/validation", "Canary:")]
    [InlineData("/canary/kernel/concurrency", "Canary:")]
    [InlineData("/canary/kernel/dependency-failure", "Canary:")]
    [InlineData("/canary/kernel/security", "Canary:")]
    public async Task KernelExceptions_DoNotLeakInternalMessage(string path, string internalFragment)
    {
        HttpClient client = _factory.CreateClient();
        HttpResponseMessage response = await client.GetAsync(new Uri(path, UriKind.Relative));

        ApiErrorResponse? error = await DeserializeError(response);
        error.ShouldNotBeNull();

        // The safe static message should NOT contain the internal exception message fragment
        error.Error!.Message.ShouldNotContain(internalFragment);
    }

    private static async Task<ApiErrorResponse?> DeserializeError(HttpResponseMessage response)
    {
        string body = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ApiErrorResponse>(body, JsonOptionsDefaults.SerializerOptions);
    }
}
