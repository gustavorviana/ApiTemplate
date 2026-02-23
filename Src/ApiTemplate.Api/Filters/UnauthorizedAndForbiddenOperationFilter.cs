using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json.Nodes;

namespace ApiTemplate.Api.Filters;

/// <summary>
/// Adds 401 (Unauthorized) and 403 (Forbidden) responses with RFC 9457 Problem schema
/// to operations that use [Authorize].
/// </summary>
public class UnauthorizedAndForbiddenOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var method = context.MethodInfo;
        var declaringType = method.DeclaringType;

        // If AllowAnonymous exists, don't add 401/403
        var isAnonymous =
            method.IsDefined(typeof(AllowAnonymousAttribute), inherit: true) ||
            (declaringType?.IsDefined(typeof(AllowAnonymousAttribute), inherit: true) ?? false);

        if (isAnonymous)
            return;

        var hasAuthorize =
            method.IsDefined(typeof(AuthorizeAttribute), inherit: true) ||
            (declaringType?.IsDefined(typeof(AuthorizeAttribute), inherit: true) ?? false);

        if (!hasAuthorize)
            return;

        var problemSchema = new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            Required = new HashSet<string>() { "type", "title", "status" },
            Properties = new Dictionary<string, IOpenApiSchema>()
        };


        problemSchema.Properties!["type"] = new OpenApiSchema { Type = JsonSchemaType.String, Description = "RFC 9457: Problem type URI" };
        problemSchema.Properties["title"] = new OpenApiSchema { Type = JsonSchemaType.String, Description = "RFC 9457: Human-readable summary" };
        problemSchema.Properties["status"] = new OpenApiSchema { Type = JsonSchemaType.Integer, Description = "RFC 9457: HTTP status code" };
         
        operation.Responses!.TryAdd("401", CreateResponse(problemSchema, 401, "Unauthorized"));
        operation.Responses.TryAdd("403", CreateResponse(problemSchema, 403, "Forbidden"));
    }

    private static OpenApiResponse CreateResponse(OpenApiSchema schema, int status, string title)
    {
        return new OpenApiResponse
        {
            Description = title,
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/json"] = new()
                {
                    Schema = schema,
                    Example = new JsonObject
                    {
                        ["type"] = "about:blank",
                        ["title"] = title,
                        ["status"] = status
                    }
                }
            }
        };
    }
}
