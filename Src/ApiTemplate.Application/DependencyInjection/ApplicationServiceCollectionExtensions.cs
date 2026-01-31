using Microsoft.Extensions.DependencyInjection;

namespace ApiTemplate.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // services.AddScoped<CreateUserUseCase>();
        // services.AddValidatorsFromAssembly(...);
        return services;
    }
}
