namespace HoneyDrunk.Web.Rest.Abstractions.Telemetry;

/// <summary>
/// Defines standard telemetry tag names used by HoneyDrunk REST services.
/// These constants are used for consistent tagging in logs and traces.
/// </summary>
public static class RestTelemetryTags
{
    /// <summary>
    /// The tag name for correlation ID.
    /// </summary>
    public const string CorrelationId = "correlation.id";

    /// <summary>
    /// The tag name for request ID.
    /// </summary>
    public const string RequestId = "request.id";

    /// <summary>
    /// The tag name for HTTP method.
    /// </summary>
    public const string HttpMethod = "http.method";

    /// <summary>
    /// The tag name for HTTP route.
    /// </summary>
    public const string HttpRoute = "http.route";

    /// <summary>
    /// The tag name for HTTP status code.
    /// </summary>
    public const string HttpStatusCode = "http.status_code";

    /// <summary>
    /// The tag name for HTTP request path.
    /// </summary>
    public const string HttpPath = "http.path";

    /// <summary>
    /// The tag name for error type.
    /// </summary>
    public const string ErrorType = "error.type";

    /// <summary>
    /// The tag name for error code.
    /// </summary>
    public const string ErrorCode = "error.code";

    /// <summary>
    /// The tag name for user ID.
    /// </summary>
    public const string UserId = "user.id";

    /// <summary>
    /// The tag name for tenant ID.
    /// </summary>
    public const string TenantId = "tenant.id";

    /// <summary>
    /// The tag name for service name.
    /// </summary>
    public const string ServiceName = "service.name";

    /// <summary>
    /// The tag name for service version.
    /// </summary>
    public const string ServiceVersion = "service.version";

    /// <summary>
    /// The tag name for request duration in milliseconds.
    /// </summary>
    public const string RequestDurationMs = "request.duration_ms";
}
