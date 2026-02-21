using ApiTemplate.Application.DependencyInjection;
using ApiTemplate.Application.Interfaces;
using ApiTemplate.Infrastructure.Auth;
#if (UseDatabase)
using Microsoft.Extensions.Configuration;
using ApiTemplate.Infrastructure.Data;
using ApiTemplate.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
#else
using ApiTemplate.Infrastructure.Fakes;
using Microsoft.Extensions.Configuration;

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

        builder.Services.AddJwtServices(builder.Configuration);

        // Uncomment the following to restrict the allowed schemes for service discovery.
        // builder.Services.Configure<ServiceDiscoveryOptions>(options =>
        // {
        //     options.AllowedSchemes = ["https"];
        // });

#if (UseDatabase)
        builder.Services.AddDatabase(builder.Configuration.GetConnectionString("DefaultConnection")!);
#else
        builder.Services.AddScoped<IWeatherForecastRepository, FakeWeatherForecastRepository>();
        builder.Services.AddScoped<IUserRepository, FakeUserRepository>();
        builder.Services.AddScoped<IRefreshTokenRepository, FakeRefreshTokenRepository>();
#endif

        return builder;
    }

    private static IServiceCollection AddJwtServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.AddSingleton<IJwtService, JwtService>();

        return services;
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
        services.AddScoped<IWeatherForecastRepository, WeatherForecastRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        return services;
    }
#endif
}