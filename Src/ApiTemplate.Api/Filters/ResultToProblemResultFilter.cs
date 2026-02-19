using ApiTemplate.Application.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ApiTemplate.Api.Filters;

/// <summary>
/// Intercepts action results that implement <see cref="IResult"/>.
/// When <see cref="IResult.Problem"/> is not null, returns the problem details with the error status.
/// When status is "no content" (e.g. 204), returns that status with no response body.
/// </summary>
public sealed class ResultToProblemResultFilter : IResultFilter
{
	public void OnResultExecuting(ResultExecutingContext context)
	{
		if (context.Result is not ObjectResult objectResult || objectResult.Value is not Application.Results.IResult result)
			return;

		if (result.Problem is not null)
		{
			context.Result = new ObjectResult(result.Problem) { StatusCode = result.Status };
			return;
		}

		// Result (non-generic) = only status, no body
		if (objectResult.Value.GetType() == typeof(Result))
		{
			context.Result = new StatusCodeResult(result.Status);
			return;
		}

		if (result.Status == 204)
			context.Result = new StatusCodeResult(result.Status);
	}

	public void OnResultExecuted(ResultExecutedContext context) { }
}
