using ApiTemplate.Application.Results;
using ApiTemplate.Api.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ApiTemplate.Tests.Filters;

public class ResultToProblemResultFilterTests
{
	private static ResultExecutingContext CreateContext(IActionResult result)
	{
		var httpContext = new DefaultHttpContext();
		var actionContext = new ActionContext(
			httpContext,
			new RouteData(),
			new ActionDescriptor(),
			new ModelStateDictionary());
		return new ResultExecutingContext(
			actionContext,
			new List<IFilterMetadata>(),
			result,
			controller: null!);
	}

	[Fact]
	public void OnResultExecuting_WhenProblemIsNotNull_ReplacesResultWithProblemAndStatusCode()
	{
		var problem = new ProblemResult(404, "Not Found");
		Result<string> result = problem;
		var objectResult = new ObjectResult(result);
		var context = CreateContext(objectResult);
		var filter = new ResultToProblemResultFilter();

		filter.OnResultExecuting(context);

		var replaced = Assert.IsType<ObjectResult>(context.Result);
		Assert.Same(problem, replaced.Value);
		Assert.Equal(404, replaced.StatusCode);
	}

	[Fact]
	public void OnResultExecuting_WhenResultIsNonGenericResult_ReplacesWithStatusCodeResultNoBody()
	{
		var result = new Result(204);
		var objectResult = new ObjectResult(result);
		var context = CreateContext(objectResult);
		var filter = new ResultToProblemResultFilter();

		filter.OnResultExecuting(context);

		var statusResult = Assert.IsType<StatusCodeResult>(context.Result);
		Assert.Equal(204, statusResult.StatusCode);
	}

	[Fact]
	public void OnResultExecuting_WhenResultTWithSuccess_DoesNotReplaceResult()
	{
		Result<string> result = "ok";
		var objectResult = new ObjectResult(result);
		var context = CreateContext(objectResult);
		var filter = new ResultToProblemResultFilter();

		filter.OnResultExecuting(context);

		Assert.Same(objectResult, context.Result);
		Assert.Same(result, ((ObjectResult)context.Result).Value);
	}

	[Fact]
	public void OnResultExecuting_WhenResultTStatus204_ReplacesWithStatusCodeResultNoBody()
	{
		var result = new Result<string>(204, null);
		var objectResult = new ObjectResult(result);
		var context = CreateContext(objectResult);
		var filter = new ResultToProblemResultFilter();

		filter.OnResultExecuting(context);

		var statusResult = Assert.IsType<StatusCodeResult>(context.Result);
		Assert.Equal(204, statusResult.StatusCode);
	}

	[Fact]
	public void OnResultExecuting_WhenResultIsNotObjectResult_DoesNothing()
	{
		var statusResult = new StatusCodeResult(200);
		var context = CreateContext(statusResult);
		var filter = new ResultToProblemResultFilter();

		filter.OnResultExecuting(context);

		Assert.Same(statusResult, context.Result);
	}

	[Fact]
	public void OnResultExecuting_WhenValueIsNotIResult_DoesNothing()
	{
		var objectResult = new ObjectResult("plain string");
		var context = CreateContext(objectResult);
		var filter = new ResultToProblemResultFilter();

		filter.OnResultExecuting(context);

		Assert.Same(objectResult, context.Result);
	}
}