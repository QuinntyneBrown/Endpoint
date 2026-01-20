// Auto-generated code
using Microsoft.Extensions.DependencyInjection;
using FlightSim.Shared.Messaging.Abstractions;

namespace FlightSim.Shared.Messaging.AzureServiceBus;

/// <summary>
/// Extension methods for service registration.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Azure Service Bus event bus to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Configuration action.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAzureServiceBusEventBus(
        this IServiceCollection services,
        Action<AzureServiceBusOptions>? configure = null)
    {
        if (configure != null)
        {
            services.Configure(configure);
        }

        services.AddSingleton<IMessageSerializer, MessagePackMessageSerializer>();
        services.AddSingleton<IEventBus, AzureServiceBusEventBus>();

        return services;
    }
}
