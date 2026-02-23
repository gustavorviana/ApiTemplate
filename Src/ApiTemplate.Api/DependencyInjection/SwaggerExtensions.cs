#if !EnableJwtWithDatabase
using ApiTemplate.Api.Filters;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
#endif
using Viana.Results.OpenApi.Swashbuckle;

namespace ApiTemplate.Api.DependencyInjection;

/// <summary>
/// Swagger/OpenAPI configuration. Excluded when both Result and JWT are disabled.
/// </summary>
public static class SwaggerExtensions
{
	public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
	{
		services.AddEndpointsApiExplorer();
		services.AddSwaggerGen(options =>
		{
            options.AddVianaResultFilters();
#if (EnableJwtWithDatabase)
			ConfigureJwtForSwagger(options);
			options.OperationFilter<UnauthorizedAndForbiddenOperationFilter>();
#endif
        });

		return services;
	}

#if (EnableJwtWithDatabase)
    /// <summary>
    /// Configures Swagger for JWT Bearer authentication (security definition and requirement).
    /// </summary>
    private static void ConfigureJwtForSwagger(SwaggerGenOptions options)
    {
        var bearerScheme = new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter your JWT token"
        };

        options.AddSecurityDefinition("Bearer", bearerScheme);
        options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("bearer", document)] = []
        });
    }
#endif
}
