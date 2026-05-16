using System.Net;
using ApiTemplate.IntegrationTests.Fixtures;

namespace ApiTemplate.IntegrationTests.Endpoints;

public sealed class WeatherForecastEndpointTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public WeatherForecastEndpointTests(ApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Get_WeatherForecast_ReturnsOk()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/WeatherForecast");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
