using HoneyDrunk.Web.Rest.Abstractions.Constants;
using HoneyDrunk.Web.Rest.Abstractions.Errors;
using HoneyDrunk.Web.Rest.AspNetCore.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace HoneyDrunk.Web.Rest.AspNetCore.Mvc;

/// <summary>
/// An action filter that converts invalid model state to <see cref="ApiErrorResponse"/> with validation errors.
/// </summary>
public sealed class ModelStateValidationFilter : IActionFilter
{
    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ModelState.IsValid)
        {
            return;
        }

        ICorrelationIdAccessor? accessor = context.HttpContext.RequestServices.GetService<ICorrelationIdAccessor>();
        string correlationId = accessor?.CorrelationId
            ?? (context.HttpContext.Items.TryGetValue(HeaderNames.CorrelationId, out object? value) && value is string id ? id : Guid.NewGuid().ToString("N"));
        string? traceId = Activity.Current?.Id;

        List<ValidationError> validationErrors =
            context.ModelState
                .SelectMany(
                    entry => (entry.Value?.Errors ?? []).Select(
                        error => new ValidationError
                        {
                            Field = entry.Key,
                            Message = !string.IsNullOrWhiteSpace(error.ErrorMessage)
                                ? error.ErrorMessage
                                : error.Exception?.Message ?? "Invalid value.",
                        }))
                .ToList();

        ApiErrorResponse response = ApiErrorResponse.CreateValidationError(correlationId, validationErrors, traceId);

        context.Result = new BadRequestObjectResult(response);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context)
    {
        // No action needed after execution.
    }
}
