// Auto-generated code
using Microsoft.Extensions.DependencyInjection;

namespace MCC.Shared.Shared.Messaging.Infrastructure;

/// <summary>
/// Extension methods for service registration.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds messaging infrastructure to the service collection.
    /// </summary>
    public static IServiceCollection AddMessagingInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IRetryExecutor, RetryExecutor>();
        services.AddSingleton<CircuitBreaker>();
        services.AddSingleton<IDeadLetterQueue, InMemoryDeadLetterQueue>();
        services.AddSingleton<ITracer, NoOpTracer>();
        return services;
    }
}
