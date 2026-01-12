namespace HoneyDrunk.Web.Rest.Abstractions.Errors;

/// <summary>
/// Represents a validation error for a specific field.
/// </summary>
public sealed record ValidationError
{
    /// <summary>
    /// Gets the name of the field that failed validation.
    /// </summary>
    public required string Field { get; init; }

    /// <summary>
    /// Gets the validation error message.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Gets the optional error code for this validation error.
    /// </summary>
    public string? Code { get; init; }
}
