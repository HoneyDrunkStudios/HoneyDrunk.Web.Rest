using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace HoneyDrunk.Web.Rest.Canary;

#pragma warning disable CA1812 // Instantiated via AddScheme<> reflection

/// <summary>
/// A test authentication handler that authenticates based on a custom header.
/// Send <c>X-Canary-Auth: authenticated</c> to be authenticated with no roles.
/// Send <c>X-Canary-Auth: admin</c> to be authenticated with the Admin role.
/// Omit the header to be unauthenticated.
/// </summary>
internal sealed class CanaryAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    /// <inheritdoc/>
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("X-Canary-Auth", out Microsoft.Extensions.Primitives.StringValues authHeader)
            || string.IsNullOrWhiteSpace(authHeader.ToString()))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        string authValue = authHeader.ToString();

        List<Claim> claims =
        [
            new Claim(ClaimTypes.Name, "canary-user"),
        ];

        if (string.Equals(authValue, "admin", StringComparison.OrdinalIgnoreCase))
        {
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));
        }

        ClaimsIdentity identity = new(claims, "CanaryScheme");
        ClaimsPrincipal principal = new(identity);
        AuthenticationTicket ticket = new(principal, "CanaryScheme");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
