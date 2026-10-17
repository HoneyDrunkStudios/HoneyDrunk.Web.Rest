using HoneyDrunk.Web.Rest.Abstractions.Errors;
using HoneyDrunk.Web.Rest.Abstractions.Results;
using Shouldly;

namespace HoneyDrunk.Web.Rest.Tests.Abstractions;

/// <summary>
/// Tests for <see cref="ApiResult{T}"/> factory methods.
/// </summary>
public sealed class ApiResultOfTTests
{
    /// <summary>
    /// Verifies that Success creates correct result with data.
    /// </summary>
    [Fact]
    public void Success_CreatesCorrectResult()
    {
        TestData data = new() { Id = 42, Name = "Test" };

        ApiResult<TestData> result = ApiResult<TestData>.Success(data, "corr-123");

        result.Status.ShouldBe(ApiResultStatus.Success);
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data.Id.ShouldBe(42);
        result.Data.Name.ShouldBe("Test");
        result.CorrelationId.ShouldBe("corr-123");
        result.Error.ShouldBeNull();
    }

    /// <summary>
    /// Verifies that Fail creates correct result without data.
    /// </summary>
    [Fact]
    public void Fail_CreatesCorrectResult()
    {
        ApiResult<TestData> result = ApiResult<TestData>.Fail("Error message", ApiErrorCode.BadRequest, "corr-456");

        result.Status.ShouldBe(ApiResultStatus.Failed);
        result.IsSuccess.ShouldBeFalse();
        result.Data.ShouldBeNull();
        result.CorrelationId.ShouldBe("corr-456");
        result.Error.ShouldNotBeNull();
        result.Error.Code.ShouldBe(ApiErrorCode.BadRequest);
        result.Error.Message.ShouldBe("Error message");
    }

    /// <summary>
    /// Verifies that NotFound creates correct result.
    /// </summary>
    [Fact]
    public void NotFound_CreatesCorrectResult()
    {
        ApiResult<TestData> result = ApiResult<TestData>.NotFound("Item not found", "corr-789");

        result.Status.ShouldBe(ApiResultStatus.NotFound);
        result.IsSuccess.ShouldBeFalse();
        result.Data.ShouldBeNull();
        result.Error.ShouldNotBeNull();
        result.Error.Code.ShouldBe(ApiErrorCode.NotFound);
        result.Error.Message.ShouldBe("Item not found");
    }

    /// <summary>
    /// Verifies that NotFound with default message works.
    /// </summary>
    [Fact]
    public void NotFound_WithDefaultMessage_Works()
    {
        ApiResult<TestData> result = ApiResult<TestData>.NotFound();

        result.Error.ShouldNotBeNull();
        result.Error.Message.ShouldBe("The requested resource was not found.");
    }

    /// <summary>
    /// Verifies that Unauthorized creates correct result.
    /// </summary>
    [Fact]
    public void Unauthorized_CreatesCorrectResult()
    {
        ApiResult<TestData> result = ApiResult<TestData>.Unauthorized("Please log in");

        result.Status.ShouldBe(ApiResultStatus.Unauthorized);
        result.Error.ShouldNotBeNull();
        result.Error.Code.ShouldBe(ApiErrorCode.Unauthorized);
        result.Error.Message.ShouldBe("Please log in");
    }

    /// <summary>
    /// Verifies that Unauthorized with default message works.
    /// </summary>
    [Fact]
    public void Unauthorized_WithDefaultMessage_Works()
    {
        ApiResult<TestData> result = ApiResult<TestData>.Unauthorized();

        result.Error.ShouldNotBeNull();
        result.Error.Message.ShouldBe("Authentication is required.");
    }

    /// <summary>
    /// Verifies that Forbidden creates correct result.
    /// </summary>
    [Fact]
    public void Forbidden_CreatesCorrectResult()
    {
        ApiResult<TestData> result = ApiResult<TestData>.Forbidden("Access denied");

        result.Status.ShouldBe(ApiResultStatus.Forbidden);
        result.Error.ShouldNotBeNull();
        result.Error.Code.ShouldBe(ApiErrorCode.Forbidden);
        result.Error.Message.ShouldBe("Access denied");
    }

