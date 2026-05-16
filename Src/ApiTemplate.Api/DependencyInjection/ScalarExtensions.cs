#if (UseScalar)
#if (EnableJwt)
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
#endif
using Scalar.AspNetCore;
using Viana.Results.OpenApi;

namespace ApiTemplate.Api.DependencyInjection;

public static class ScalarExtensions
{
    public static IServiceCollection AddOpenApiDocumentation(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddVianaResultTransformers();
#if (EnableJwt)
            options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
#endif
        });

        return services;
    }

    public static WebApplication UseOpenApiDocumentation(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
            return app;

        app.MapOpenApi();
        app.MapScalarApiReference();
        return app;
    }

#if (EnableJwt)
    private sealed class BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer
    {
        public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
        {
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
            document.Components.SecuritySchemes["bearer"] = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter your JWT token"
            };

            document.Security ??= [];
            document.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("bearer", document)] = []
            });

            return Task.CompletedTask;
        }
    }
#endif
}
#endif
