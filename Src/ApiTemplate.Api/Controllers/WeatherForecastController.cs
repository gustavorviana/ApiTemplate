using ApiTemplate.Application.UseCases.WeatherForecasts.Create;
using ApiTemplate.Application.UseCases.WeatherForecasts.Delete;
using ApiTemplate.Application.UseCases.WeatherForecasts.GetAll;
#if EnableRateLimiting
using ApiTemplate.Api.Extensions;
using Microsoft.AspNetCore.RateLimiting;
#endif
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Viana.Results;

namespace ApiTemplate.Api.Controllers;

[ApiController]
[Route("[controller]")]
#if (UseUseCase)
public class WeatherForecastController(
    GetAllWeatherForecastHandler getAllHandler,
    CreateWeatherForecastHandler createHandler,
    DeleteWeatherForecastHandler deleteHandler) : ControllerBase
#else
public class WeatherForecastController : ControllerBase
#endif
{
    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<ListResult<GetAllWeatherForecastResponse>> Get(
#if (UseCqrs)
        [FromServices] GetAllWeatherForecastHandler handler,
#endif
        CancellationToken token)
#if (UseCqrs)
        => await handler.ExecuteAsync(new GetAllWeatherForecastRequest(), token);
#elif (UseUseCase)
        => await getAllHandler.ExecuteAsync(new GetAllWeatherForecastRequest(), token);
#endif

#if (EnableJwt)
    [Authorize]
#if EnableRateLimiting
    [EnableRateLimiting(RateLimitingExtensions.AuthenticatedPolicyName)]
#endif
#endif
    [HttpPost(Name = "CreateWeatherForecast")]
    public async Task<IResult<CreateWeatherForecastResponse>> Create(
#if (UseCqrs)
        [FromServices] CreateWeatherForecastHandler handler,
#endif
        [FromBody] CreateWeatherForecastRequest request,
        CancellationToken token)
#if (UseCqrs)
        => await handler.ExecuteAsync(request, token);
#elif (UseUseCase)
        => await createHandler.ExecuteAsync(request, token);
#endif

#if (EnableJwt)
    [Authorize]
#if EnableRateLimiting
    [EnableRateLimiting(RateLimitingExtensions.AuthenticatedPolicyName)]
#endif
#endif
    [HttpDelete("{id:int}", Name = "DeleteWeatherForecast")]
    public async Task<Result> Delete(
#if (UseCqrs)
        [FromServices] DeleteWeatherForecastHandler handler,
#endif
        [FromRoute] int id,
        CancellationToken token)
#if (UseCqrs)
        => await handler.ExecuteAsync(new DeleteWeatherForecastRequest { Id = id }, token);
#elif (UseUseCase)
        => await deleteHandler.ExecuteAsync(new DeleteWeatherForecastRequest { Id = id }, token);
#endif
}
