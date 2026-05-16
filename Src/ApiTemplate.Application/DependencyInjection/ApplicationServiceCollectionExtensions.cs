using ApiTemplate.Application.UseCases;
#if (UseValidation)
using FluentValidation;
#endif
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace ApiTemplate.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var types = GetLoadableTypes(typeof(ApplicationServiceCollectionExtensions).Assembly);

        RegisterUseCases(services, types);
#if (UseValidation)
        RegisterValidators(services, types);
#endif
        return services;
    }

    private static void RegisterUseCases(IServiceCollection services, Type[] types)
    {
        var useCaseOpenType = typeof(IUseCaseHandler<,>);

        foreach (var type in types)
        {
            if (!IsConcrete(type)) continue;

            var implementsHandler = type.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == useCaseOpenType);

            if (!implementsHandler) continue;

            services.AddScoped(type);
        }
    }

#if (UseValidation)
    private static void RegisterValidators(IServiceCollection services, Type[] types)
    {
        var validatorOpenType = typeof(IValidator<>);

        foreach (var type in types)
        {
            if (!IsConcrete(type)) continue;

            var validatorInterface = type.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == validatorOpenType);

            if (validatorInterface is null) continue;

            services.AddScoped(validatorInterface, type);
        }
    }
#endif

    private static bool IsConcrete(Type type)
        => type.IsClass && !type.IsAbstract && !type.ContainsGenericParameters;

    private static Type[] GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t != null).ToArray()!;
        }
    }
}
