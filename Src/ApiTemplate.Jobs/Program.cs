using ApiTemplate.Infrastructure.DependencyInjection;
using ExecutionFlow.Hangfire.DependencyInjection;
using Hangfire;
#if (UsePostgres)
using Hangfire.PostgreSql;
#elif (UseMySQL)
using Hangfire.MySql;
#endif

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddInfrastructure(builder.Configuration);

var hangfireConnection = builder.Configuration.GetConnectionString("Hangfire")
    ?? throw new InvalidOperationException("Missing connection string 'Hangfire'.");

builder.Services.AddHangfire(c =>
{
#if (UseSqlServer)
    c.UseSqlServerStorage(hangfireConnection);
#elif (UsePostgres)
    c.UsePostgreSqlStorage(hangfireConnection);
#elif (UseMySQL)
    c.UseStorage(new MySqlStorage(hangfireConnection, new MySqlStorageOptions()));
#endif
});
builder.Services.AddHangfireServer();
builder.Services.AddHangfireToExecutionFlow(options =>
{
    options.Scan(typeof(Program).Assembly);
});

builder.Services.AddHealthChecks().AddHangfire(_ => { });

var app = builder.Build();

app.MapDefaultEndpoints();
app.UseHangfireDashboard("");

app.Run();
