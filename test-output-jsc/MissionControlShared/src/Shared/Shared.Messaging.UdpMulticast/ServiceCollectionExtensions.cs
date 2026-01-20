// Auto-generated code
using Microsoft.Extensions.DependencyInjection;
using MCC.Shared.Shared.Messaging.Abstractions;

namespace MCC.Shared.Shared.Messaging.UdpMulticast;

/// <summary>
/// Extension methods for service registration.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds UDP multicast event bus to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Configuration action.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddUdpMulticastEventBus(
        this IServiceCollection services,
        Action<UdpMulticastOptions>? configure = null)
    {
        if (configure != null)
        {
            services.Configure(configure);
        }

        services.AddSingleton<IMessageSerializer, MessagePackMessageSerializer>();
        services.AddSingleton<IEventBus, UdpMulticastEventBus>();

        return services;
    }
}
