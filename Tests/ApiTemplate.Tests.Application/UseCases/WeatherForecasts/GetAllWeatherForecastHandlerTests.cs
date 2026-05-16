using Viana.Results;
using ApiTemplate.Application.Core.Entities;
using ApiTemplate.Application.UseCases.WeatherForecasts.GetAll;
using ApiTemplate.Tests.Application.Base;
using NSubstitute;

namespace ApiTemplate.Tests.Application.UseCases.WeatherForecasts;

public class GetAllWeatherForecastHandlerTests : TestBase
{
    private readonly GetAllWeatherForecastHandler _handler;

    public GetAllWeatherForecastHandlerTests()
    {
        _handler = new GetAllWeatherForecastHandler(AppDbContextFactory);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnAllForecasts()
    {
        var list = new List<WeatherForecastEntity>
        {
            new() { Id = Guid.NewGuid(), Date = DateOnly.FromDateTime(DateTime.UtcNow), TemperatureC = 10, Summary = "Cold" },
            new() { Id = Guid.NewGuid(), Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)), TemperatureC = 20, Summary = "Warm" }
        };
        var dbSet = ToMockDbSet(list);
        AppContext.WeatherForecasts.Returns(dbSet);

        var result = await _handler.ExecuteAsync(new GetAllWeatherForecastRequest(), CancellationToken.None);

        var responses = Assert.IsType<ListResult<GetAllWeatherForecastResponse>>(result).Data;
        Assert.Equal(list.Count, responses.Count);
    }
}
