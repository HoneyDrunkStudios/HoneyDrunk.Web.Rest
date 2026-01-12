using HoneyDrunk.Web.Rest.Abstractions.Constants;
using Shouldly;

namespace HoneyDrunk.Web.Rest.Tests.Abstractions;

/// <summary>
/// Tests for <see cref="HeaderNames"/> and <see cref="MediaTypes"/> constants.
/// </summary>
public sealed class ConstantsTests
{
    /// <summary>
    /// Verifies that HeaderNames.CorrelationId has correct value.
    /// </summary>
    [Fact]
    public void HeaderNames_CorrelationId_HasCorrectValue()
    {
        HeaderNames.CorrelationId.ShouldBe("X-Correlation-Id");
    }

    /// <summary>
    /// Verifies that HeaderNames.RequestId has correct value.
    /// </summary>
    [Fact]
    public void HeaderNames_RequestId_HasCorrectValue()
    {
        HeaderNames.RequestId.ShouldBe("X-Request-Id");
    }

    /// <summary>
    /// Verifies that HeaderNames.TraceId has correct value.
    /// </summary>
    [Fact]
    public void HeaderNames_TraceId_HasCorrectValue()
    {
        HeaderNames.TraceId.ShouldBe("X-Trace-Id");
    }

    /// <summary>
    /// Verifies that HeaderNames.ApiVersion has correct value.
    /// </summary>
    [Fact]
    public void HeaderNames_ApiVersion_HasCorrectValue()
    {
        HeaderNames.ApiVersion.ShouldBe("X-Api-Version");
    }

    /// <summary>
    /// Verifies that HeaderNames.RateLimitRemaining has correct value.
    /// </summary>
    [Fact]
    public void HeaderNames_RateLimitRemaining_HasCorrectValue()
    {
        HeaderNames.RateLimitRemaining.ShouldBe("X-RateLimit-Remaining");
    }

    /// <summary>
    /// Verifies that HeaderNames.RateLimitReset has correct value.
    /// </summary>
    [Fact]
    public void HeaderNames_RateLimitReset_HasCorrectValue()
    {
        HeaderNames.RateLimitReset.ShouldBe("X-RateLimit-Reset");
    }

    /// <summary>
    /// Verifies that MediaTypes.Json has correct value.
    /// </summary>
    [Fact]
    public void MediaTypes_Json_HasCorrectValue()
    {
        MediaTypes.Json.ShouldBe("application/json");
    }

    /// <summary>
    /// Verifies that MediaTypes.JsonUtf8 has correct value.
    /// </summary>
    [Fact]
    public void MediaTypes_JsonUtf8_HasCorrectValue()
    {
        MediaTypes.JsonUtf8.ShouldBe("application/json; charset=utf-8");
    }

    /// <summary>
    /// Verifies that MediaTypes.ProblemJson has correct value.
    /// </summary>
    [Fact]
    public void MediaTypes_ProblemJson_HasCorrectValue()
    {
        MediaTypes.ProblemJson.ShouldBe("application/problem+json");
    }

    /// <summary>
    /// Verifies that MediaTypes.PlainText has correct value.
    /// </summary>
    [Fact]
    public void MediaTypes_PlainText_HasCorrectValue()
    {
        MediaTypes.PlainText.ShouldBe("text/plain");
    }

    /// <summary>
    /// Verifies that MediaTypes.OctetStream has correct value.
    /// </summary>
    [Fact]
    public void MediaTypes_OctetStream_HasCorrectValue()
    {
        MediaTypes.OctetStream.ShouldBe("application/octet-stream");
    }
}
