namespace HoneyDrunk.Web.Rest.Abstractions.Constants;

/// <summary>
/// Defines standard HTTP header names used by HoneyDrunk REST services.
/// </summary>
public static class HeaderNames
{
    /// <summary>
    /// The default header name for correlation ID propagation.
    /// </summary>
    public const string CorrelationId = "X-Correlation-Id";

    /// <summary>
    /// The header name for request ID (typically set by the framework).
    /// </summary>
    public const string RequestId = "X-Request-Id";

    /// <summary>
    /// The header name for trace ID from distributed tracing.
    /// </summary>
    public const string TraceId = "X-Trace-Id";

    /// <summary>
    /// The header name for API version specification.
    /// </summary>
    public const string ApiVersion = "X-Api-Version";

    /// <summary>
    /// The header name for rate limit remaining count.
    /// </summary>
    public const string RateLimitRemaining = "X-RateLimit-Remaining";

    /// <summary>
    /// The header name for rate limit reset time.
    /// </summary>
    public const string RateLimitReset = "X-RateLimit-Reset";
}
