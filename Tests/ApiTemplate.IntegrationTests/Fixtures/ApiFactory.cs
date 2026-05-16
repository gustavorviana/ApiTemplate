using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
#if (UseSqlServer)
using Testcontainers.MsSql;
#elif (UsePostgres)
using Testcontainers.PostgreSql;
#elif (UseMySQL)
using Testcontainers.MySql;
#endif

namespace ApiTemplate.IntegrationTests.Fixtures;

public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
#if (UseSqlServer)
    private readonly MsSqlContainer _db = new MsSqlBuilder().Build();
#elif (UsePostgres)
    private readonly PostgreSqlContainer _db = new PostgreSqlBuilder().Build();
#elif (UseMySQL)
    private readonly MySqlContainer _db = new MySqlBuilder().Build();
#endif

    public HttpClient CreateAuthenticatedClient(string? bearerToken = null)
    {
        var client = CreateClient();
        if (!string.IsNullOrEmpty(bearerToken))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        return client;
    }

    public async Task InitializeAsync()
    {
        await _db.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _db.DisposeAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(Environments.Development);

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _db.GetConnectionString()
            });
        });
    }
}
