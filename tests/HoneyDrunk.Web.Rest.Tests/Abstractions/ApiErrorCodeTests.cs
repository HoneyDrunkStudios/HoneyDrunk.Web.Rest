using HoneyDrunk.Web.Rest.Abstractions.Errors;
using Shouldly;

namespace HoneyDrunk.Web.Rest.Tests.Abstractions;

/// <summary>
/// Tests for <see cref="ApiErrorCode"/> constants.
/// </summary>
public sealed class ApiErrorCodeTests
{
    /// <summary>
    /// Verifies that GeneralError has correct value.
    /// </summary>
    [Fact]
    public void ApiErrorCode_GeneralError_HasCorrectValue()
    {
        ApiErrorCode.GeneralError.ShouldBe("GENERAL_ERROR");
    }

    /// <summary>
    /// Verifies that ValidationFailed has correct value.
    /// </summary>
    [Fact]
    public void ApiErrorCode_ValidationFailed_HasCorrectValue()
    {
        ApiErrorCode.ValidationFailed.ShouldBe("VALIDATION_FAILED");
    }

    /// <summary>
    /// Verifies that NotFound has correct value.
    /// </summary>
    [Fact]
    public void ApiErrorCode_NotFound_HasCorrectValue()
    {
        ApiErrorCode.NotFound.ShouldBe("NOT_FOUND");
    }

    /// <summary>
    /// Verifies that Unauthorized has correct value.
    /// </summary>
    [Fact]
    public void ApiErrorCode_Unauthorized_HasCorrectValue()
    {
        ApiErrorCode.Unauthorized.ShouldBe("UNAUTHORIZED");
    }

    /// <summary>
    /// Verifies that Forbidden has correct value.
    /// </summary>
    [Fact]
    public void ApiErrorCode_Forbidden_HasCorrectValue()
    {
        ApiErrorCode.Forbidden.ShouldBe("FORBIDDEN");
    }

    /// <summary>
    /// Verifies that Conflict has correct value.
    /// </summary>
    [Fact]
    public void ApiErrorCode_Conflict_HasCorrectValue()
    {
        ApiErrorCode.Conflict.ShouldBe("CONFLICT");
    }

    /// <summary>
    /// Verifies that BadRequest has correct value.
    /// </summary>
    [Fact]
    public void ApiErrorCode_BadRequest_HasCorrectValue()
    {
        ApiErrorCode.BadRequest.ShouldBe("BAD_REQUEST");
    }

    /// <summary>
    /// Verifies that InternalError has correct value.
    /// </summary>
    [Fact]
    public void ApiErrorCode_InternalError_HasCorrectValue()
    {
        ApiErrorCode.InternalError.ShouldBe("INTERNAL_ERROR");
    }

    /// <summary>
    /// Verifies that NotImplemented has correct value.
    /// </summary>
    [Fact]
    public void ApiErrorCode_NotImplemented_HasCorrectValue()
    {
        ApiErrorCode.NotImplemented.ShouldBe("NOT_IMPLEMENTED");
    }

    /// <summary>
    /// Verifies that ServiceUnavailable has correct value.
    /// </summary>
    [Fact]
    public void ApiErrorCode_ServiceUnavailable_HasCorrectValue()
    {
        ApiErrorCode.ServiceUnavailable.ShouldBe("SERVICE_UNAVAILABLE");
    }
}
