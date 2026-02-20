using ApiTemplate.Application.DependencyInjection;
#if (UseDatabase)
using Microsoft.Extensions.Configuration;
using ApiTemplate.Application.Interfaces;
using ApiTemplate.Infrastructure.Data;
using ApiTemplate.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
#endif
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ApiTemplate.Infrastructure.DependencyInjection;

// Adds common Aspire services: service discovery, resilience, health checks, and OpenTelemetry.
// This project should be referenced by each service project in your solution.
// To learn more about using this project, see https://aka.ms/dotnet/aspire/service-defaults
public static class ApiServiceCollectionExtensions
{
    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddApplication();

        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            // Turn on resilience by default
            http.AddStandardResilienceHandler();

            // Turn on service discovery by default
            http.AddServiceDiscovery();
        });

        // Uncomment the following to restrict the allowed schemes for service discovery.
        // builder.Services.Configure<ServiceDiscoveryOptions>(options =>
        // {
        //     options.AllowedSchemes = ["https"];
        // });

#if (UseDatabase)
        builder.Services.AddDatabase(builder.Configuration.GetConnectionString("DefaultConnection")!);
#endif

        return builder;
    }

#if (UseDatabase)
    private static IServiceCollection AddDatabase(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
#if (UseSqlServer)
            options.UseSqlServer(connectionString);
#elif (UsePostgres)
            options.UseNpgsql(connectionString);
#elif (UseMySQL)
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
#endif
        });

        services.AddScoped<IDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
#endif
}