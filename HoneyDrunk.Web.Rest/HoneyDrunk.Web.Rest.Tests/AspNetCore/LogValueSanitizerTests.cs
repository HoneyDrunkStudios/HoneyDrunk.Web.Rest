using HoneyDrunk.Web.Rest.AspNetCore.Diagnostics;
using Shouldly;

namespace HoneyDrunk.Web.Rest.Tests.AspNetCore;

/// <summary>
/// Tests for log value sanitization.
/// </summary>
public sealed class LogValueSanitizerTests
{
    /// <summary>
    /// Verifies that line-break characters are removed from user-controlled values.
    /// </summary>
    [Fact]
    public void Sanitize_RemovesControlCharacters()
    {
        string sanitized = LogValueSanitizer.Sanitize("line1\r\nline2\tline3")!;

        sanitized.ShouldBe("line1line2 line3");
    }

    /// <summary>
    /// Verifies that clean values are preserved.
    /// </summary>
    [Fact]
    public void Sanitize_LeavesSafeValueUnchanged()
    {
        const string value = "safe-correlation-id";

        LogValueSanitizer.Sanitize(value).ShouldBe(value);
    }
}
