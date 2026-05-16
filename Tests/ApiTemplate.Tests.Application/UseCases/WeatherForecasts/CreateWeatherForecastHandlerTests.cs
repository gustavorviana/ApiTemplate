using Viana.Results;
using ApiTemplate.Application.Core.Entities;
using ApiTemplate.Application.UseCases.WeatherForecasts.Create;
using ApiTemplate.Tests.Application.Base;
#if (EnableHangfire)
using ExecutionFlow.Abstractions;
#endif
using NSubstitute;

namespace ApiTemplate.Tests.Application.UseCases.WeatherForecasts;

public class CreateWeatherForecastHandlerTests : TestBase
{
#if (EnableHangfire)
    private readonly IEventDispatcher _dispatcher = Substitute.For<IEventDispatcher>();
#endif
    private readonly CreateWeatherForecastHandler _handler;

    public CreateWeatherForecastHandlerTests()
    {
        _handler = new CreateWeatherForecastHandler(
            AppDbContextFactory
#if (EnableHangfire)
            , _dispatcher
#endif
        );
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCreateWeatherForecast()
    {
        var store = new List<WeatherForecastEntity>();
        var dbSet = ToMockDbSet(store);
        dbSet
            .When(s => s.Add(Arg.Any<WeatherForecastEntity>()))
            .Do(ci => store.Add(ci.Arg<WeatherForecastEntity>()));
        AppContext.WeatherForecasts.Returns(dbSet);

        var request = new CreateWeatherForecastRequest { TemperatureC = 10, Summary = "Cold" };
        var result = await _handler.ExecuteAsync(request, CancellationToken.None);

        await AppContext.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        Assert.Single(store);
        Assert.Equal(request.TemperatureC, store[0].TemperatureC);
        Assert.IsType<Result<CreateWeatherForecastResponse>>(result);
    }
}
