namespace HoneyDrunk.Web.Rest.Abstractions.Errors;

/// <summary>
/// Standard error codes used in API error responses.
/// These codes provide machine-readable identifiers for error conditions.
/// </summary>
public static class ApiErrorCode
{
    /// <summary>
    /// A general error occurred.
    /// </summary>
    public const string GeneralError = "GENERAL_ERROR";

    /// <summary>
    /// The request validation failed.
    /// </summary>
    public const string ValidationFailed = "VALIDATION_FAILED";

    /// <summary>
    /// The requested resource was not found.
    /// </summary>
    public const string NotFound = "NOT_FOUND";

    /// <summary>
    /// The request is unauthorized.
    /// </summary>
    public const string Unauthorized = "UNAUTHORIZED";

    /// <summary>
    /// The request is forbidden.
    /// </summary>
    public const string Forbidden = "FORBIDDEN";

    /// <summary>
    /// A conflict occurred with the current state.
    /// </summary>
    public const string Conflict = "CONFLICT";

    /// <summary>
    /// The request was malformed or contains invalid data.
    /// </summary>
    public const string BadRequest = "BAD_REQUEST";

    /// <summary>
    /// An internal server error occurred.
    /// </summary>
    public const string InternalError = "INTERNAL_ERROR";

    /// <summary>
    /// The requested operation is not implemented.
    /// </summary>
    public const string NotImplemented = "NOT_IMPLEMENTED";

    /// <summary>
    /// The service is temporarily unavailable.
    /// </summary>
    public const string ServiceUnavailable = "SERVICE_UNAVAILABLE";
}
