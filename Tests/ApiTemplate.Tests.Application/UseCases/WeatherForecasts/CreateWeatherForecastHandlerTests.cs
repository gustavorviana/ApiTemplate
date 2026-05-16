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
        var store = new List<WeatherForecastEntity>();
        var dbSet = store.BuildMockDbSet();
        dbSet
            .When(s => s.Add(Arg.Any<WeatherForecastEntity>()))
            .Do(ci => store.Add(ci.Arg<WeatherForecastEntity>()));

        var db = Substitute.For<IAppDbContext>();
        db.WeatherForecasts.Returns(dbSet);

        var factory = Substitute.For<IAppDbContextFactory>();
        factory.Create().Returns(db);

        var handler = new CreateWeatherForecastHandler(factory);
        var request = new CreateWeatherForecastRequest { TemperatureC = 10, Summary = "Cold" };

        var result = await handler.ExecuteAsync(request, CancellationToken.None);

        await db.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        Assert.Single(store);
        Assert.Equal(request.TemperatureC, store[0].TemperatureC);
        Assert.IsType<Result<CreateWeatherForecastResponse>>(result);
    }
}
