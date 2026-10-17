using HoneyDrunk.Web.Rest.Abstractions.Errors;
using HoneyDrunk.Web.Rest.AspNetCore.Serialization;
using Shouldly;
using System.Text.Json;

namespace HoneyDrunk.Web.Rest.Tests.Abstractions;

/// <summary>
/// Tests for <see cref="ApiErrorResponse"/> serialization.
/// </summary>
public sealed class ApiErrorResponseSerializationTests
{
    /// <summary>
    /// Verifies that ApiErrorResponse serializes to expected JSON format.
    /// </summary>
    [Fact]
    public void ApiErrorResponse_SerializesToExpectedJson()
    {
        ApiErrorResponse response = new()
        {
            CorrelationId = "test-correlation-id",
            Error = new ApiError { Code = ApiErrorCode.BadRequest, Message = "Test error" },
            TraceId = "test-trace-id",
            Timestamp = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
        };

        string json = JsonSerializer.Serialize(response, JsonOptionsDefaults.SerializerOptions);

        json.ShouldContain("\"correlationId\":\"test-correlation-id\"");
        json.ShouldContain("\"code\":\"BAD_REQUEST\"");
        json.ShouldContain("\"message\":\"Test error\"");
        json.ShouldContain("\"traceId\":\"test-trace-id\"");
    }

    /// <summary>
    /// Verifies that ApiErrorResponse deserializes from JSON correctly.
    /// </summary>
    [Fact]
    public void ApiErrorResponse_DeserializesFromJson()
    {
        string json = """
        {
            "correlationId": "test-correlation-id",
            "error": { "code": "NOT_FOUND", "message": "Resource not found" },
            "traceId": "test-trace-id",
            "timestamp": "2024-01-01T00:00:00+00:00"
        }
        """;

        ApiErrorResponse? response = JsonSerializer.Deserialize<ApiErrorResponse>(json, JsonOptionsDefaults.SerializerOptions);

        response.ShouldNotBeNull();
        response.CorrelationId.ShouldBe("test-correlation-id");
        response.Error.Code.ShouldBe("NOT_FOUND");
        response.Error.Message.ShouldBe("Resource not found");
        response.TraceId.ShouldBe("test-trace-id");
    }

    /// <summary>
    /// Verifies that ApiErrorResponse with validation errors serializes correctly.
    /// </summary>
    [Fact]
    public void ApiErrorResponse_WithValidationErrors_SerializesCorrectly()
    {
        List<ValidationError> validationErrors =
        [
            new ValidationError { Field = "email", Message = "Email is required." },
            new ValidationError { Field = "name", Message = "Name is too short." },
        ];

        ApiErrorResponse response = ApiErrorResponse.CreateValidationError("corr-id", validationErrors);

        string json = JsonSerializer.Serialize(response, JsonOptionsDefaults.SerializerOptions);

        json.ShouldContain("\"validationErrors\"");
        json.ShouldContain("\"field\":\"email\"");
        json.ShouldContain("\"message\":\"Email is required.\"");
        json.ShouldContain("\"field\":\"name\"");
    }

    /// <summary>
    /// Verifies that ApiErrorResponse without validation errors omits the field.
    /// </summary>
    [Fact]
    public void ApiErrorResponse_WithoutValidationErrors_OmitsValidationErrorsField()
    {
        ApiErrorResponse response = ApiErrorResponse.Create("corr-id", "Simple error");

        string json = JsonSerializer.Serialize(response, JsonOptionsDefaults.SerializerOptions);

        json.ShouldNotContain("validationErrors");
    }

    /// <summary>
    /// Verifies that CreateNotFound sets the correct error code.
    /// </summary>
    [Fact]
    public void ApiErrorResponse_CreateNotFound_SetsCorrectCode()
    {
        ApiErrorResponse response = ApiErrorResponse.CreateNotFound("corr-id");

        response.Error.Code.ShouldBe(ApiErrorCode.NotFound);
        response.Error.Message.ShouldBe("The requested resource was not found.");
    }

    /// <summary>
    /// Verifies that CreateUnauthorized sets the correct error code.
    /// </summary>
    [Fact]
    public void ApiErrorResponse_CreateUnauthorized_SetsCorrectCode()
    {
        ApiErrorResponse response = ApiErrorResponse.CreateUnauthorized("corr-id");

        response.Error.Code.ShouldBe(ApiErrorCode.Unauthorized);
    }

    /// <summary>
    /// Verifies that CreateForbidden sets the correct error code.
    /// </summary>
    [Fact]
    public void ApiErrorResponse_CreateForbidden_SetsCorrectCode()
    {
        ApiErrorResponse response = ApiErrorResponse.CreateForbidden("corr-id");

        response.Error.Code.ShouldBe(ApiErrorCode.Forbidden);
    }

    /// <summary>
    /// Verifies that CreateInternalError sets the correct error code.
    /// </summary>
    [Fact]
    public void ApiErrorResponse_CreateInternalError_SetsCorrectCode()
    {
        ApiErrorResponse response = ApiErrorResponse.CreateInternalError("corr-id");

        response.Error.Code.ShouldBe(ApiErrorCode.InternalError);
    }
}
