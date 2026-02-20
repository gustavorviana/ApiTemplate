using ApiTemplate.Application.Results;
using ApiTemplate.Application.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ApiTemplate.Api.Filters;

/// <summary>
/// Intercepts actions whose arguments implement <see cref="IValidatableRequest"/>.
/// Calls <see cref="IValidatableRequest.Validate"/> on each such argument and returns
/// a ProblemResult (400) when validation fails.
/// </summary>
public sealed class ValidationActionFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        foreach (var value in context.ActionArguments.Values)
        {
            if (value is not IValidatableRequest validatable)
                continue;

            var result = validatable.Validate();
            if (result.IsValid)
                continue;

            var errors = result.Errors
                .Select(e => new Dictionary<string, object?>(StringComparer.Ordinal)
                {
                    ["propertyName"] = e.PropertyName,
                    ["errorMessage"] = e.ErrorMessage
                })
                .ToList<object?>();

            var extensions = new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["errors"] = errors
            };

            var problemResult = new ProblemResult(
                400,
                "One or more validation errors occurred",
                "about:blank",
                extensions);

            context.Result = new ObjectResult(problemResult) { StatusCode = 400 };
            return;
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}
