using HoneyDrunk.Web.Rest.Abstractions.Errors;
using HoneyDrunk.Web.Rest.AspNetCore.Serialization;
using HoneyDrunk.Web.Rest.Tests.TestHost;
using Shouldly;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace HoneyDrunk.Web.Rest.Tests.AspNetCore;

/// <summary>
/// Tests for model validation response shapes.
/// Note: Minimal API validation support is limited. These tests verify the error shape contract.
/// </summary>
public sealed class ModelValidationShapeTests : IDisposable
{
    private readonly TestApiFactory _factory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ModelValidationShapeTests"/> class.
    /// </summary>
    public ModelValidationShapeTests()
    {
        _factory = new TestApiFactory();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _factory.Dispose();
    }

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
    /// Verifies that ApiErrorResponse with validation errors has correct code.
    /// </summary>
    [Fact]
    public void ApiErrorResponse_WithValidationErrors_HasCorrectCode()
    {
        List<ValidationError> errors =
        [
            new ValidationError { Field = "name", Message = "Name is required." },
        ];

        ApiErrorResponse response = ApiErrorResponse.CreateValidationError("corr-id", errors);

        response.Error.Code.ShouldBe(ApiErrorCode.ValidationFailed);
        response.Error.Message.ShouldBe("One or more validation errors occurred.");
        response.ValidationErrors.ShouldNotBeNull();
        response.ValidationErrors.Count.ShouldBe(1);
    }

    /// <summary>
    /// Verifies that ApiErrorResponse validation shape serializes correctly.
    /// </summary>
    [Fact]
    public void ApiErrorResponse_ValidationShape_SerializesCorrectly()
    {
        List<ValidationError> errors =
        [
            new ValidationError { Field = "email", Message = "Email is required." },
            new ValidationError { Field = "password", Message = "Password must be at least 8 characters." },
        ];

        ApiErrorResponse response = ApiErrorResponse.CreateValidationError("test-corr", errors);

        string json = JsonSerializer.Serialize(response, JsonOptionsDefaults.SerializerOptions);

        json.ShouldContain("\"correlationId\":\"test-corr\"");
        json.ShouldContain("\"code\":\"VALIDATION_FAILED\"");
        json.ShouldContain("\"validationErrors\"");
        json.ShouldContain("\"field\":\"email\"");
        json.ShouldContain("\"field\":\"password\"");
    }

    /// <summary>
    /// Verifies that POST endpoint with valid data succeeds.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task PostEndpoint_WithValidData_Succeeds()
    {
        HttpClient client = _factory.CreateClient();
        object validData = new { Name = "Test User", Email = "test@example.com" };

        HttpResponseMessage response = await client.PostAsJsonAsync(new Uri("/test/validation", UriKind.Relative), validData);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifies that POST endpoint with empty body returns BadRequest.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task PostEndpoint_WithEmptyBody_ReturnsBadRequest()
    {
        HttpClient client = _factory.CreateClient();
        using StringContent content = new(string.Empty, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await client.PostAsync(new Uri("/test/validation", UriKind.Relative), content);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Verifies that ValidationError without Code omits the code field in JSON.
    /// </summary>
    [Fact]
    public void ValidationError_WithoutCode_OmitsCodeInJson()
    {
        ValidationError error = new()
        {
            Field = "name",
            Message = "Name is required.",
        };

        string json = JsonSerializer.Serialize(error, JsonOptionsDefaults.SerializerOptions);

        json.ShouldNotContain("\"code\"");
    }
}
