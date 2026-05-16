#if (UseSwashbuckle)
#if (EnableJwt)
using ApiTemplate.Api.Filters;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
#endif
using Viana.Results.OpenApi.Swashbuckle;

namespace ApiTemplate.Api.DependencyInjection;

public static class SwaggerExtensions
{
    public static IServiceCollection AddOpenApiDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.AddVianaResultFilters();
#if (EnableJwt)
            ConfigureJwtForSwagger(options);
            options.OperationFilter<UnauthorizedAndForbiddenOperationFilter>();
#endif
        });

        return services;
    }

    public static WebApplication UseOpenApiDocumentation(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
            return app;

        app.UseSwagger();
        app.UseSwaggerUI();
        return app;
    }

#if (EnableJwt)
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
#endif
