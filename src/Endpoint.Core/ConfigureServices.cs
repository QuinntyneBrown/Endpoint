// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Reflection;
using Endpoint.Core;
using Endpoint.Core.Artifacts;
using Endpoint.Core.Syntax;

namespace Microsoft.Extensions.DependencyInjection;

public static class ConfigureServices
{
    public static void AddCoreServices(this IServiceCollection services, Assembly assembly)
    {
        AddArifactGenerator(services, assembly);
        AddSyntaxGenerator(services, assembly);
    }

    public static void AddArifactGenerator(this IServiceCollection services, Assembly assembly)
    {
        var @interface = typeof(IArtifactGenerationStrategy<>);

        var implementations = assembly.GetTypes()
            .Where(type =>
                !type.IsAbstract &&
                type.GetInterfaces().Any(interfaceType =>
                    interfaceType.IsGenericType &&
                    interfaceType.GetGenericTypeDefinition() == @interface))
            .ToList();

        foreach (var implementation in implementations)
        {
            foreach (var implementedInterface in implementation.GetInterfaces())
            {
                if (implementedInterface.IsGenericType && implementedInterface.GetGenericTypeDefinition() == @interface)
                {
                    services.AddSingleton(implementedInterface, implementation);
                }
            }
        }

        services.AddSingleton<IObjectCache, ObjectCache>();
        services.AddSingleton<IArtifactGenerator, ArtifactGenerator>();
    }

    public static void AddSyntaxGenerator(this IServiceCollection services, Assembly assembly)
    {
        var @interface = typeof(ISyntaxGenerationStrategy<>);

        var implementations = assembly.GetTypes()
            .Where(type =>
                !type.IsAbstract &&
                type.GetInterfaces().Any(interfaceType =>
                    interfaceType.IsGenericType &&
                    interfaceType.GetGenericTypeDefinition() == @interface))
            .ToList();

        foreach (var implementation in implementations)
        {
            foreach (var implementedInterface in implementation.GetInterfaces())
            {
                if (implementedInterface.IsGenericType && implementedInterface.GetGenericTypeDefinition() == @interface)
                {
                    services.AddSingleton(implementedInterface, implementation);
                }
            }
        }

        services.AddSingleton<ISyntaxGenerator, SyntaxGenerator>();
    }
}