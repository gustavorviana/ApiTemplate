using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Viana.Results;

namespace ApiTemplate.Api.Filters;

/// <summary>
/// Catches exceptions that escape the MVC pipeline (or other middleware) and
/// converts them into a ProblemDetails/ProblemResult response so the client
/// always sees a consistent error envelope.
/// </summary>
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled exception while processing {Method} {Path}",
            httpContext.Request.Method, httpContext.Request.Path);

        var problem = new ProblemResult(
            StatusCodes.Status500InternalServerError,
            "An unexpected error occurred.",
            "about:blank");

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = "application/problem+json";
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);

        return true;
    }
}
