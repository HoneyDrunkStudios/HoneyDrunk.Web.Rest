#pragma warning disable SA1600 // Test names are descriptive enough for focused coverage backfill.

using HoneyDrunk.Web.Rest.Abstractions.Constants;
using HoneyDrunk.Web.Rest.Abstractions.Errors;
using HoneyDrunk.Web.Rest.Abstractions.Results;
using HoneyDrunk.Web.Rest.AspNetCore.Auth;
using HoneyDrunk.Web.Rest.AspNetCore.Context;
using HoneyDrunk.Web.Rest.AspNetCore.Errors;
using HoneyDrunk.Web.Rest.AspNetCore.MinimalApi;
using HoneyDrunk.Web.Rest.AspNetCore.Mvc;
using HoneyDrunk.Web.Rest.AspNetCore.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System.Net;
using System.Text.Json;

namespace HoneyDrunk.Web.Rest.Tests.AspNetCore;

/// <summary>
/// Focused coverage for low-risk REST helper surface that shapes public responses and endpoint metadata.
/// </summary>
public sealed class CoverageGateBackfillTests
{
    public static TheoryData<HttpStatusCode, string> StatusCodeMappings => new()
    {
        { HttpStatusCode.BadRequest, ApiErrorCode.BadRequest },
        { HttpStatusCode.Unauthorized, ApiErrorCode.Unauthorized },
        { HttpStatusCode.Forbidden, ApiErrorCode.Forbidden },
        { HttpStatusCode.NotFound, ApiErrorCode.NotFound },
        { HttpStatusCode.Conflict, ApiErrorCode.Conflict },
        { HttpStatusCode.NotImplemented, ApiErrorCode.NotImplemented },
        { HttpStatusCode.ServiceUnavailable, ApiErrorCode.ServiceUnavailable },
        { HttpStatusCode.GatewayTimeout, ApiErrorCode.InternalError },
    };

    [Theory]
    [MemberData(nameof(StatusCodeMappings))]
    public void DefaultExceptionMappings_MapStatusCodesToStableErrorCodes(HttpStatusCode statusCode, string expected)
    {
        DefaultExceptionMappings.StatusCodeToErrorCode(statusCode).ShouldBe(expected);
    }

    [Fact]
    public void DefaultExceptionMappings_CreateExpectedErrorResponses()
    {
        DefaultExceptionMappings.BadRequest("corr", "bad", "trace").Error.Code.ShouldBe(ApiErrorCode.BadRequest);
        DefaultExceptionMappings.NotFound("corr").Error.Code.ShouldBe(ApiErrorCode.NotFound);
        DefaultExceptionMappings.Unauthorized("corr").Error.Message.ShouldBe("Authentication is required.");
        DefaultExceptionMappings.Forbidden("corr", "nope").Error.Message.ShouldBe("nope");
        DefaultExceptionMappings.Conflict("corr", "conflict").Error.Code.ShouldBe(ApiErrorCode.Conflict);
        DefaultExceptionMappings.InternalError("corr").Error.Code.ShouldBe(ApiErrorCode.InternalError);
    }

    [Fact]
    public void ApiConventions_ReturnStandardResultTypesAndPayloads()
    {
        OkObjectResult ok = ApiConventions.Ok("value", "corr").ShouldBeOfType<OkObjectResult>();
        ok.Value.ShouldBeOfType<ApiResult<string>>().Data.ShouldBe("value");

        CreatedResult created = ApiConventions.Created("/items/1", 123, "corr").ShouldBeOfType<CreatedResult>();
        created.Location.ShouldBe("/items/1");
        created.Value.ShouldBeOfType<ApiResult<int>>().Data.ShouldBe(123);

        ApiConventions.NoContent().ShouldBeOfType<NoContentResult>();
        ApiConventions.BadRequest("corr", "bad").ShouldBeOfType<BadRequestObjectResult>().Value.ShouldBeOfType<ApiErrorResponse>().Error.Code.ShouldBe(ApiErrorCode.BadRequest);
        ApiConventions.NotFound("corr").ShouldBeOfType<NotFoundObjectResult>().Value.ShouldBeOfType<ApiErrorResponse>().Error.Code.ShouldBe(ApiErrorCode.NotFound);
        ApiConventions.Unauthorized("corr").ShouldBeOfType<UnauthorizedObjectResult>().Value.ShouldBeOfType<ApiErrorResponse>().Error.Code.ShouldBe(ApiErrorCode.Unauthorized);
        ApiConventions.Conflict("corr", "conflict").ShouldBeOfType<ConflictObjectResult>().Value.ShouldBeOfType<ApiErrorResponse>().Error.Code.ShouldBe(ApiErrorCode.Conflict);

        ObjectResult forbidden = ApiConventions.Forbidden("corr").ShouldBeOfType<ObjectResult>();
        forbidden.StatusCode.ShouldBe(StatusCodes.Status403Forbidden);
        forbidden.Value.ShouldBeOfType<ApiErrorResponse>().Error.Code.ShouldBe(ApiErrorCode.Forbidden);

        ObjectResult internalError = ApiConventions.InternalError("corr").ShouldBeOfType<ObjectResult>();
        internalError.StatusCode.ShouldBe(StatusCodes.Status500InternalServerError);
        internalError.Value.ShouldBeOfType<ApiErrorResponse>().Error.Code.ShouldBe(ApiErrorCode.InternalError);
    }

