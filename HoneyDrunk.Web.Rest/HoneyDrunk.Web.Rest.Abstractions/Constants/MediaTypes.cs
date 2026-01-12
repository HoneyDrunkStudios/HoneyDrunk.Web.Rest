namespace HoneyDrunk.Web.Rest.Abstractions.Constants;

/// <summary>
/// Defines standard media types used by HoneyDrunk REST services.
/// </summary>
public static class MediaTypes
{
    /// <summary>
    /// The standard JSON media type.
    /// </summary>
    public const string Json = "application/json";

    /// <summary>
    /// The JSON media type with UTF-8 charset.
    /// </summary>
    public const string JsonUtf8 = "application/json; charset=utf-8";

    /// <summary>
    /// The JSON problem details media type (RFC 7807).
    /// </summary>
    public const string ProblemJson = "application/problem+json";

    /// <summary>
    /// The plain text media type.
    /// </summary>
    public const string PlainText = "text/plain";

    /// <summary>
    /// The octet-stream media type for binary content.
    /// </summary>
    public const string OctetStream = "application/octet-stream";
}
