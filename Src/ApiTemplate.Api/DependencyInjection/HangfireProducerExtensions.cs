#if (EnableHangfire)
using ExecutionFlow.Hangfire.DependencyInjection;
using Hangfire;
#if (UsePostgres)
using Hangfire.PostgreSql;
#elif (UseMySQL)
using Hangfire.MySql;
#endif

namespace ApiTemplate.Api.DependencyInjection;

public static class HangfireProducerExtensions
{
    public static IServiceCollection AddHangfireProducer(this IServiceCollection services, IConfiguration configuration)
    {
        var hangfireConnection = configuration.GetConnectionString("Hangfire")
            ?? throw new InvalidOperationException("Missing connection string 'Hangfire'.");

        services.AddHangfire(c =>
        {
#if (UseSqlServer)
            c.UseSqlServerStorage(hangfireConnection);
#elif (UsePostgres)
            c.UsePostgreSqlStorage(hangfireConnection);
#elif (UseMySQL)
            c.UseStorage(new MySqlStorage(hangfireConnection, new MySqlStorageOptions()));
#endif
        });

        services.AddExecutionFlowDispatcher();
        return services;
    }
}
#endif
