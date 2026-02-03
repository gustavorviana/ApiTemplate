using ApiTemplate.Application.UseCases.WeatherForecasts.GetAll;
using Microsoft.AspNetCore.Mvc;

namespace ApiTemplate.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    [HttpGet(Name = "GetWeatherForecast")]
    public Task<GetAllWeatherForecastResponse[]> Get([FromServices] GetAllWeatherForecastHandle handle, CancellationToken token)
        => handle.ExecuteAsync(new GetAllWeatherForecastRequest(), token);
}