using HoneyDrunk.Web.Rest.Abstractions.Errors;
using HoneyDrunk.Web.Rest.AspNetCore.Serialization;
using Shouldly;
using System.Text.Json;

namespace HoneyDrunk.Web.Rest.Tests.Abstractions;

/// <summary>
/// Tests for <see cref="ApiError"/>.
/// </summary>
public sealed class ApiErrorTests
{
    /// <summary>
    /// Verifies that ApiError serializes with all properties.
    /// </summary>
    [Fact]
    public void ApiError_SerializesWithAllProperties()
    {
        ApiError error = new()
        {
            Code = "CUSTOM_ERROR",
            Message = "A custom error occurred.",
            Details = "Stack trace here...",
            Target = "orderId",
        };

        string json = JsonSerializer.Serialize(error, JsonOptionsDefaults.SerializerOptions);

        json.ShouldContain("\"code\":\"CUSTOM_ERROR\"");
        json.ShouldContain("\"message\":\"A custom error occurred.\"");
        json.ShouldContain("\"details\":\"Stack trace here...\"");
        json.ShouldContain("\"target\":\"orderId\"");
    }

    /// <summary>
    /// Verifies that ApiError omits null optional properties.
    /// </summary>
    [Fact]
    public void ApiError_OmitsNullOptionalProperties()
    {
        ApiError error = new()
        {
            Code = "ERROR",
            Message = "Error message",
        };

        string json = JsonSerializer.Serialize(error, JsonOptionsDefaults.SerializerOptions);

        json.ShouldContain("\"code\":\"ERROR\"");
        json.ShouldContain("\"message\":\"Error message\"");
        json.ShouldNotContain("\"details\"");
        json.ShouldNotContain("\"target\"");
    }

    /// <summary>
    /// Verifies that ApiError deserializes correctly.
    /// </summary>
    [Fact]
    public void ApiError_DeserializesCorrectly()
    {
        string json = """
        {
            "code": "NOT_FOUND",
            "message": "Resource not found.",
            "details": "Additional info",
            "target": "userId"
        }
        """;

        ApiError? error = JsonSerializer.Deserialize<ApiError>(json, JsonOptionsDefaults.SerializerOptions);

        error.ShouldNotBeNull();
        error.Code.ShouldBe("NOT_FOUND");
        error.Message.ShouldBe("Resource not found.");
        error.Details.ShouldBe("Additional info");
        error.Target.ShouldBe("userId");
    }

    /// <summary>
    /// Verifies that ApiError deserializes correctly without optional properties.
    /// </summary>
    [Fact]
    public void ApiError_DeserializesCorrectly_WithoutOptionalProperties()
    {
        string json = """
        {
            "code": "BAD_REQUEST",
            "message": "Invalid request."
        }
        """;

        ApiError? error = JsonSerializer.Deserialize<ApiError>(json, JsonOptionsDefaults.SerializerOptions);

        error.ShouldNotBeNull();
        error.Code.ShouldBe("BAD_REQUEST");
        error.Message.ShouldBe("Invalid request.");
        error.Details.ShouldBeNull();
        error.Target.ShouldBeNull();
    }

    /// <summary>
    /// Verifies that ApiError with Details only serializes correctly.
    /// </summary>
    [Fact]
    public void ApiError_WithDetailsOnly_SerializesCorrectly()
    {
        ApiError error = new()
        {
            Code = "ERROR",
            Message = "Error",
            Details = "Details here",
        };

        string json = JsonSerializer.Serialize(error, JsonOptionsDefaults.SerializerOptions);

        json.ShouldContain("\"details\":\"Details here\"");
        json.ShouldNotContain("\"target\"");
    }

    /// <summary>
    /// Verifies that ApiError with Target only serializes correctly.
    /// </summary>
    [Fact]
    public void ApiError_WithTargetOnly_SerializesCorrectly()
    {
        ApiError error = new()
        {
            Code = "ERROR",
            Message = "Error",
            Target = "fieldName",
        };

        string json = JsonSerializer.Serialize(error, JsonOptionsDefaults.SerializerOptions);

        json.ShouldContain("\"target\":\"fieldName\"");
        json.ShouldNotContain("\"details\"");
    }
}
