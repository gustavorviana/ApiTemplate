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
        var list = new List<WeatherForecastEntity>
        {
            new() { Id = Guid.NewGuid(), Date = DateOnly.FromDateTime(DateTime.UtcNow), TemperatureC = 10, Summary = "Cold" },
            new() { Id = Guid.NewGuid(), Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)), TemperatureC = 20, Summary = "Warm" }
        };

        var dbSet = list.BuildMockDbSet();
        var db = Substitute.For<IAppDbContext>();
        db.WeatherForecasts.Returns(dbSet);

        var factory = Substitute.For<IAppDbContextFactory>();
        factory.Create().Returns(db);

        var handler = new GetAllWeatherForecastHandler(factory);
        var result = await handler.ExecuteAsync(new GetAllWeatherForecastRequest(), CancellationToken.None);

        var responses = Assert.IsType<ListResult<GetAllWeatherForecastResponse>>(result).Data;
        Assert.Equal(list.Count, responses.Count);
    }
}
