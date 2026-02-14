using HoneyDrunk.Web.Rest.Abstractions.Errors;
using HoneyDrunk.Web.Rest.AspNetCore.Serialization;
using Shouldly;
using System.Net;
using System.Text.Json;

namespace HoneyDrunk.Web.Rest.Canary;

/// <summary>
/// Invariant 3: Auth shaping — 401 vs 403 message correctness,
/// including fallback when IAuthenticatedIdentityAccessor is not registered.
/// </summary>
public sealed class AuthShapingCanary : IDisposable
{
    private readonly CanaryHostFactory _factory;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthShapingCanary"/> class.
    /// </summary>
    public AuthShapingCanary()
    {
        _factory = new CanaryHostFactory
        {
            // Auth is configured WITHOUT IAuthenticatedIdentityAccessor,
            // forcing RestAuthorizationResultHandler to fall back to HttpContext.User.Identity.IsAuthenticated
            UseAuthWithoutIdentityAccessor = true,
        };
    }

    /// <inheritdoc/>
    public void Dispose() => _factory.Dispose();

    /// <summary>
    /// No auth token → 401 with "Authentication is required." message.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test.</returns>
    [Fact]
    public async Task NoToken_Returns401_WithAuthRequiredMessage()
    {
        HttpClient client = _factory.CreateClient();
        HttpResponseMessage response = await client.GetAsync(new Uri("/canary/auth/protected", UriKind.Relative));

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        string body = await response.Content.ReadAsStringAsync();
        ApiErrorResponse? error = JsonSerializer.Deserialize<ApiErrorResponse>(body, JsonOptionsDefaults.SerializerOptions);

        error.ShouldNotBeNull();
        error.Error.ShouldNotBeNull();
        error.Error.Code.ShouldBe(ApiErrorCode.Unauthorized);
        error.Error.Message.ShouldContain("Authentication is required");
    }

    /// <summary>
    /// Authenticated user lacking required role → 403 with permission-denied message.
    /// IAuthenticatedIdentityAccessor is NOT registered — handler falls back to HttpContext.User.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test.</returns>
    [Fact]
    public async Task AuthenticatedButUnauthorized_Returns403_WithPermissionMessage()
    {
        HttpClient client = _factory.CreateClient();

        // Send auth header that authenticates the user with no roles (not Admin)
        using HttpRequestMessage request = new(HttpMethod.Get, new Uri("/canary/auth/protected", UriKind.Relative));
        request.Headers.Add("X-Canary-Auth", "authenticated");

        HttpResponseMessage response = await client.SendAsync(request);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        string body = await response.Content.ReadAsStringAsync();
        ApiErrorResponse? error = JsonSerializer.Deserialize<ApiErrorResponse>(body, JsonOptionsDefaults.SerializerOptions);

        error.ShouldNotBeNull();
        error.Error.ShouldNotBeNull();
        error.Error.Code.ShouldBe(ApiErrorCode.Forbidden);

        // Critical: with HttpContext.User fallback, authenticated user must get permission message, not auth-required
        error.Error.Message.ShouldContain("permission");
        error.Error.Message.ShouldNotContain("Authentication is required");
    }

    /// <summary>
    /// Authenticated user WITH required role → 200 (control case).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test.</returns>
    [Fact]
    public async Task AuthenticatedWithRole_Returns200()
    {
        HttpClient client = _factory.CreateClient();

        using HttpRequestMessage request = new(HttpMethod.Get, new Uri("/canary/auth/protected", UriKind.Relative));
        request.Headers.Add("X-Canary-Auth", "admin");

        HttpResponseMessage response = await client.SendAsync(request);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    /// <summary>
    /// Auth-shaped responses include correlation ID in the response body.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test.</returns>
    [Fact]
    public async Task AuthError_IncludesCorrelationId()
    {
        HttpClient client = _factory.CreateClient();
        HttpResponseMessage response = await client.GetAsync(new Uri("/canary/auth/protected", UriKind.Relative));

        string body = await response.Content.ReadAsStringAsync();
        ApiErrorResponse? error = JsonSerializer.Deserialize<ApiErrorResponse>(body, JsonOptionsDefaults.SerializerOptions);

        error.ShouldNotBeNull();

        // Correlation ID may be empty if correlation middleware ran before auth, or generated
        // The key invariant is that the field is present and the envelope is shaped correctly
        error.CorrelationId.ShouldNotBeNull();
    }
}
