using ApiTemplate.Api.DependencyInjection;
#if (EnableRateLimiting)
using ApiTemplate.Api.Extensions;
#endif
using ApiTemplate.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApiServices();
builder.Services.AddCorsServices(builder.Configuration);

#if (EnableJwt)
builder.Services.AddJwtAuthentication(builder.Configuration);
#endif

#if (EnableHangfire)
builder.Services.AddHangfireProducer(builder.Configuration);
#endif

#if (UseOpenApi)
builder.Services.AddOpenApiDocumentation();
#endif
#if (EnableRateLimiting)
builder.AddCustomRateLimiting();
#endif

var app = builder.Build();

app.UseExceptionHandler();

#if (RunMigrationsOnStartup)
await DatabaseMigrationRunner.RunMigrationsAsync(app.Services);
#endif

app.MapDefaultEndpoints();
#if (UseOpenApi)
app.UseOpenApiDocumentation();
#endif
app.UseHttpsRedirection();
app.UseCors(CorsExtensions.DefaultPolicyName);

#if (EnableJwt)
app.UseJwtAuthentication();
#endif

#if (EnableRateLimiting)
app.UseCustomRateLimiting();
#endif

app.MapControllers();

app.Run();

public partial class Program;
