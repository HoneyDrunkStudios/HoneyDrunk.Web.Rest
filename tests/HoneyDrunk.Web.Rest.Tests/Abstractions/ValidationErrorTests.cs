using HoneyDrunk.Web.Rest.Abstractions.Errors;
using HoneyDrunk.Web.Rest.AspNetCore.Serialization;
using Shouldly;
using System.Text.Json;

namespace HoneyDrunk.Web.Rest.Tests.Abstractions;

/// <summary>
/// Tests for <see cref="ValidationError"/>.
/// </summary>
public sealed class ValidationErrorTests
{
    /// <summary>
    /// Verifies that ValidationError serializes correctly.
    /// </summary>
    [Fact]
    public void ValidationError_SerializesCorrectly()
    {
        ValidationError error = new()
        {
            Field = "email",
            Message = "Email is required.",
            Code = "REQUIRED",
        };

        string json = JsonSerializer.Serialize(error, JsonOptionsDefaults.SerializerOptions);

        json.ShouldContain("\"field\":\"email\"");
        json.ShouldContain("\"message\":\"Email is required.\"");
        json.ShouldContain("\"code\":\"REQUIRED\"");
    }

    /// <summary>
    /// Verifies that ValidationError without code omits code field.
    /// </summary>
    [Fact]
    public void ValidationError_WithoutCode_OmitsCodeField()
    {
        ValidationError error = new()
        {
            Field = "name",
            Message = "Name is too short.",
        };

        string json = JsonSerializer.Serialize(error, JsonOptionsDefaults.SerializerOptions);

        json.ShouldContain("\"field\":\"name\"");
        json.ShouldContain("\"message\":\"Name is too short.\"");
        json.ShouldNotContain("\"code\"");
    }

    /// <summary>
    /// Verifies that ValidationError deserializes correctly.
    /// </summary>
    [Fact]
    public void ValidationError_DeserializesCorrectly()
    {
        string json = """
        {
            "field": "password",
            "message": "Password must be at least 8 characters.",
            "code": "MIN_LENGTH"
        }
        """;

        ValidationError? error = JsonSerializer.Deserialize<ValidationError>(json, JsonOptionsDefaults.SerializerOptions);

        error.ShouldNotBeNull();
        error.Field.ShouldBe("password");
        error.Message.ShouldBe("Password must be at least 8 characters.");
        error.Code.ShouldBe("MIN_LENGTH");
    }

    /// <summary>
    /// Verifies that ValidationError deserializes correctly without code.
    /// </summary>
    [Fact]
    public void ValidationError_DeserializesCorrectly_WithoutCode()
    {
        string json = """
        {
            "field": "username",
            "message": "Username is already taken."
        }
        """;

        ValidationError? error = JsonSerializer.Deserialize<ValidationError>(json, JsonOptionsDefaults.SerializerOptions);

        error.ShouldNotBeNull();
        error.Field.ShouldBe("username");
        error.Message.ShouldBe("Username is already taken.");
        error.Code.ShouldBeNull();
    }

    /// <summary>
    /// Verifies that multiple ValidationErrors serialize correctly.
    /// </summary>
    [Fact]
    public void MultipleValidationErrors_SerializeCorrectly()
    {
        List<ValidationError> errors =
        [
            new() { Field = "email", Message = "Email is required." },
            new() { Field = "password", Message = "Password is required." },
            new() { Field = "password", Message = "Password must be at least 8 characters.", Code = "MIN_LENGTH" },
        ];

        string json = JsonSerializer.Serialize(errors, JsonOptionsDefaults.SerializerOptions);

        json.ShouldContain("\"field\":\"email\"");
        json.ShouldContain("\"field\":\"password\"");
        json.ShouldContain("Password is required.");
        json.ShouldContain("Password must be at least 8 characters.");
    }
}
