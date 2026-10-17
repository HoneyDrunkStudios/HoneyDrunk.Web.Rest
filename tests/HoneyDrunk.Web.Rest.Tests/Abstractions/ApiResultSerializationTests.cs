using HoneyDrunk.Web.Rest.Abstractions.Errors;
using HoneyDrunk.Web.Rest.Abstractions.Results;
using HoneyDrunk.Web.Rest.AspNetCore.Serialization;
using Shouldly;
using System.Text.Json;

namespace HoneyDrunk.Web.Rest.Tests.Abstractions;

/// <summary>
/// Tests for <see cref="ApiResult"/> and <see cref="ApiResult{T}"/> serialization.
/// </summary>
public sealed class ApiResultSerializationTests
{
    /// <summary>
    /// Verifies that ApiResult success serializes to expected JSON.
    /// </summary>
    [Fact]
    public void ApiResult_Success_SerializesToExpectedJson()
    {
        ApiResult result = ApiResult.Success("test-correlation-id");

        string json = JsonSerializer.Serialize(result, JsonOptionsDefaults.SerializerOptions);

        json.ShouldContain("\"status\":\"success\"");
        json.ShouldContain("\"correlationId\":\"test-correlation-id\"");
        json.ShouldNotContain("\"error\"");
    }

    /// <summary>
    /// Verifies that ApiResult fail serializes to expected JSON.
    /// </summary>
    [Fact]
    public void ApiResult_Fail_SerializesToExpectedJson()
    {
        ApiResult result = ApiResult.Fail("Something went wrong", ApiErrorCode.BadRequest, "test-correlation-id");

        string json = JsonSerializer.Serialize(result, JsonOptionsDefaults.SerializerOptions);

        json.ShouldContain("\"status\":\"failed\"");
        json.ShouldContain("\"error\"");
        json.ShouldContain("\"code\":\"BAD_REQUEST\"");
        json.ShouldContain("\"message\":\"Something went wrong\"");
    }

    /// <summary>
    /// Verifies that ApiResult with data serializes correctly.
    /// </summary>
    [Fact]
    public void ApiResultT_Success_WithData_SerializesToExpectedJson()
    {
        TestDto dto = new() { Id = 42, Name = "Test Item" };
        ApiResult<TestDto> result = ApiResult<TestDto>.Success(dto, "test-correlation-id");

        string json = JsonSerializer.Serialize(result, JsonOptionsDefaults.SerializerOptions);

        json.ShouldContain("\"status\":\"success\"");
        json.ShouldContain("\"data\"");
        json.ShouldContain("\"id\":42");
        json.ShouldContain("\"name\":\"Test Item\"");
    }

    /// <summary>
    /// Verifies that ApiResult fail omits data field.
    /// </summary>
    [Fact]
    public void ApiResultT_Fail_OmitsData()
    {
        ApiResult<TestDto> result = ApiResult<TestDto>.Fail("Not found", ApiErrorCode.NotFound);

        string json = JsonSerializer.Serialize(result, JsonOptionsDefaults.SerializerOptions);

        json.ShouldContain("\"status\":\"failed\"");
        json.ShouldNotContain("\"data\"");
    }

    /// <summary>
    /// Verifies that ApiResult deserializes from JSON.
    /// </summary>
    [Fact]
    public void ApiResult_DeserializesFromJson()
    {
        string json = """
        {
            "status": "success",
            "correlationId": "test-id",
            "timestamp": "2024-01-01T00:00:00+00:00"
        }
        """;

        ApiResult? result = JsonSerializer.Deserialize<ApiResult>(json, JsonOptionsDefaults.SerializerOptions);

        result.ShouldNotBeNull();
        result.Status.ShouldBe(ApiResultStatus.Success);
        result.CorrelationId.ShouldBe("test-id");
        result.IsSuccess.ShouldBeTrue();
    }

    /// <summary>
    /// Verifies that generic ApiResult deserializes from JSON with data.
    /// </summary>
    [Fact]
    public void ApiResultT_DeserializesFromJson()
    {
        string json = """
        {
            "status": "success",
            "correlationId": "test-id",
            "data": { "id": 123, "name": "Deserialized" }
        }
        """;

        ApiResult<TestDto>? result = JsonSerializer.Deserialize<ApiResult<TestDto>>(json, JsonOptionsDefaults.SerializerOptions);

        result.ShouldNotBeNull();
        result.Status.ShouldBe(ApiResultStatus.Success);
        result.Data.ShouldNotBeNull();
        result.Data.Id.ShouldBe(123);
        result.Data.Name.ShouldBe("Deserialized");
    }

    /// <summary>
    /// Verifies that NotFound sets correct status.
    /// </summary>
    [Fact]
    public void ApiResult_NotFound_SetsCorrectStatus()
    {
        ApiResult result = ApiResult.NotFound();

        result.Status.ShouldBe(ApiResultStatus.NotFound);
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNull();
        result.Error.Code.ShouldBe(ApiErrorCode.NotFound);
    }

    /// <summary>
    /// Verifies that Unauthorized sets correct status.
    /// </summary>
    [Fact]
    public void ApiResult_Unauthorized_SetsCorrectStatus()
    {
        ApiResult result = ApiResult.Unauthorized();

        result.Status.ShouldBe(ApiResultStatus.Unauthorized);
        result.Error.ShouldNotBeNull();
        result.Error.Code.ShouldBe(ApiErrorCode.Unauthorized);
    }

    /// <summary>
    /// Verifies that Forbidden sets correct status.
    /// </summary>
    [Fact]
    public void ApiResult_Forbidden_SetsCorrectStatus()
    {
        ApiResult result = ApiResult.Forbidden();

        result.Status.ShouldBe(ApiResultStatus.Forbidden);
        result.Error.ShouldNotBeNull();
        result.Error.Code.ShouldBe(ApiErrorCode.Forbidden);
    }

    /// <summary>
    /// Verifies that Conflict sets correct status.
    /// </summary>
    [Fact]
    public void ApiResult_Conflict_SetsCorrectStatus()
    {
        ApiResult result = ApiResult.Conflict("Duplicate entry");

        result.Status.ShouldBe(ApiResultStatus.Conflict);
        result.Error.ShouldNotBeNull();
        result.Error.Code.ShouldBe(ApiErrorCode.Conflict);
    }

    /// <summary>
    /// Verifies that InternalError sets correct status.
    /// </summary>
    [Fact]
    public void ApiResult_InternalError_SetsCorrectStatus()
    {
        ApiResult result = ApiResult.InternalError();

        result.Status.ShouldBe(ApiResultStatus.Error);
        result.Error.ShouldNotBeNull();
        result.Error.Code.ShouldBe(ApiErrorCode.InternalError);
    }

    /// <summary>
    /// Verifies that FromResult copies properties correctly.
    /// </summary>
    [Fact]
    public void ApiResultT_FromResult_CopiesProperties()
    {
        ApiResult source = ApiResult.NotFound("Custom message", "source-correlation");

        ApiResult<TestDto> converted = ApiResult<TestDto>.FromResult(source);

        converted.Status.ShouldBe(ApiResultStatus.NotFound);
        converted.CorrelationId.ShouldBe("source-correlation");
        converted.Error.ShouldNotBeNull();
        converted.Error.Message.ShouldBe("Custom message");
        converted.Data.ShouldBeNull();
    }

    private sealed class TestDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }
}
