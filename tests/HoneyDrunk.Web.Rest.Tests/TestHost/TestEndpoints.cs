using HoneyDrunk.Web.Rest.Abstractions.Results;
using HoneyDrunk.Web.Rest.AspNetCore.Context;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.ComponentModel.DataAnnotations;

namespace HoneyDrunk.Web.Rest.Tests.TestHost;

/// <summary>
/// Defines test endpoints for triggering various scenarios.
/// </summary>
public static class TestEndpoints
{
    /// <summary>
    /// Maps all test endpoints to the endpoint route builder.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <returns>The endpoint route builder for chaining.</returns>
    public static IEndpointRouteBuilder MapTestEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/test/success", SuccessHandler);
        endpoints.MapGet("/test/success-with-data", SuccessWithDataHandler);
        endpoints.MapGet("/test/not-found", NotFoundHandler);
        endpoints.MapGet("/test/argument-exception", ArgumentExceptionHandler);
        endpoints.MapGet("/test/argument-null-exception", ArgumentNullExceptionHandler);
        endpoints.MapGet("/test/invalid-operation", InvalidOperationHandler);
        endpoints.MapGet("/test/unauthorized", UnauthorizedHandler);
        endpoints.MapGet("/test/not-implemented", NotImplementedHandler);
        endpoints.MapGet("/test/not-supported", NotSupportedHandler);
        endpoints.MapGet("/test/internal-error", InternalErrorHandler);
        endpoints.MapGet("/test/correlation", CorrelationHandler);
        endpoints.MapGet("/test/cancelled", CancelledHandler);
        endpoints.MapPost("/test/validation", ValidationHandler);

        return endpoints;
    }

    private static IResult SuccessHandler()
    {
        return Results.Ok(ApiResult.Success());
    }

    private static IResult SuccessWithDataHandler()
    {
        TestData data = new() { Id = 1, Name = "Test" };
        return Results.Ok(ApiResult<TestData>.Success(data));
    }

    private static IResult NotFoundHandler()
    {
        throw new KeyNotFoundException("The requested item was not found.");
    }

    private static IResult ArgumentExceptionHandler()
    {
        throw new ArgumentException("Invalid argument provided.", "testParam");
    }

    private static IResult ArgumentNullExceptionHandler()
    {
        throw new ArgumentNullException("requiredParam");
    }

    private static IResult InvalidOperationHandler()
    {
        throw new InvalidOperationException("This operation is not valid in the current state.");
    }

    private static IResult UnauthorizedHandler()
    {
        throw new UnauthorizedAccessException("Access denied.");
    }

    private static IResult NotImplementedHandler()
    {
        throw new NotImplementedException("This feature is not implemented.");
    }

    private static IResult NotSupportedHandler()
    {
        throw new NotSupportedException("This operation is not supported.");
    }

    private static IResult InternalErrorHandler()
    {
#pragma warning disable CA2201 // Exception type System.Exception is not sufficiently specific - intentional for testing
        throw new Exception("An unexpected error occurred.");
#pragma warning restore CA2201
    }

    private static IResult CorrelationHandler(HttpContext context, ICorrelationIdAccessor correlationAccessor)
    {
        string? correlationId = correlationAccessor.CorrelationId;
        return Results.Ok(new { CorrelationId = correlationId });
    }

    private static async Task<IResult> CancelledHandler(CancellationToken cancellationToken)
    {
        await Task.Delay(1, cancellationToken);
        throw new OperationCanceledException("Request was cancelled.", cancellationToken);
    }

    private static IResult ValidationHandler([FromBody] TestValidationModel model)
    {
        return Results.Ok(new { model.Name, model.Email });
    }

    /// <summary>
    /// Test data class for success responses.
    /// </summary>
    public sealed class TestData
    {
        /// <summary>
        /// Gets or sets the ID.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Test model for validation scenarios.
    /// </summary>
    public sealed class TestValidationModel
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 100 characters.")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;
    }
}
