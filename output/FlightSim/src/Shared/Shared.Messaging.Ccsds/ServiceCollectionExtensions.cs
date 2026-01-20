// Auto-generated code
using Microsoft.Extensions.DependencyInjection;
using FlightSim.Shared.Messaging.Abstractions;

namespace FlightSim.Shared.Messaging.Ccsds;

/// <summary>
/// Extension methods for service registration.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds CCSDS serialization to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureRegistry">Action to configure the packet registry.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCcsdsSerializer(
        this IServiceCollection services,
        Action<PacketRegistry>? configureRegistry = null)
    {
        var registry = new PacketRegistry();
        configureRegistry?.Invoke(registry);

        services.AddSingleton(registry);
        services.AddSingleton<IMessageSerializer, CcsdsSerializer>();

        return services;
    }
}
