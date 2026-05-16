using ApiTemplate.Application.DependencyInjection;
using ApiTemplate.Application.Interfaces;
#if (EnableJwt)
using ApiTemplate.Infrastructure.Auth;
#if (EnablePasswordSecurity)
using ApiTemplate.Application.Core.Options;
#endif
#endif
using Microsoft.Extensions.Configuration;
using ApiTemplate.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ApiTemplate.Infrastructure.DependencyInjection;

public static class ApiServiceCollectionExtensions
{
    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddApplication();

        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler();
            http.AddServiceDiscovery();
        });

#if (EnableJwt)
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();
        builder.Services.AddJwtServices(builder.Configuration);
#endif

        builder.Services.AddDatabase(builder.Configuration.GetConnectionString("DefaultConnection")!);

        return builder;
    }

#if (EnableJwt)
    private static IServiceCollection AddJwtServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

#if (EnablePasswordSecurity)
        services.Configure<PasswordSecurityOptions>(configuration.GetSection(PasswordSecurityOptions.SectionName));
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
            options.UseSqlServer(connectionString);
#elif (UsePostgres)
            options.UseNpgsql(connectionString);
#elif (UseMySqlPomelo)
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
#elif (UseMySqlOracle)
            options.UseMySQL(connectionString);
#endif
        });

        services.AddScoped<IAppDbContextFactory, AppDbContextFactory>();

        return services;
    }
}
