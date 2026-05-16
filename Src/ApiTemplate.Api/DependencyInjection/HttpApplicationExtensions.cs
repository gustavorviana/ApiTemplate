using ApiTemplate.Application.Interfaces;
using ApiTemplate.Application.UseCases;
using ApiTemplate.Infrastructure.Data;
#if (EnableJwt)
using ApiTemplate.Infrastructure.Auth;
using ApiTemplate.Infrastructure.DependencyInjection;
#endif
#if (UseValidation)
using FluentValidation;
#endif
using System.Reflection;

namespace ApiTemplate.Api.DependencyInjection;

/// <summary>
/// Registers application-layer services that are bound to the HTTP request pipeline.
/// These services assume an active HTTP context (e.g. <c>ICurrentUser</c>) and must
/// NOT be consumed by background workers, Hangfire handlers or other non-HTTP hosts.
///
/// Background hosts should depend on <see cref="ISystemDbContextFactory"/> and provide
/// their own user-context abstraction populated from the job payload.
/// </summary>
public static class HttpApplicationExtensions
{
    public static IServiceCollection AddHttpApplication(this IServiceCollection services, IConfiguration configuration)
    {
        var applicationAssembly = typeof(IUseCaseHandler<,>).Assembly;
        var types = GetLoadableTypes(applicationAssembly);

        RegisterUseCases(services, types);
#if (UseValidation)
        RegisterValidators(services, types);
#endif

#if (EnableJwt)
        services.AddAuthInfrastructure(configuration);
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpCurrentUser>();
#endif
        services.AddScoped<IAppDbContextFactory, AppDbContextFactory>();

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
