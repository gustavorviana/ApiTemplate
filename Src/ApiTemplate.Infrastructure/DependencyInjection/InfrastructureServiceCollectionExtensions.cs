using ApiTemplate.Application.DependencyInjection;
using ApiTemplate.Application.Interfaces;
#if (EnableJwt)
using ApiTemplate.Infrastructure.Auth;
#if (EnablePasswordSecurity)
using ApiTemplate.Application.Core.Options;
#endif
#endif
using ApiTemplate.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ApiTemplate.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddApplication();
        services.AddDatabase(configuration.GetConnectionString("DefaultConnection")!);
#if (EnableJwt)
        services.AddJwtServices(configuration);
#endif
        return services;
    }

#if (EnableJwt)
    private static IServiceCollection AddJwtServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JwtSettings>()
            .Bind(configuration.GetSection(JwtSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

#if (EnablePasswordSecurity)
        services.AddOptions<PasswordSecurityOptions>()
            .Bind(configuration.GetSection(PasswordSecurityOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton<IPasswordSecurityProvider, IsoPasswordSecurityProvider>();
#endif

        services.AddSingleton<IJwtService, JwtService>();
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();

        return services;
    }
#endif

    private static IServiceCollection AddDatabase(this IServiceCollection services, string connectionString)
    {
        services.AddDbContextFactory<AppDbContext>(options =>
        {
#if (UseSqlServer)
#if (EnableDbResilience)
            options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure());
#else
            options.UseSqlServer(connectionString);
#endif
#elif (UsePostgres)
#if (EnableDbResilience)
            options.UseNpgsql(connectionString, npg => npg.EnableRetryOnFailure());
#else
            options.UseNpgsql(connectionString);
#endif
#elif (UseMySqlPomelo)
#if (EnableDbResilience)
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), my => my.EnableRetryOnFailure());
#else
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
#endif
#elif (UseMySqlOracle)
            options.UseMySQL(connectionString);
#endif
        });

        services.AddScoped<IAppDbContextFactory, AppDbContextFactory>();

        services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>("database");

        return services;
    }
}
