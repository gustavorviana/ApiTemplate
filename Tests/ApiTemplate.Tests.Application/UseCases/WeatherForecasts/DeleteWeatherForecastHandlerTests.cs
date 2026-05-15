using Viana.Results;
using ApiTemplate.Application.Core.Entities;
using ApiTemplate.Application.Interfaces;
using ApiTemplate.Application.UseCases.WeatherForecasts.Delete;
using MockQueryable.NSubstitute;
using NSubstitute;

namespace ApiTemplate.Tests.Application.UseCases.WeatherForecasts;

public class DeleteWeatherForecastHandlerTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldReturnNotFound_WhenEntityDoesNotExist()
    {
        var dbSet = new List<WeatherForecast>().BuildMockDbSet();
        var db = Substitute.For<IDbContext>();
        db.WeatherForecasts.Returns(dbSet);

        var handler = new DeleteWeatherForecastHandler(db);
        var result = await handler.ExecuteAsync(new DeleteWeatherForecastRequest { Id = 1 }, CancellationToken.None);

        await db.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        Assert.IsType<Result>(result);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldDeleteEntity_WhenItExists()
    {
        var entity = WeatherForecast.Create(DateOnly.FromDateTime(DateTime.UtcNow), 10, "Cold");
        var dbSet = new List<WeatherForecast> { entity }.BuildMockDbSet();
        var db = Substitute.For<IDbContext>();
        db.WeatherForecasts.Returns(dbSet);

        var handler = new DeleteWeatherForecastHandler(db);
        var result = await handler.ExecuteAsync(new DeleteWeatherForecastRequest { Id = entity.Id }, CancellationToken.None);

        dbSet.Received(1).Remove(entity);
        await db.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        Assert.IsType<Result>(result);
    }
}
