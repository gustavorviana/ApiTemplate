using Viana.Results;
using ApiTemplate.Application.Core.Entities;
using ApiTemplate.Application.UseCases.WeatherForecasts.Delete;
using ApiTemplate.Tests.Application.Base;
using NSubstitute;

namespace ApiTemplate.Tests.Application.UseCases.WeatherForecasts;

public class DeleteWeatherForecastHandlerTests : TestBase
{
    private readonly DeleteWeatherForecastHandler _handler;

    public DeleteWeatherForecastHandlerTests()
    {
        _handler = new DeleteWeatherForecastHandler(AppDbContextFactory);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnNotFound_WhenEntityDoesNotExist()
    {
        var dbSet = ToMockDbSet(Array.Empty<WeatherForecastEntity>());
        AppContext.WeatherForecasts.Returns(dbSet);

        var result = await _handler.ExecuteAsync(
            new DeleteWeatherForecastRequest { Id = Guid.NewGuid() },
            CancellationToken.None);

        await AppContext.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        Assert.IsType<Result>(result);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldDeleteEntity_WhenItExists()
    {
        var entity = new WeatherForecastEntity
        {
            Id = Guid.NewGuid(),
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            TemperatureC = 10,
            Summary = "Cold"
        };
        var dbSet = ToMockDbSet(new[] { entity });
        AppContext.WeatherForecasts.Returns(dbSet);

        var result = await _handler.ExecuteAsync(
            new DeleteWeatherForecastRequest { Id = entity.Id },
            CancellationToken.None);

        dbSet.Received(1).Remove(entity);
        await AppContext.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        Assert.IsType<Result>(result);
    }
}
