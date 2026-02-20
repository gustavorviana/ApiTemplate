using ApiTemplate.Api.DependencyInjection;
#if (EnableResult)
using ApiTemplate.Api.Filters;
using ApiTemplate.Api.Serialization;
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
    options.Filters.Add<ApplicationResultFilter>();
})
.AddJsonOptions(json =>
{
	json.JsonSerializerOptions.Converters.Add(new ProblemResultJsonConverter());
});
#else
builder.Services.AddControllers();
#endif

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
