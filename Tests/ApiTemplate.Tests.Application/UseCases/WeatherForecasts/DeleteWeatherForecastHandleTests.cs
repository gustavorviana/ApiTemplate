using Viana.Results;
using ApiTemplate.Application.Core.Entities;
using ApiTemplate.Application.Interfaces;
using ApiTemplate.Application.UseCases.WeatherForecasts.Delete;
using NSubstitute;

namespace ApiTemplate.Tests.Application.UseCases.WeatherForecasts;

public class DeleteWeatherForecastHandleTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldReturnNotFound_WhenEntityDoesNotExist()
    {
        var repository = Substitute.For<IWeatherForecastRepository>();
        repository
            .GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((WeatherForecast?)null);

        var handle = new DeleteWeatherForecastHandle(repository);

        var request = new DeleteWeatherForecastRequest { Id = 1 };

        var result = await handle.ExecuteAsync(request, CancellationToken.None);

        await repository
            .DidNotReceive()
            .DeleteAsync(Arg.Any<WeatherForecast>(), Arg.Any<CancellationToken>());

        Assert.IsType<Result>(result);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldDeleteEntity_WhenItExists()
    {
        var entity = WeatherForecast.Create(
            DateOnly.FromDateTime(DateTime.UtcNow),
            10,
            "Cold");

        var repository = Substitute.For<IWeatherForecastRepository>();
        repository
            .GetByIdAsync(entity.Id, Arg.Any<CancellationToken>())
            .Returns(entity);

        var handle = new DeleteWeatherForecastHandle(repository);

        var request = new DeleteWeatherForecastRequest { Id = entity.Id };

        var result = await handle.ExecuteAsync(request, CancellationToken.None);

        await repository
            .Received(1)
            .DeleteAsync(entity, Arg.Any<CancellationToken>());

        Assert.IsType<Result>(result);
    }
}
