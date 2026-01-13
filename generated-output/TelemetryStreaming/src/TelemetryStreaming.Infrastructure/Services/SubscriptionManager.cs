// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using TelemetryStreaming.Core.Entities;
using TelemetryStreaming.Core.Interfaces;

namespace TelemetryStreaming.Infrastructure.Services;

public class SubscriptionManager : ISubscriptionManager
{
    private readonly ConcurrentDictionary<string, TelemetrySubscription> subscriptions = new();

    public Task<TelemetrySubscription> CreateSubscriptionAsync(string connectionId, string clientId, CancellationToken cancellationToken = default)
    {
        var subscription = new TelemetrySubscription
        {
            SubscriptionId = Guid.NewGuid(),
            ConnectionId = connectionId,
            ClientId = clientId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        subscriptions.TryAdd(connectionId, subscription);
        return Task.FromResult(subscription);
    }

    public Task UpdateSubscriptionAsync(Guid subscriptionId, List<string> metrics, List<string> sources, int updateRate, CancellationToken cancellationToken = default)
    {
        var subscription = subscriptions.Values.FirstOrDefault(s => s.SubscriptionId == subscriptionId);
        if (subscription != null)
        {
            subscription.SubscribedMetrics = metrics;
            subscription.SubscribedSources = sources;
            subscription.UpdateRateMs = updateRate;
            subscription.LastUpdateAt = DateTime.UtcNow;
        }
        return Task.CompletedTask;
    }

    public Task<TelemetrySubscription?> GetSubscriptionAsync(string connectionId, CancellationToken cancellationToken = default)
    {
        subscriptions.TryGetValue(connectionId, out var subscription);
        return Task.FromResult(subscription);
    }

    public Task RemoveSubscriptionAsync(string connectionId, CancellationToken cancellationToken = default)
    {
        subscriptions.TryRemove(connectionId, out _);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<TelemetrySubscription>> GetActiveSubscriptionsAsync(CancellationToken cancellationToken = default)
    {
        var activeSubscriptions = subscriptions.Values.Where(s => s.IsActive);
        return Task.FromResult(activeSubscriptions);
    }
}