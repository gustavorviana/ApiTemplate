using Viana.Results;
using ApiTemplate.Application.Core.Entities;
using ApiTemplate.Application.Interfaces;
using ApiTemplate.Application.UseCases.WeatherForecasts.GetAll;
using MockQueryable.NSubstitute;
using NSubstitute;

namespace ApiTemplate.Tests.Application.UseCases.WeatherForecasts;

public class GetAllWeatherForecastHandlerTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldReturnAllForecasts()
    {
        var list = new List<WeatherForecast>
        {
            WeatherForecast.Create(DateOnly.FromDateTime(DateTime.UtcNow), 10, "Cold"),
            WeatherForecast.Create(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)), 20, "Warm")
        };

        var dbSet = list.BuildMockDbSet();
        var db = Substitute.For<IDbContext>();
        db.WeatherForecasts.Returns(dbSet);

        var handler = new GetAllWeatherForecastHandler(db);
        var result = await handler.ExecuteAsync(new GetAllWeatherForecastRequest(), CancellationToken.None);

        var responses = Assert.IsType<ListResult<GetAllWeatherForecastResponse>>(result).Data;
        Assert.Equal(list.Count, responses.Count);
    }
}
