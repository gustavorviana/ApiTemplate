using ApiTemplate.Api.DependencyInjection;
using ApiTemplate.Api.Filters;
#if (EnableRateLimiting)
using ApiTemplate.Api.Extensions;
#endif
using Viana.Results.Mvc;
using Viana.Results.Mvc.Filters;
using ApiTemplate.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults()
    .ConfigureOpenTelemetry()
    .AddDefaultHealthChecks();

builder.Services.AddControllers(options =>
{
#if (UseValidation)
	options.Filters.Add<ValidationActionFilter>();
#endif
    options.Filters.Add<VianaResultFilter>();
}).AddVianaResultFilter();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

#if (EnableJwt)
builder.Services.AddJwtAuthentication(builder.Configuration);
#endif

builder.Services.AddOpenApiDocumentation();
#if (EnableRateLimiting)
builder.AddCustomRateLimiting();
#endif

var app = builder.Build();

app.UseExceptionHandler();

#if (RunMigrationsOnStartup)
await DatabaseMigrationRunner.RunMigrationsAsync(app.Services);
#endif

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseOpenApiDocumentation();
}

app.UseHttpsRedirection();

#if (EnableJwt)
app.UseJwtAuthentication();
#endif

#if (EnableRateLimiting)
var rateLimitingEnabled = app.Configuration
    .GetSection("RateLimiting")
    .GetValue<bool>("Enabled");

if (rateLimitingEnabled)
{
	app.UseRateLimiter();
}
#endif

app.MapControllers();

app.Run();
