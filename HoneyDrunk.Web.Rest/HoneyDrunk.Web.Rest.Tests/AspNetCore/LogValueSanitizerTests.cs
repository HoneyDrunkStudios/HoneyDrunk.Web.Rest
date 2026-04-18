using HoneyDrunk.Web.Rest.AspNetCore.Diagnostics;
using Shouldly;

namespace HoneyDrunk.Web.Rest.Tests.AspNetCore;

/// <summary>
/// Tests for log value sanitization.
/// </summary>
public sealed class LogValueSanitizerTests
{
    /// <summary>
    /// Verifies that carriage returns, line feeds, and tabs are sanitized.
    /// </summary>
    [Fact]
    public void Sanitize_RemovesLineBreaksAndTabs()
    {
        string sanitized = LogValueSanitizer.Sanitize("line1\r\nline2\tline3")!;

        sanitized.ShouldBe("line1line2 line3");
    }

    /// <summary>
    /// Verifies that null values are preserved.
    /// </summary>
    [Fact]
    public void Sanitize_NullValue_ReturnsNull()
    {
        LogValueSanitizer.Sanitize(null).ShouldBeNull();
    }

    /// <summary>
    /// Verifies that empty values are preserved.
    /// </summary>
    [Fact]
    public void Sanitize_EmptyValue_ReturnsEmptyString()
    {
        LogValueSanitizer.Sanitize(string.Empty).ShouldBe(string.Empty);
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
