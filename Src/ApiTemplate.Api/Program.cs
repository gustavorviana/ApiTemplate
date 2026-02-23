using ApiTemplate.Api.DependencyInjection;
#if (EnableResult)
using ApiTemplate.Api.Filters;
using Viana.Results.Mvc;
using Viana.Results.Mvc.Filters;
#endif
using ApiTemplate.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults()
    .ConfigureOpenTelemetry()
    .AddDefaultHealthChecks();

#if (EnableResult)
builder.Services.AddControllers(options =>
{
#if (UseValidation)
	options.Filters.Add<ValidationActionFilter>();
#endif
    options.Filters.Add<VianaResultFilter>();
}).AddVianaResultFilter();
#else
builder.Services.AddControllers();
#endif

#if (EnableJwt)
builder.Services.AddJwtAuthentication(builder.Configuration);
#endif

#if (EnableResult || EnableJwt)
builder.Services.AddSwaggerDocumentation();
#else
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
#endif

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
