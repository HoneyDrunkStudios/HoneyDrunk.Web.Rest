using Microsoft.Extensions.Logging;

namespace HoneyDrunk.Web.Rest.Canary;

/// <summary>
/// A captured log entry for assertion.
/// </summary>
internal sealed class CapturedLogEntry
{
    /// <summary>Gets or sets the logger category.</summary>
    public required string Category { get; set; }

    /// <summary>Gets or sets the log level.</summary>
    public required LogLevel LogLevel { get; set; }

    /// <summary>Gets or sets the formatted message.</summary>
    public required string Message { get; set; }

    /// <summary>Gets or sets the exception, if any.</summary>
    public Exception? Exception { get; set; }
}
