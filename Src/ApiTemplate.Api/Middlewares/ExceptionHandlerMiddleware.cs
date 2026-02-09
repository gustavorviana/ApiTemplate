using ApiTemplate.Application.Core.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ApiTemplate.Api.Middlewares
{
    public class ExceptionHandlerMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (DomainException ex)
            {
                await HandleDomainExceptionAsync(context, ex);
            }
            catch (Exception ex)
            {
                await HandleUnknownExceptionAsync(context, ex);
            }
        }

        private static async Task HandleDomainExceptionAsync(HttpContext context, DomainException exception)
        {
            context.Response.StatusCode = exception.StatusCode;

            var problemDetails = new ProblemDetails
            {
                Detail = exception.Message
            };

            await context.Response.WriteAsJsonAsync(problemDetails);
        }

        private static async Task HandleUnknownExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            var problemDetails = new ProblemDetails
            {
                Detail = "The server was unable to complete your request. Please try again later",
                Instance = context.Request.Path
            };

            await context.Response.WriteAsJsonAsync(problemDetails);
        }
    }
}