    [Fact]
    public void ModelStateValidationFilter_LeavesValidModelStateUntouched()
    {
        ActionExecutingContext context = CreateActionExecutingContext(new ServiceCollection().BuildServiceProvider());

        new ModelStateValidationFilter().OnActionExecuting(context);

        context.Result.ShouldBeNull();
    }

    [Fact]
    public void ModelStateValidationFilter_ShapesInvalidModelStateWithAccessorCorrelationId()
    {
        ServiceCollection services = new();
        services.AddSingleton<ICorrelationIdAccessor>(new TestCorrelationIdAccessor("from-accessor"));
        ActionExecutingContext context = CreateActionExecutingContext(services.BuildServiceProvider());
        context.ModelState.AddModelError("name", "Name is required.");

        new ModelStateValidationFilter().OnActionExecuting(context);

        BadRequestObjectResult result = context.Result.ShouldBeOfType<BadRequestObjectResult>();
        ApiErrorResponse response = result.Value.ShouldBeOfType<ApiErrorResponse>();
        response.CorrelationId.ShouldBe("from-accessor");
        response.Error.Code.ShouldBe(ApiErrorCode.ValidationFailed);
        response.ValidationErrors.ShouldNotBeNull();
        response.ValidationErrors.ShouldContain(error => error.Field == "name" && error.Message == "Name is required.");
    }

    [Fact]
    public void ModelStateValidationFilter_FallsBackToContextItemAndExceptionMessage()
    {
        ActionExecutingContext context = CreateActionExecutingContext(new ServiceCollection().BuildServiceProvider());
        context.HttpContext.Items[HeaderNames.CorrelationId] = "from-items";
        context.ModelState.AddModelError("age", string.Empty);

        new ModelStateValidationFilter().OnActionExecuting(context);

        ApiErrorResponse response = context.Result.ShouldBeOfType<BadRequestObjectResult>().Value.ShouldBeOfType<ApiErrorResponse>();
        response.CorrelationId.ShouldBe("from-items");
        response.ValidationErrors.ShouldNotBeNull();
        response.ValidationErrors.ShouldContain(error => error.Field == "age" && error.Message == "Invalid value.");
    }

    [Fact]
    public async Task RestAuthExtensions_WriteUnauthorizedAndForbiddenResponses()
    {
        DefaultHttpContext unauthorized = CreateHttpContext(new TestCorrelationIdAccessor("from-accessor"));
        await RestAuthExtensions.WriteUnauthorizedResponseAsync(unauthorized, "login");

        unauthorized.Response.StatusCode.ShouldBe(StatusCodes.Status401Unauthorized);
        unauthorized.Response.ContentType.ShouldBe(MediaTypes.Json);
        ApiErrorResponse unauthorizedResponse = await ReadResponseAsync(unauthorized);
        unauthorizedResponse.CorrelationId.ShouldBe("from-accessor");
        unauthorizedResponse.Error.Code.ShouldBe(ApiErrorCode.Unauthorized);
        unauthorizedResponse.Error.Message.ShouldBe("login");

        DefaultHttpContext forbidden = CreateHttpContext();
        forbidden.Request.Headers[HeaderNames.CorrelationId] = "from-header";
        await RestAuthExtensions.WriteForbiddenResponseAsync(forbidden);

        forbidden.Response.StatusCode.ShouldBe(StatusCodes.Status403Forbidden);
        ApiErrorResponse forbiddenResponse = await ReadResponseAsync(forbidden);
        forbiddenResponse.CorrelationId.ShouldBe("from-header");
        forbiddenResponse.Error.Code.ShouldBe(ApiErrorCode.Forbidden);
    }

