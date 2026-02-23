using Viana.Results;
using ApiTemplate.Application.Core.Entities;
using ApiTemplate.Application.Interfaces;
using ApiTemplate.Application.UseCases.WeatherForecasts.Create;
using NSubstitute;

namespace ApiTemplate.Tests.Application.UseCases.WeatherForecasts;

public class CreateWeatherForecastHandleTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldCreateWeatherForecast()
    {
        var repository = Substitute.For<IWeatherForecastRepository>();

        WeatherForecast? addedEntity = null;
        repository
            .AddAsync(Arg.Do<WeatherForecast>(e => addedEntity = e), Arg.Any<CancellationToken>())
            .Returns(callInfo => addedEntity!);

        var handle = new CreateWeatherForecastHandle(repository);

        var request = new CreateWeatherForecastRequest
        {
            TemperatureC = 10,
            Summary = "Cold"
        };

        var expectedDate = DateOnly.FromDateTime(DateTime.UtcNow);

        var result = await handle.ExecuteAsync(request, CancellationToken.None);

        await repository
            .Received(1)
            .AddAsync(Arg.Any<WeatherForecast>(), Arg.Any<CancellationToken>());

        Assert.NotNull(addedEntity);
        Assert.Equal(request.TemperatureC, addedEntity!.TemperatureC);
        Assert.Equal(request.Summary, addedEntity.Summary);
        Assert.Equal(expectedDate, addedEntity.Date);

        var response = Assert.IsType<Result<CreateWeatherForecastResponse>>(result).Data!;
        Assert.Equal(addedEntity.Id, response.Id);
        Assert.Equal(addedEntity.Date, response.Date);
        Assert.Equal(addedEntity.TemperatureC, response.TemperatureC);
        Assert.Equal(addedEntity.Summary, response.Summary);
    }
}
