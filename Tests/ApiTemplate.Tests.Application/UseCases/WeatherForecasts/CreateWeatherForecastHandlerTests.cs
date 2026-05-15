using Viana.Results;
using ApiTemplate.Application.Core.Entities;
using ApiTemplate.Application.Interfaces;
using ApiTemplate.Application.UseCases.WeatherForecasts.Create;
using MockQueryable.NSubstitute;
using NSubstitute;

namespace ApiTemplate.Tests.Application.UseCases.WeatherForecasts;

public class CreateWeatherForecastHandlerTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldCreateWeatherForecast()
    {
        var store = new List<WeatherForecast>();
        var dbSet = store.BuildMockDbSet();
        dbSet
            .When(s => s.Add(Arg.Any<WeatherForecast>()))
            .Do(ci => store.Add(ci.Arg<WeatherForecast>()));

        var db = Substitute.For<IDbContext>();
        db.WeatherForecasts.Returns(dbSet);

        var handler = new CreateWeatherForecastHandler(db);
        var request = new CreateWeatherForecastRequest { TemperatureC = 10, Summary = "Cold" };

        var result = await handler.ExecuteAsync(request, CancellationToken.None);

        await db.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        Assert.Single(store);
        Assert.Equal(request.TemperatureC, store[0].TemperatureC);
        Assert.IsType<Result<CreateWeatherForecastResponse>>(result);
    }
}
