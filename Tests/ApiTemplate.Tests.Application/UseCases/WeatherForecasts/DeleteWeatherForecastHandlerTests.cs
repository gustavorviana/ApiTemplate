using Viana.Results;
using ApiTemplate.Application.Core.Entities;
using ApiTemplate.Application.Interfaces;
using ApiTemplate.Application.UseCases.WeatherForecasts.Delete;
using MockQueryable.NSubstitute;
using NSubstitute;

namespace ApiTemplate.Tests.Application.UseCases.WeatherForecasts;

public class DeleteWeatherForecastHandlerTests
{
    private static (IAppDbContextFactory factory, IAppDbContext db, Microsoft.EntityFrameworkCore.DbSet<WeatherForecastEntity> dbSet) NewFactory(IEnumerable<WeatherForecastEntity>? entities = null)
    {
        var dbSet = (entities ?? new List<WeatherForecastEntity>()).ToList().BuildMockDbSet();
        var db = Substitute.For<IAppDbContext>();
        db.WeatherForecasts.Returns(dbSet);

        var factory = Substitute.For<IAppDbContextFactory>();
        factory.Create().Returns(db);

        return (factory, db, dbSet);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnNotFound_WhenEntityDoesNotExist()
    {
        var (factory, db, _) = NewFactory();

        var handler = new DeleteWeatherForecastHandler(factory);
        var result = await handler.ExecuteAsync(new DeleteWeatherForecastRequest { Id = Guid.NewGuid() }, CancellationToken.None);

        await db.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
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
        var (factory, db, dbSet) = NewFactory(new[] { entity });

        var handler = new DeleteWeatherForecastHandler(factory);
        var result = await handler.ExecuteAsync(new DeleteWeatherForecastRequest { Id = entity.Id }, CancellationToken.None);

        dbSet.Received(1).Remove(entity);
        await db.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        Assert.IsType<Result>(result);
    }
}
