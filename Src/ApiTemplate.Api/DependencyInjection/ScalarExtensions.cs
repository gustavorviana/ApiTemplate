#if (UseScalar)
#if (EnableJwt)
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;
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
            document.Components.SecuritySchemes ??= new Dictionary<string, OpenApiSecurityScheme>();
            document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter your JWT token"
            };

            var requirement = new OpenApiSecurityRequirement
            {
                [new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                }] = Array.Empty<string>()
            };

            document.SecurityRequirements ??= new List<OpenApiSecurityRequirement>();
            document.SecurityRequirements.Add(requirement);

            return Task.CompletedTask;
        }
    }
#endif
}
#endif
