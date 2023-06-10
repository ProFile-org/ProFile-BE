using System.Reflection;
using Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Common.Extensions;

public static class ServiceExtensions
{
    public static void AddAuthorizersFromAssembly(this IServiceCollection services, Assembly assembly,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        var authorizerType = typeof(IAuthorizer<>);
        assembly.GetTypesAssignableTo(authorizerType).ForEach((type) =>
        {
            foreach (var implementedInterfaces in type.ImplementedInterfaces)
            {
                switch (lifetime)
                {
                    case ServiceLifetime.Singleton:
                        services.AddSingleton(implementedInterfaces, type);
                        break;
                    
                    case ServiceLifetime.Scoped:
                        services.AddScoped(implementedInterfaces, type);
                        break;
                    
                    case ServiceLifetime.Transient:
                        services.AddTransient(implementedInterfaces, type);
                        break;
                }
            }
        });
    }

    public static List<TypeInfo> GetTypesAssignableTo(this Assembly assembly, Type compareType)
    {
        var typeInfoList = assembly.DefinedTypes.Where(x => x.IsClass 
                                                            && !x.IsAbstract
                                                            && x != compareType
                                                            && x.GetInterfaces()
                                                                .Any(y => y.IsGenericType 
                                                                          && y.GetGenericTypeDefinition() == compareType))?.ToList();
        return typeInfoList;
    }
}