    /// <summary>
    /// Verifies that Forbidden with default message works.
    /// </summary>
    [Fact]
    public void Forbidden_WithDefaultMessage_Works()
    {
        ApiResult<TestData> result = ApiResult<TestData>.Forbidden();

        result.Error.ShouldNotBeNull();
        result.Error.Message.ShouldBe("You do not have permission to access this resource.");
    }

    /// <summary>
    /// Verifies that Conflict creates correct result.
    /// </summary>
    [Fact]
    public void Conflict_CreatesCorrectResult()
    {
        ApiResult<TestData> result = ApiResult<TestData>.Conflict("Duplicate entry", "corr-conflict");

        result.Status.ShouldBe(ApiResultStatus.Conflict);
        result.Error.ShouldNotBeNull();
        result.Error.Code.ShouldBe(ApiErrorCode.Conflict);
        result.Error.Message.ShouldBe("Duplicate entry");
        result.CorrelationId.ShouldBe("corr-conflict");
    }

    /// <summary>
    /// Verifies that InternalError creates correct result.
    /// </summary>
    [Fact]
    public void InternalError_CreatesCorrectResult()
    {
        ApiResult<TestData> result = ApiResult<TestData>.InternalError("Server exploded");

        result.Status.ShouldBe(ApiResultStatus.Error);
        result.Error.ShouldNotBeNull();
        result.Error.Code.ShouldBe(ApiErrorCode.InternalError);
        result.Error.Message.ShouldBe("Server exploded");
    }

    /// <summary>
    /// Verifies that InternalError with default message works.
    /// </summary>
    [Fact]
    public void InternalError_WithDefaultMessage_Works()
    {
        ApiResult<TestData> result = ApiResult<TestData>.InternalError();

        result.Error.ShouldNotBeNull();
        result.Error.Message.ShouldBe("An internal server error occurred.");
    }

    /// <summary>
    /// Verifies that FromResult copies all properties.
    /// </summary>
    [Fact]
    public void FromResult_CopiesAllProperties()
    {
        DateTimeOffset timestamp = DateTimeOffset.UtcNow;
        ApiResult source = new()
        {
            Status = ApiResultStatus.NotFound,
            Error = new ApiError { Code = ApiErrorCode.NotFound, Message = "Not found" },
            CorrelationId = "source-corr",
            TraceId = "source-trace",
            Timestamp = timestamp,
        };

        ApiResult<TestData> result = ApiResult<TestData>.FromResult(source);

        result.Status.ShouldBe(ApiResultStatus.NotFound);
        result.Error.ShouldNotBeNull();
        result.Error.Code.ShouldBe(ApiErrorCode.NotFound);
        result.CorrelationId.ShouldBe("source-corr");
        result.TraceId.ShouldBe("source-trace");
        result.Timestamp.ShouldBe(timestamp);
        result.Data.ShouldBeNull();
    }

    /// <summary>
    /// Verifies that Success without correlation ID works.
    /// </summary>
    [Fact]
    public void Success_WithoutCorrelationId_Works()
    {
        TestData data = new() { Id = 1, Name = "Test" };

        ApiResult<TestData> result = ApiResult<TestData>.Success(data);

        result.CorrelationId.ShouldBeNull();
        result.Data.ShouldNotBeNull();
    }

    /// <summary>
    /// Verifies that Fail with default error code works.
    /// </summary>
    [Fact]
    public void Fail_WithDefaultErrorCode_Works()
    {
        ApiResult<TestData> result = ApiResult<TestData>.Fail("Something went wrong");

        result.Error.ShouldNotBeNull();
        result.Error.Code.ShouldBe(ApiErrorCode.GeneralError);
    }

    private sealed class TestData
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }
}