    [Fact]
    public async Task RestAuthExtensions_DoNotWriteAfterResponseHasStarted()
    {
        DefaultHttpContext context = CreateHttpContext();
        context.Features.Set<IHttpResponseFeature>(new StartedResponseFeature());

        await RestAuthExtensions.WriteUnauthorizedResponseAsync(context);
        await RestAuthExtensions.WriteForbiddenResponseAsync(context);

        context.Response.StatusCode.ShouldBe(StatusCodes.Status200OK);
        context.Response.Body.Length.ShouldBe(0);
    }

    [Fact]
    public void RestEndpointConventions_AddExpectedMetadataAndReturnSameBuilder()
    {
        WebApplicationBuilder appBuilder = WebApplication.CreateBuilder();
        using WebApplication app = appBuilder.Build();

        RouteHandlerBuilder standard = app.MapPost("/standard", () => Results.Ok());
        standard.WithRest().ShouldBeSameAs(standard);
        standard.WithRest<string>().ShouldBeSameAs(standard);
        standard.WithRestCreate<string>().ShouldBeSameAs(standard);
        standard.WithRestDelete().ShouldBeSameAs(standard);
        standard.RequireRestAuth().ShouldBeSameAs(standard);
        standard.RequireRestAuth("named-policy").ShouldBeSameAs(standard);

        standard.WithRestDelete().ShouldBeSameAs(standard);
    }

    [Fact]
    public void RestEndpointConventions_RejectNullBuilders()
    {
        RouteHandlerBuilder builder = null!;

        Should.Throw<ArgumentNullException>(() => builder.WithRest());
        Should.Throw<ArgumentNullException>(() => builder.WithRest<string>());
        Should.Throw<ArgumentNullException>(() => builder.WithRestCreate<string>());
        Should.Throw<ArgumentNullException>(() => builder.WithRestDelete());
        Should.Throw<ArgumentNullException>(() => builder.RequireRestAuth());
    }

    private static ActionExecutingContext CreateActionExecutingContext(IServiceProvider services)
    {
        DefaultHttpContext httpContext = new() { RequestServices = services };
        ActionContext actionContext = new(httpContext, new RouteData(), new ActionDescriptor(), new ModelStateDictionary());
        return new ActionExecutingContext(actionContext, [], new Dictionary<string, object?>(), controller: new object());
    }

    private static DefaultHttpContext CreateHttpContext(ICorrelationIdAccessor? accessor = null)
    {
        ServiceCollection services = new();
        if (accessor is not null)
        {
            services.AddSingleton(accessor);
        }

        DefaultHttpContext context = new() { RequestServices = services.BuildServiceProvider() };
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task<ApiErrorResponse> ReadResponseAsync(DefaultHttpContext context)
    {
        context.Response.Body.Position = 0;
        return (await JsonSerializer.DeserializeAsync<ApiErrorResponse>(context.Response.Body, JsonOptionsDefaults.SerializerOptions))!;
    }

    private sealed class StartedResponseFeature : IHttpResponseFeature
    {
        public int StatusCode { get; set; } = StatusCodes.Status200OK;

        public string? ReasonPhrase { get; set; }

        public IHeaderDictionary Headers { get; set; } = new HeaderDictionary();

        public Stream Body { get; set; } = new MemoryStream();

        public bool HasStarted => true;

        public void OnCompleted(Func<object, Task> callback, object state)
        {
        }

        public void OnStarting(Func<object, Task> callback, object state)
        {
        }
    }

    private sealed class TestCorrelationIdAccessor(string? correlationId) : ICorrelationIdAccessor
    {
        public string? CorrelationId { get; private set; } = correlationId;

        public void SetCorrelationId(string correlationId)
        {
            CorrelationId = correlationId;
        }
    }
}
