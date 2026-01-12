using HoneyDrunk.Web.Rest.Abstractions.Errors;
using HoneyDrunk.Web.Rest.AspNetCore.Errors;
using HoneyDrunk.Web.Rest.AspNetCore.Middleware;
using Shouldly;
using System.Net;

namespace HoneyDrunk.Web.Rest.Tests.AspNetCore;

/// <summary>
/// Tests for <see cref="ExceptionToApiErrorMapper"/>.
/// </summary>
public sealed class ExceptionToApiErrorMapperTests
{
    /// <summary>
    /// Verifies that ArgumentNullException maps to 400 Bad Request.
    /// </summary>
    [Fact]
    public void Map_ArgumentNullException_ReturnsBadRequest()
    {
        ArgumentNullException exception = new("paramName");

        ExceptionMappingResult result = ExceptionToApiErrorMapper.Map(exception);

        result.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ErrorCode.ShouldBe(ApiErrorCode.BadRequest);
        result.Message.ShouldContain("paramName");
    }

    /// <summary>
    /// Verifies that ArgumentException maps to 400 Bad Request.
    /// </summary>
    [Fact]
    public void Map_ArgumentException_ReturnsBadRequest()
    {
        ArgumentException exception = new("Invalid argument value");

        ExceptionMappingResult result = ExceptionToApiErrorMapper.Map(exception);

        result.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        result.ErrorCode.ShouldBe(ApiErrorCode.BadRequest);
        result.Message.ShouldBe("Invalid argument value");
    }

    /// <summary>
    /// Verifies that InvalidOperationException maps to 409 Conflict.
    /// </summary>
    [Fact]
    public void Map_InvalidOperationException_ReturnsConflict()
    {
        InvalidOperationException exception = new("Invalid state");

        ExceptionMappingResult result = ExceptionToApiErrorMapper.Map(exception);

        result.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        result.ErrorCode.ShouldBe(ApiErrorCode.Conflict);
        result.Message.ShouldBe("Invalid state");
    }

    /// <summary>
    /// Verifies that KeyNotFoundException maps to 404 Not Found.
    /// </summary>
    [Fact]
    public void Map_KeyNotFoundException_ReturnsNotFound()
    {
        KeyNotFoundException exception = new("Key not found");

        ExceptionMappingResult result = ExceptionToApiErrorMapper.Map(exception);

        result.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        result.ErrorCode.ShouldBe(ApiErrorCode.NotFound);
        result.Message.ShouldBe("The requested resource was not found.");
    }

    /// <summary>
    /// Verifies that UnauthorizedAccessException maps to 403 Forbidden.
    /// </summary>
    [Fact]
    public void Map_UnauthorizedAccessException_ReturnsForbidden()
    {
        UnauthorizedAccessException exception = new("Access denied");

        ExceptionMappingResult result = ExceptionToApiErrorMapper.Map(exception);

        result.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        result.ErrorCode.ShouldBe(ApiErrorCode.Forbidden);
    }

    /// <summary>
    /// Verifies that NotImplementedException maps to 501 Not Implemented.
    /// </summary>
    [Fact]
    public void Map_NotImplementedException_ReturnsNotImplemented()
    {
        NotImplementedException exception = new("Not implemented");

        ExceptionMappingResult result = ExceptionToApiErrorMapper.Map(exception);

        result.StatusCode.ShouldBe(HttpStatusCode.NotImplemented);
        result.ErrorCode.ShouldBe(ApiErrorCode.NotImplemented);
    }

    /// <summary>
    /// Verifies that OperationCanceledException maps to 499.
    /// </summary>
    [Fact]
    public void Map_OperationCanceledException_Returns499()
    {
        OperationCanceledException exception = new("Cancelled");

        ExceptionMappingResult result = ExceptionToApiErrorMapper.Map(exception);

        result.StatusCode.ShouldBe((HttpStatusCode)499);
        result.ErrorCode.ShouldBe(ApiErrorCode.GeneralError);
    }

    /// <summary>
    /// Verifies that generic Exception maps to 500 Internal Server Error.
    /// </summary>
    [Fact]
    public void Map_GenericException_ReturnsInternalServerError()
    {
#pragma warning disable CA2201 // Do not raise reserved exception types - intentional for testing
        Exception exception = new("Something went wrong");
#pragma warning restore CA2201

        ExceptionMappingResult result = ExceptionToApiErrorMapper.Map(exception);

        result.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        result.ErrorCode.ShouldBe(ApiErrorCode.InternalError);
        result.Message.ShouldBe("An internal server error occurred.");
    }

    /// <summary>
    /// Verifies that Map throws on null exception.
    /// </summary>
    [Fact]
    public void Map_NullException_Throws()
    {
        Should.Throw<ArgumentNullException>(() => ExceptionToApiErrorMapper.Map(null!));
    }

    /// <summary>
    /// Verifies that GetUnauthorizedStatusCode returns correct status for authenticated user.
    /// </summary>
    [Fact]
    public void GetUnauthorizedStatusCode_Authenticated_ReturnsForbidden()
    {
        HttpStatusCode result = ExceptionToApiErrorMapper.GetUnauthorizedStatusCode(isAuthenticated: true);

        result.ShouldBe(HttpStatusCode.Forbidden);
    }

    /// <summary>
    /// Verifies that GetUnauthorizedStatusCode returns correct status for unauthenticated user.
    /// </summary>
    [Fact]
    public void GetUnauthorizedStatusCode_NotAuthenticated_ReturnsUnauthorized()
    {
        HttpStatusCode result = ExceptionToApiErrorMapper.GetUnauthorizedStatusCode(isAuthenticated: false);

        result.ShouldBe(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// Verifies that GetUnauthorizedErrorCode returns correct code for authenticated user.
    /// </summary>
    [Fact]
    public void GetUnauthorizedErrorCode_Authenticated_ReturnsForbidden()
    {
        string result = ExceptionToApiErrorMapper.GetUnauthorizedErrorCode(isAuthenticated: true);

        result.ShouldBe(ApiErrorCode.Forbidden);
    }

    /// <summary>
    /// Verifies that GetUnauthorizedErrorCode returns correct code for unauthenticated user.
    /// </summary>
    [Fact]
    public void GetUnauthorizedErrorCode_NotAuthenticated_ReturnsUnauthorized()
    {
        string result = ExceptionToApiErrorMapper.GetUnauthorizedErrorCode(isAuthenticated: false);

        result.ShouldBe(ApiErrorCode.Unauthorized);
    }
}
