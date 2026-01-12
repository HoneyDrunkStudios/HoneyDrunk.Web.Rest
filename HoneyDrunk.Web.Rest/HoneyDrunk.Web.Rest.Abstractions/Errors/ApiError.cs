namespace HoneyDrunk.Web.Rest.Abstractions.Errors;

/// <summary>
/// Represents an error in an API response.
/// </summary>
public sealed record ApiError
{
    /// <summary>
    /// Gets the machine-readable error code.
    /// </summary>
    public required string Code { get; init; }

    /// <summary>
    /// Gets the human-readable error message.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Gets optional additional details about the error.
    /// </summary>
    public string? Details { get; init; }

    /// <summary>
    /// Gets the optional target of the error (e.g., the field or parameter that caused the error).
    /// </summary>
    public string? Target { get; init; }
}
