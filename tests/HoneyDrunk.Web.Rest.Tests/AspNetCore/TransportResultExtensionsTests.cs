using HoneyDrunk.Web.Rest.Abstractions.Results;
using HoneyDrunk.Web.Rest.AspNetCore.Transport;
using Shouldly;

namespace HoneyDrunk.Web.Rest.Tests.AspNetCore;

/// <summary>
/// Tests for <see cref="TransportResultExtensions"/>.
/// </summary>
public sealed class TransportResultExtensionsTests
{
    /// <summary>
    /// Verifies that ToSuccessResult creates successful result with correlation ID.
    /// </summary>
    [Fact]
    public void ToSuccessResult_CreatesSuccessfulResult()
    {
        string correlationId = "test-correlation-123";

        ApiResult result = TransportResultExtensions.ToSuccessResult(correlationId);

        result.IsSuccess.ShouldBeTrue();
        result.CorrelationId.ShouldBe(correlationId);
    }

    /// <summary>
    /// Verifies that ToSuccessResult with null correlation ID works.
    /// </summary>
    [Fact]
    public void ToSuccessResult_WithNullCorrelationId_Works()
    {
        ApiResult result = TransportResultExtensions.ToSuccessResult(null);

        result.IsSuccess.ShouldBeTrue();
        result.CorrelationId.ShouldBeNull();
    }

    /// <summary>
    /// Verifies that generic ToSuccessResult creates result with data.
    /// </summary>
    [Fact]
    public void ToSuccessResult_Generic_CreatesResultWithData()
    {
        TestData data = new() { Id = 42, Name = "Test" };
        string correlationId = "test-correlation-456";

        ApiResult<TestData> result = TransportResultExtensions.ToSuccessResult(data, correlationId);

        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data.Id.ShouldBe(42);
        result.Data.Name.ShouldBe("Test");
        result.CorrelationId.ShouldBe(correlationId);
    }

    /// <summary>
    /// Verifies that ToFailureResult creates failed result.
    /// </summary>
    [Fact]
    public void ToFailureResult_CreatesFailedResult()
    {
        string errorMessage = "Something went wrong";
        string correlationId = "failure-correlation";

        ApiResult result = TransportResultExtensions.ToFailureResult(errorMessage, correlationId);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNull();
        result.Error.Message.ShouldBe(errorMessage);
        result.CorrelationId.ShouldBe(correlationId);
    }

    /// <summary>
    /// Verifies that generic ToFailureResult creates failed result.
    /// </summary>
    [Fact]
    public void ToFailureResult_Generic_CreatesFailedResult()
    {
        string errorMessage = "Not found";
        string correlationId = "failure-generic-correlation";

        ApiResult<TestData> result = TransportResultExtensions.ToFailureResult<TestData>(errorMessage, correlationId);

        result.IsSuccess.ShouldBeFalse();
        result.Data.ShouldBeNull();
        result.Error.ShouldNotBeNull();
        result.Error.Message.ShouldBe(errorMessage);
        result.CorrelationId.ShouldBe(correlationId);
    }

    /// <summary>
    /// Verifies that FromOutcome returns success when isSuccess is true.
    /// </summary>
    [Fact]
    public void FromOutcome_WhenSuccess_ReturnsSuccessResult()
    {
        string correlationId = "outcome-correlation";

        ApiResult result = TransportResultExtensions.FromOutcome(true, correlationId: correlationId);

        result.IsSuccess.ShouldBeTrue();
        result.CorrelationId.ShouldBe(correlationId);
    }

    /// <summary>
    /// Verifies that FromOutcome returns failure when isSuccess is false.
    /// </summary>
    [Fact]
    public void FromOutcome_WhenFailure_ReturnsFailureResult()
    {
        string errorMessage = "Operation failed";
        string correlationId = "outcome-failure-correlation";

        ApiResult result = TransportResultExtensions.FromOutcome(false, errorMessage, correlationId);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNull();
        result.Error.Message.ShouldBe(errorMessage);
        result.CorrelationId.ShouldBe(correlationId);
    }

    /// <summary>
    /// Verifies that FromOutcome uses default message when none provided.
    /// </summary>
    [Fact]
    public void FromOutcome_WhenFailure_UsesDefaultMessage()
    {
        ApiResult result = TransportResultExtensions.FromOutcome(false);

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldNotBeNull();
        result.Error.Message.ShouldBe("Operation failed.");
    }

    /// <summary>
    /// Verifies that generic FromOutcome returns success with data.
    /// </summary>
    [Fact]
    public void FromOutcome_Generic_WhenSuccess_ReturnsSuccessWithData()
    {
        TestData data = new() { Id = 99, Name = "Outcome Data" };
        string correlationId = "generic-outcome-correlation";

        ApiResult<TestData> result = TransportResultExtensions.FromOutcome(true, data, correlationId: correlationId);

        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data.Id.ShouldBe(99);
        result.CorrelationId.ShouldBe(correlationId);
    }

    /// <summary>
    /// Verifies that generic FromOutcome returns failure without data.
    /// </summary>
    [Fact]
    public void FromOutcome_Generic_WhenFailure_ReturnsFailureWithoutData()
    {
        TestData data = new() { Id = 99, Name = "Should not be used" };
        string errorMessage = "Generic operation failed";

        ApiResult<TestData> result = TransportResultExtensions.FromOutcome(false, data, errorMessage);

        result.IsSuccess.ShouldBeFalse();
        result.Data.ShouldBeNull();
        result.Error.ShouldNotBeNull();
        result.Error.Message.ShouldBe(errorMessage);
    }

    private sealed class TestData
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }
}
