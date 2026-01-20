// Auto-generated code
using Microsoft.Extensions.DependencyInjection;
using MCC.Shared.Shared.Messaging.Abstractions;

namespace MCC.Shared.Shared.Messaging.Jsc;

/// <summary>
/// Extension methods for service registration.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds JSC messaging to the service collection.
    /// </summary>
    public static IServiceCollection AddJscMessaging(this IServiceCollection services)
    {
        services.AddSingleton<IMessageSerializer, JscMessageSerializer>();
        return services;
    }
}
