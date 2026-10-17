namespace HoneyDrunk.Web.Rest.AspNetCore.Configuration;

/// <summary>
/// Configuration options for REST middleware and filters.
/// </summary>
public sealed class RestOptions
{
    /// <summary>
    /// Gets or sets the header name used for correlation ID.
    /// Default is "X-Correlation-Id".
    /// </summary>
    public string CorrelationIdHeaderName { get; set; } = "X-Correlation-Id";

    /// <summary>
    /// Gets or sets a value indicating whether to include exception details in error responses.
    /// This should only be enabled in development environments.
    /// Default is false.
    /// </summary>
    public bool IncludeExceptionDetails { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to include trace ID in responses.
    /// Default is true.
    /// </summary>
    public bool IncludeTraceId { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to return correlation ID in response headers.
    /// Default is true.
    /// </summary>
    public bool ReturnCorrelationIdInResponseHeader { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to generate a correlation ID if one is not provided.
    /// Default is true.
    /// </summary>
    public bool GenerateCorrelationIdIfMissing { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to enable the request logging scope middleware.
    /// Default is true.
    /// </summary>
    public bool EnableRequestLoggingScope { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to enable the exception mapping middleware.
    /// Default is true.
    /// </summary>
    public bool EnableExceptionMapping { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to enable the model state validation filter for MVC.
    /// Default is true.
    /// </summary>
    public bool EnableModelStateValidationFilter { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to configure JSON serialization defaults.
    /// Default is true.
    /// </summary>
    public bool ConfigureJsonDefaults { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to enable auth failure response shaping.
    /// When enabled, 401 and 403 responses are shaped as ApiErrorResponse.
    /// Default is true.
    /// </summary>
    public bool EnableAuthFailureShaping { get; set; } = true;
}
