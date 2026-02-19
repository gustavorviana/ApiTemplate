using ApiTemplate.Api.DependencyInjection;
using ApiTemplate.Api.Filters;
using ApiTemplate.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults()
    .ConfigureOpenTelemetry()
    .AddDefaultHealthChecks();

// Add services to the container.

builder.Services.AddControllers(options =>
{
	options.Filters.Add<ResultToProblemResultFilter>();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
