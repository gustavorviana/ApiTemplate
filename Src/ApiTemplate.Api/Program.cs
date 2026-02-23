using ApiTemplate.Api.DependencyInjection;
using ApiTemplate.Api.Filters;
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

#if (EnableJwt)
builder.Services.AddJwtAuthentication(builder.Configuration);
#endif

builder.Services.AddSwaggerDocumentation();

var app = builder.Build();

#if (UseDatabase && RunMigrationsOnStartup)
await DatabaseMigrationRunner.RunMigrationsAsync(app.Services);
#endif

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

#if (EnableJwt)
app.UseJwtAuthentication();
#endif

app.MapControllers();

app.Run();
