namespace HoneyDrunk.Web.Rest.AspNetCore.Diagnostics;

/// <summary>
/// Sanitizes user-controlled values before they are written to logs or log scopes.
/// </summary>
internal static class LogValueSanitizer
{
    /// <summary>
    /// Removes carriage returns and line feeds, and replaces tabs with spaces,
    /// to reduce the risk of forged multi-line log entries.
    /// </summary>
    /// <param name="value">The value to sanitize.</param>
    /// <returns>The sanitized value.</returns>
    public static string? Sanitize(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        int index = value.AsSpan().IndexOfAny('\r', '\n', '\t');
        if (index < 0)
        {
            return value;
        }

        return value
            .Replace("\r", string.Empty, StringComparison.Ordinal)
            .Replace("\n", string.Empty, StringComparison.Ordinal)
            .Replace("\t", " ", StringComparison.Ordinal);
    }
}
