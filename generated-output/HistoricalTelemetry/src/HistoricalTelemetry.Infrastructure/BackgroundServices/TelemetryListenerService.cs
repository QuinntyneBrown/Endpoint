// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using HistoricalTelemetry.Core.Entities;
using HistoricalTelemetry.Core.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace HistoricalTelemetry.Infrastructure.BackgroundServices;

public class TelemetryListenerService : BackgroundService
{
    private readonly ILogger<TelemetryListenerService> logger;
    private readonly ITelemetryPersistenceService persistenceService;

    public TelemetryListenerService(
        ILogger<TelemetryListenerService> logger,
        ITelemetryPersistenceService persistenceService)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.persistenceService = persistenceService ?? throw new ArgumentNullException(nameof(persistenceService));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Telemetry Listener Service is starting");

        // This is a placeholder for Redis Pub/Sub listener
        // In a real implementation, this would subscribe to a Redis channel
        // and persist incoming telemetry messages to the database

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Simulated listener loop
                // In production, this would be replaced with:
                // await redis.SubscribeAsync("telemetry", async (channel, message) => {
                //     var record = JsonSerializer.Deserialize<HistoricalTelemetryRecord>(message);
                //     await persistenceService.PersistTelemetryAsync(record, stoppingToken);
                // });

                await Task.Delay(1000, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while listening for telemetry messages");
            }
        }

        logger.LogInformation("Telemetry Listener Service is stopping");
    }
}