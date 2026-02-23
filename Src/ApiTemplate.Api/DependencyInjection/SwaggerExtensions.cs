using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;
#if (EnableResult)
using ApiTemplate.Application.Results;
#endif

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
#if (EnableResult)
			ConfigureResultForSwagger(options);
#endif
#if (EnableJwt)
			ConfigureJwtForSwagger(options);
#endif
#if (EnableResult && EnableJwt)
			options.OperationFilter<UnauthorizedAndForbiddenOperationFilter>();
#endif
		});

		return services;
	}

#if (EnableResult)
	/// <summary>
	/// Configures Swagger to document the Result pattern (e.g. ProblemResult / RFC 9457).
	/// </summary>
	private static void ConfigureResultForSwagger(SwaggerGenOptions options)
	{
		options.MapType<ProblemResult>(() => new OpenApiSchema
		{
			Type = "object",
			Required = ["type", "title", "status"],
			Properties =
			{
				["type"] = new OpenApiSchema { Type = "string", Description = "RFC 9457: Problem type URI", Example = new Microsoft.OpenApi.Any.OpenApiString("about:blank") },
				["title"] = new OpenApiSchema { Type = "string", Description = "RFC 9457: Human-readable summary" },
				["status"] = new OpenApiSchema { Type = "integer", Description = "RFC 9457: HTTP status code" },
				["extensions"] = new OpenApiSchema { Type = "object", Description = "RFC 9457: Extension members" }
			}
		});
	}
#endif

#if (EnableJwt)
	/// <summary>
	/// Configures Swagger for JWT Bearer authentication (security definition and requirement).
	/// </summary>
	private static void ConfigureJwtForSwagger(SwaggerGenOptions options)
	{
		options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
		{
			Name = "Authorization",
			Type = SecuritySchemeType.Http,
			Scheme = "bearer",
			BearerFormat = "JWT",
			In = ParameterLocation.Header,
			Description = "Enter your JWT token"
		});

		options.AddSecurityRequirement(new OpenApiSecurityRequirement
		{
			[new OpenApiSecurityScheme
			{
				Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
			}] = []
		});
	}
#endif

#if (EnableResult && EnableJwt)
	/// <summary>
	/// Adds 401 (Unauthorized) and 403 (Forbidden) responses with RFC 9457 Problem schema
	/// to operations that use [Authorize].
	/// </summary>
	private sealed class UnauthorizedAndForbiddenOperationFilter : IOperationFilter
	{
		public void Apply(OpenApiOperation operation, OperationFilterContext context)
		{
			var hasAuthorize = context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any()
				|| context.MethodInfo.DeclaringType?.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any() == true;

			if (!hasAuthorize)
				return;

			var problemSchema = new OpenApiSchema
			{
				Type = "object",
				Required = ["type", "title", "status"],
				Properties =
				{
					["type"] = new OpenApiSchema { Type = "string", Description = "RFC 9457: Problem type URI" },
					["title"] = new OpenApiSchema { Type = "string", Description = "RFC 9457: Human-readable summary" },
					["status"] = new OpenApiSchema { Type = "integer", Description = "RFC 9457: HTTP status code" }
				}
			};

			operation.Responses.TryAdd("401", new OpenApiResponse
			{
				Description = "Unauthorized",
				Content = new Dictionary<string, OpenApiMediaType>
				{
					["application/json"] = new OpenApiMediaType
					{
						Schema = problemSchema,
						Example = new Microsoft.OpenApi.Any.OpenApiObject
						{
							["type"] = new Microsoft.OpenApi.Any.OpenApiString("about:blank"),
							["title"] = new Microsoft.OpenApi.Any.OpenApiString("Unauthorized"),
							["status"] = new Microsoft.OpenApi.Any.OpenApiInteger(401)
						}
					}
				}
			});

			operation.Responses.TryAdd("403", new OpenApiResponse
			{
				Description = "Forbidden",
				Content = new Dictionary<string, OpenApiMediaType>
				{
					["application/json"] = new OpenApiMediaType
					{
						Schema = problemSchema,
						Example = new Microsoft.OpenApi.Any.OpenApiObject
						{
							["type"] = new Microsoft.OpenApi.Any.OpenApiString("about:blank"),
							["title"] = new Microsoft.OpenApi.Any.OpenApiString("Forbidden"),
							["status"] = new Microsoft.OpenApi.Any.OpenApiInteger(403)
						}
					}
				}
			});
		}
	}
#endif
}
