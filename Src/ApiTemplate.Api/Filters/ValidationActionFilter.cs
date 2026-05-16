using ApiTemplate.Application.Resources;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Viana.Results;

namespace ApiTemplate.Api.Filters;

/// <summary>
/// Resolves an IValidator&lt;T&gt; via DI for each action argument and short-circuits
/// the pipeline with a ProblemResult (400) when validation fails. Override
/// <see cref="BuildErrorResult"/> to customize the response shape.
/// </summary>
public class ValidationActionFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var services = context.HttpContext.RequestServices;

        foreach (var value in context.ActionArguments.Values)
        {
            if (value is null) continue;

            var (validator, validationContext) = ResolveValidator(services, value);
            if (validator is null) continue;

            var result = await validator.ValidateAsync(validationContext, context.HttpContext.RequestAborted);
            if (result.IsValid) continue;

            context.Result = BuildErrorResult(result);
            return;
        }

        await next();
    }

    private static (IValidator? validator, IValidationContext? context) ResolveValidator(IServiceProvider services, object value)
    {
        var valueType = value.GetType();
        var validatorType = typeof(IValidator<>).MakeGenericType(valueType);

        if (services.GetService(validatorType) is not IValidator validator)
            return (null, null);

        var validationContext = (IValidationContext)Activator.CreateInstance(
            typeof(ValidationContext<>).MakeGenericType(valueType),
            value)!;

        return (validator, validationContext);
    }

    /// <summary>
    /// Maps a failed <see cref="ValidationResult"/> into the IActionResult that
    /// will be returned to the client. Override to change status code, message
    /// or shape of the error payload.
    /// </summary>
    protected virtual IActionResult BuildErrorResult(ValidationResult result)
    {
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

        var problem = new ProblemResult(
            400,
            Messages.Validation.OneOrMoreErrorsOccurred,
            "about:blank",
            extensions);

        return new ObjectResult(problem) { StatusCode = 400 };
    }
}
