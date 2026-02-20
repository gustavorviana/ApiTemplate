using ApiTemplate.Application.UseCases.WeatherForecasts.Create;
using ApiTemplate.Application.UseCases.WeatherForecasts.GetAll;
using Microsoft.AspNetCore.Mvc;
#if (EnableResult)
using IResult = ApiTemplate.Application.Results.IResult;
#endif

namespace ApiTemplate.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    [HttpGet(Name = "GetWeatherForecast")]
#if (EnableResult)
    public async Task<IResult> Get(
#else
    public async Task<List<GetAllWeatherForecastResponse>> Get(
#endif
        [FromServices] GetAllWeatherForecastHandle handle,
        CancellationToken token)
        => await handle.ExecuteAsync(new GetAllWeatherForecastRequest(), token);

    [HttpPost(Name = "CreateWeatherForecast")]
#if (EnableResult)
    public async Task<IResult> Create(
#else
    public async Task<CreateWeatherForecastResponse> Create(
#endif
        [FromServices] CreateWeatherForecastHandle handle,
        [FromBody] CreateWeatherForecastRequest request,
        CancellationToken token)
        => await handle.ExecuteAsync(request, token);
}
