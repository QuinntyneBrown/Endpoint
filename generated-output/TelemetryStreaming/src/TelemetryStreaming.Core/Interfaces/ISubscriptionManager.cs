// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using TelemetryStreaming.Core.Entities;

namespace TelemetryStreaming.Core.Interfaces;

public interface ISubscriptionManager
{
    Task<TelemetrySubscription> CreateSubscriptionAsync(string connectionId, string clientId, CancellationToken cancellationToken = default);
    Task UpdateSubscriptionAsync(Guid subscriptionId, List<string> metrics, List<string> sources, int updateRate, CancellationToken cancellationToken = default);
    Task<TelemetrySubscription?> GetSubscriptionAsync(string connectionId, CancellationToken cancellationToken = default);
    Task RemoveSubscriptionAsync(string connectionId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TelemetrySubscription>> GetActiveSubscriptionsAsync(CancellationToken cancellationToken = default);
}