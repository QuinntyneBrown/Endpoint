// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TelemetryStreaming.Core.Interfaces;

namespace TelemetryStreaming.Infrastructure.BackgroundServices;

public class TelemetryPublisherService : BackgroundService
{
    private readonly ILogger<TelemetryPublisherService> logger;
    private readonly ITelemetryGenerator telemetryGenerator;
    private readonly ISubscriptionManager subscriptionManager;

    public TelemetryPublisherService(
        ILogger<TelemetryPublisherService> logger,
        ITelemetryGenerator telemetryGenerator,
        ISubscriptionManager subscriptionManager)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.telemetryGenerator = telemetryGenerator ?? throw new ArgumentNullException(nameof(telemetryGenerator));
        this.subscriptionManager = subscriptionManager ?? throw new ArgumentNullException(nameof(subscriptionManager));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Telemetry Publisher Service is starting");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var subscriptions = await subscriptionManager.GetActiveSubscriptionsAsync(stoppingToken);
                
                foreach (var subscription in subscriptions)
                {
                    if (subscription.SubscribedMetrics.Any())
                    {
                        foreach (var source in subscription.SubscribedSources.Any() ? subscription.SubscribedSources : new List<string> { "default" })
                        {
                            var messages = await telemetryGenerator.GenerateBatchAsync(source, subscription.SubscribedMetrics, stoppingToken);
                            // Messages would be sent via SignalR hub here
                        }
                    }
                }

                // Wait before next cycle - subscriptions are checked at a base rate
                await Task.Delay(1000, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while publishing telemetry");
            }
        }

        logger.LogInformation("Telemetry Publisher Service is stopping");
    }
}