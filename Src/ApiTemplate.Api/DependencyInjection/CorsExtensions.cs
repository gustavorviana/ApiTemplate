using ApiTemplate.Api.Configuration;

namespace ApiTemplate.Api.DependencyInjection;

public static class CorsExtensions
{
    public const string DefaultPolicyName = "Default";

    public static IServiceCollection AddCorsServices(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(CorsOptions.SectionName);

        services.AddOptions<CorsOptions>()
            .Bind(section)
            .ValidateOnStart();

        var options = section.Get<CorsOptions>() ?? new CorsOptions();

        services.AddCors(corsOptions =>
        {
            corsOptions.AddPolicy(DefaultPolicyName, policy =>
            {
                if (options.AllowedOrigins.Length > 0)
                    policy.WithOrigins(options.AllowedOrigins);

                policy.WithMethods(options.AllowedMethods.Length > 0 ? options.AllowedMethods : ["*"]);
                policy.WithHeaders(options.AllowedHeaders.Length > 0 ? options.AllowedHeaders : ["*"]);

                if (options.AllowCredentials)
                    policy.AllowCredentials();
            });
        });

        return services;
    }
}
