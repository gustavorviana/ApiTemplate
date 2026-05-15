using ApiTemplate.Application.UseCases;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace ApiTemplate.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        RegisterUseCases(services);
        return services;
    }

    private static void RegisterUseCases(IServiceCollection services)
    {
        var useCaseOpenType = typeof(IUseCaseHandler<,>);
        var types = GetLoadableTypes(typeof(ApplicationServiceCollectionExtensions).Assembly);

        foreach (var type in types)
        {
            if (!IsConcrete(type)) continue;

            var implementsHandler = type.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == useCaseOpenType);

            if (!implementsHandler) continue;

            services.AddScoped(type);
        }
    }

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
