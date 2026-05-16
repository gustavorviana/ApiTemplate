using ApiTemplate.Api.Filters;
using Viana.Results.Mvc;
using Viana.Results.Mvc.Filters;

namespace ApiTemplate.Api.DependencyInjection;

public static class ApiServicesExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddControllers(options =>
        {
#if (UseValidation)
            options.Filters.Add<ValidationActionFilter>();
#endif
            options.Filters.Add<VianaResultFilter>();
        }).AddVianaResultFilter();

        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();

        return services;
    }
}
