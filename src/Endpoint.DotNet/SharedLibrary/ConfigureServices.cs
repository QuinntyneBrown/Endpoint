// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DotNet.SharedLibrary.Configuration;
using Endpoint.DotNet.SharedLibrary.Services;
using Endpoint.DotNet.SharedLibrary.Services.Generators;
using Microsoft.Extensions.DependencyInjection;

namespace Endpoint.DotNet.SharedLibrary;

/// <summary>
/// Extension methods for registering shared library services.
/// </summary>
public static class ConfigureServices
{
    /// <summary>
    /// Adds shared library generator services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSharedLibraryServices(this IServiceCollection services)
    {
        // Configuration
        services.AddSingleton<ISharedLibraryConfigLoader, SharedLibraryConfigLoader>();

        // Generator service
        services.AddSingleton<ISharedLibraryGeneratorService, SharedLibraryGeneratorService>();

        // Project generators
        services.AddSingleton<IProjectGenerator, AbstractionsProjectGenerator>();
        services.AddSingleton<IProjectGenerator, DomainProjectGenerator>();
        services.AddSingleton<IProjectGenerator, ContractsProjectGenerator>();
        services.AddSingleton<IProjectGenerator, RedisProjectGenerator>();
        services.AddSingleton<IProjectGenerator, UdpMulticastProjectGenerator>();
        services.AddSingleton<IProjectGenerator, AzureServiceBusProjectGenerator>();
        services.AddSingleton<IProjectGenerator, CcsdsProjectGenerator>();
        services.AddSingleton<IProjectGenerator, JscProjectGenerator>();
        services.AddSingleton<IProjectGenerator, MessagingInfrastructureProjectGenerator>();
        services.AddSingleton<IProjectGenerator, DocumentationGenerator>();
        services.AddSingleton<IProjectGenerator, SharedAggregatorProjectGenerator>();

        return services;
    }
}
