using ApiTemplate.Application.Results;
using ApiTemplate.Application.Validation;
using ApiTemplate.Api.Filters;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;

namespace ApiTemplate.Tests.Filters;

public class ValidationActionFilterTests
{
	private static ActionExecutingContext CreateContext(IDictionary<string, object?> actionArguments, object? controller = null)
	{
		var httpContext = new DefaultHttpContext();
		var actionContext = new ActionContext(
			httpContext,
			new RouteData(),
			new ActionDescriptor(),
			new ModelStateDictionary());
		return new ActionExecutingContext(
			actionContext,
			new List<IFilterMetadata>(),
			actionArguments,
			controller!);
	}

	private sealed class ValidatableStub : IValidatableRequest
	{
		private readonly ValidationResult _result;

		public ValidatableStub(ValidationResult result) => _result = result;

		public ValidationResult Validate() => _result;
	}

	[Fact]
	public void OnActionExecuting_WhenNoArgumentImplementsIValidatableRequest_DoesNotSetResult()
	{
		var actionArguments = new Dictionary<string, object?>
		{
			["request"] = "plain string"
		};
		var context = CreateContext(actionArguments);
		var filter = new ValidationActionFilter();

		filter.OnActionExecuting(context);

		Assert.Null(context.Result);
	}

	[Fact]
	public void OnActionExecuting_WhenValidatableRequestIsValid_DoesNotSetResult()
	{
		var validatable = new ValidatableStub(new ValidationResult());
		var actionArguments = new Dictionary<string, object?>
		{
			["request"] = validatable
		};
		var context = CreateContext(actionArguments);
		var filter = new ValidationActionFilter();

		filter.OnActionExecuting(context);

		Assert.Null(context.Result);
	}

	[Fact]
	public void OnActionExecuting_WhenValidatableRequestIsInvalid_SetsObjectResultWithStatusCode400()
	{
		var errors = new[]
		{
			new ValidationFailure("TemperatureC", "Must be between -100 and 100.")
		};
		var validatable = new ValidatableStub(new ValidationResult(errors));
		var actionArguments = new Dictionary<string, object?>
		{
			["request"] = validatable
		};
		var context = CreateContext(actionArguments);
		var filter = new ValidationActionFilter();

		filter.OnActionExecuting(context);

		var objectResult = Assert.IsType<ObjectResult>(context.Result);
		Assert.Equal(400, objectResult.StatusCode);
		var problemResult = Assert.IsType<ProblemResult>(objectResult.Value);
		Assert.Equal(400, problemResult.Status);
		Assert.Equal("One or more validation errors occurred", problemResult.Title);
		Assert.Equal("about:blank", problemResult.Type);
	}

	[Fact]
	public void OnActionExecuting_WhenInvalid_ProblemResultExtensionsContainErrorsWithPropertyNameAndErrorMessage()
	{
		var errors = new[]
		{
			new ValidationFailure("TemperatureC", "Must be between -100 and 100."),
			new ValidationFailure("Summary", "Must not exceed 500 characters.")
		};
		var validatable = new ValidatableStub(new ValidationResult(errors));
		var actionArguments = new Dictionary<string, object?>
		{
			["request"] = validatable
		};
		var context = CreateContext(actionArguments);
		var filter = new ValidationActionFilter();

		filter.OnActionExecuting(context);

		var problemResult = Assert.IsType<ProblemResult>(((ObjectResult)context.Result!).Value);
		Assert.True(problemResult.Extensions.ContainsKey("errors"));
		var errorsList = Assert.IsAssignableFrom<IEnumerable<object>>(problemResult.Extensions["errors"]!);
		var errorsArray = errorsList.Cast<Dictionary<string, object?>>().ToList();
		Assert.Equal(2, errorsArray.Count);
		Assert.Equal("TemperatureC", errorsArray[0]["propertyName"]);
		Assert.Equal("Must be between -100 and 100.", errorsArray[0]["errorMessage"]);
		Assert.Equal("Summary", errorsArray[1]["propertyName"]);
		Assert.Equal("Must not exceed 500 characters.", errorsArray[1]["errorMessage"]);
	}

	[Fact]
	public void OnActionExecuting_WhenMultipleArguments_ValidatesFirstValidatableAndShortCircuitsOnInvalid()
	{
		var validatable = new ValidatableStub(new ValidationResult(new[]
		{
			new ValidationFailure("A", "Error A")
		}));
		var actionArguments = new Dictionary<string, object?>
		{
			["other"] = 42,
			["request"] = validatable
		};
		var context = CreateContext(actionArguments);
		var filter = new ValidationActionFilter();

		filter.OnActionExecuting(context);

		var objectResult = Assert.IsType<ObjectResult>(context.Result);
		Assert.Equal(400, objectResult.StatusCode);
	}
}
