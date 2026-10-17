using HoneyDrunk.Web.Rest.Abstractions.Telemetry;
using Shouldly;

namespace HoneyDrunk.Web.Rest.Tests.Abstractions;

/// <summary>
/// Tests for <see cref="RestTelemetryTags"/> constants.
/// </summary>
public sealed class TelemetryTagsTests
{
    /// <summary>
    /// Verifies that CorrelationId tag has correct value.
    /// </summary>
    [Fact]
    public void RestTelemetryTags_CorrelationId_HasCorrectValue()
    {
        RestTelemetryTags.CorrelationId.ShouldBe("correlation.id");
    }

    /// <summary>
    /// Verifies that RequestId tag has correct value.
    /// </summary>
    [Fact]
    public void RestTelemetryTags_RequestId_HasCorrectValue()
    {
        RestTelemetryTags.RequestId.ShouldBe("request.id");
    }

    /// <summary>
    /// Verifies that HttpMethod tag has correct value.
    /// </summary>
    [Fact]
    public void RestTelemetryTags_HttpMethod_HasCorrectValue()
    {
        RestTelemetryTags.HttpMethod.ShouldBe("http.method");
    }

    /// <summary>
    /// Verifies that HttpRoute tag has correct value.
    /// </summary>
    [Fact]
    public void RestTelemetryTags_HttpRoute_HasCorrectValue()
    {
        RestTelemetryTags.HttpRoute.ShouldBe("http.route");
    }

    /// <summary>
    /// Verifies that HttpStatusCode tag has correct value.
    /// </summary>
    [Fact]
    public void RestTelemetryTags_HttpStatusCode_HasCorrectValue()
    {
        RestTelemetryTags.HttpStatusCode.ShouldBe("http.status_code");
    }

    /// <summary>
    /// Verifies that HttpPath tag has correct value.
    /// </summary>
    [Fact]
    public void RestTelemetryTags_HttpPath_HasCorrectValue()
    {
        RestTelemetryTags.HttpPath.ShouldBe("http.path");
    }

    /// <summary>
    /// Verifies that ErrorType tag has correct value.
    /// </summary>
    [Fact]
    public void RestTelemetryTags_ErrorType_HasCorrectValue()
    {
        RestTelemetryTags.ErrorType.ShouldBe("error.type");
    }

    /// <summary>
    /// Verifies that ErrorCode tag has correct value.
    /// </summary>
    [Fact]
    public void RestTelemetryTags_ErrorCode_HasCorrectValue()
    {
        RestTelemetryTags.ErrorCode.ShouldBe("error.code");
    }

    /// <summary>
    /// Verifies that UserId tag has correct value.
    /// </summary>
    [Fact]
    public void RestTelemetryTags_UserId_HasCorrectValue()
    {
        RestTelemetryTags.UserId.ShouldBe("user.id");
    }

    /// <summary>
    /// Verifies that TenantId tag has correct value.
    /// </summary>
    [Fact]
    public void RestTelemetryTags_TenantId_HasCorrectValue()
    {
        RestTelemetryTags.TenantId.ShouldBe("tenant.id");
    }

    /// <summary>
    /// Verifies that ServiceName tag has correct value.
    /// </summary>
    [Fact]
    public void RestTelemetryTags_ServiceName_HasCorrectValue()
    {
        RestTelemetryTags.ServiceName.ShouldBe("service.name");
    }

    /// <summary>
    /// Verifies that ServiceVersion tag has correct value.
    /// </summary>
    [Fact]
    public void RestTelemetryTags_ServiceVersion_HasCorrectValue()
    {
        RestTelemetryTags.ServiceVersion.ShouldBe("service.version");
    }

    /// <summary>
    /// Verifies that RequestDurationMs tag has correct value.
    /// </summary>
    [Fact]
    public void RestTelemetryTags_RequestDurationMs_HasCorrectValue()
    {
        RestTelemetryTags.RequestDurationMs.ShouldBe("request.duration_ms");
    }
}
