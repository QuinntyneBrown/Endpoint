// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using HistoricalTelemetry.Core.Entities;
using HistoricalTelemetry.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace HistoricalTelemetry.Infrastructure.Services;

public class TelemetryPersistenceService : ITelemetryPersistenceService
{
    private readonly IHistoricalTelemetryRepository repository;
    private readonly ILogger<TelemetryPersistenceService> logger;

    public TelemetryPersistenceService(
        IHistoricalTelemetryRepository repository,
        ILogger<TelemetryPersistenceService> logger)
    {
        this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task PersistTelemetryAsync(HistoricalTelemetryRecord record, CancellationToken cancellationToken = default)
    {
        try
        {
            await repository.AddAsync(record, cancellationToken);
            logger.LogDebug("Persisted telemetry record: {Source}/{MetricName}", record.Source, record.MetricName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error persisting telemetry record: {Source}/{MetricName}", record.Source, record.MetricName);
            throw;
        }
    }

    public async Task PersistBatchAsync(IEnumerable<HistoricalTelemetryRecord> records, CancellationToken cancellationToken = default)
    {
        try
        {
            await repository.AddRangeAsync(records, cancellationToken);
            logger.LogDebug("Persisted batch of {Count} telemetry records", records.Count());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error persisting batch of telemetry records");
            throw;
        }
    }
}