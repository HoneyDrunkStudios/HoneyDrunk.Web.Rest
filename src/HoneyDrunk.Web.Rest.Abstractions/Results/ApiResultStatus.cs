namespace HoneyDrunk.Web.Rest.Abstractions.Results;

/// <summary>
/// Represents the status of an API result.
/// </summary>
public enum ApiResultStatus
{
    /// <summary>
    /// The operation completed successfully.
    /// </summary>
    Success = 0,

    /// <summary>
    /// The operation failed due to a general error.
    /// </summary>
    Failed = 1,

    /// <summary>
    /// The request is unauthorized (authentication required).
    /// </summary>
    Unauthorized = 2,

    /// <summary>
    /// The request is forbidden (insufficient permissions).
    /// </summary>
    Forbidden = 3,

    /// <summary>
    /// The requested resource was not found.
    /// </summary>
    NotFound = 4,

    /// <summary>
    /// A conflict occurred with the current state.
    /// </summary>
    Conflict = 5,

    /// <summary>
    /// The request validation failed.
    /// </summary>
    ValidationFailed = 6,

    /// <summary>
    /// An internal error occurred.
    /// </summary>
    Error = 7,
}
