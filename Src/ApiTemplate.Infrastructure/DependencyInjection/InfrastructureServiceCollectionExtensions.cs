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

/// <summary>
/// Registers host-agnostic infrastructure services (database, health checks).
/// Anything here must work identically in the API, background workers, migrations
/// and integration tests — it MUST NOT pull in HTTP-bound concerns or feature-
/// specific configuration (like JWT secrets) that some hosts do not provide.
///
/// Auth-related infrastructure (JWT issuer, password hashing, password strength)
/// lives in <see cref="AddAuthInfrastructure"/>, which only hosts that actually
/// authenticate users should call (typically only the API).
/// </summary>
public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDatabase(configuration.GetConnectionString("DefaultConnection")!);
        return services;
    }

#if (EnableJwt)
    /// <summary>
    /// Registers JWT issuance, password hashing and password-strength services.
    /// Only call from hosts that authenticate users (the API). Background workers
    /// must NOT call this — they have no JWT config and would fail on startup
    /// when <see cref="Microsoft.Extensions.Options.OptionsBuilder{TOptions}.ValidateOnStart"/>
    /// runs against an absent <c>JwtSettings</c> section.
    /// </summary>
    public static IServiceCollection AddAuthInfrastructure(this IServiceCollection services, IConfiguration configuration)
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

        services.AddScoped<ISystemDbContextFactory, SystemDbContextFactory>();

        services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>("database");

        return services;
    }
}
