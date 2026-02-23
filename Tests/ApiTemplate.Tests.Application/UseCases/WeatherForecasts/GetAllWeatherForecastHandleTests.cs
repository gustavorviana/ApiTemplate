using Viana.Results;
using ApiTemplate.Application.Core.Entities;
using ApiTemplate.Application.Interfaces;
using ApiTemplate.Application.UseCases.WeatherForecasts.GetAll;
using NSubstitute;

namespace ApiTemplate.Tests.Application.UseCases.WeatherForecasts;

public class GetAllWeatherForecastHandleTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldReturnAllForecasts()
    {
        var list = new List<WeatherForecast>
        {
            WeatherForecast.Create(DateOnly.FromDateTime(DateTime.UtcNow), 10, "Cold"),
            WeatherForecast.Create(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)), 20, "Warm")
        };

        var repository = Substitute.For<IWeatherForecastRepository>();
        repository
            .GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(list);

        var handle = new GetAllWeatherForecastHandle(repository);

        var result = await handle.ExecuteAsync(new GetAllWeatherForecastRequest(), CancellationToken.None);

        await repository
            .Received(1)
            .GetAllAsync(Arg.Any<CancellationToken>());

        var responses = Assert.IsType<ListResult<GetAllWeatherForecastResponse>>(result).Data;
        Assert.Equal(list.Count, responses.Count);
        for (var i = 0; i < list.Count; i++)
        {
            Assert.Equal(list[i].Date, responses[i].Date);
            Assert.Equal(list[i].TemperatureC, responses[i].TemperatureC);
            Assert.Equal(list[i].Summary, responses[i].Summary);
        }

    }
}
