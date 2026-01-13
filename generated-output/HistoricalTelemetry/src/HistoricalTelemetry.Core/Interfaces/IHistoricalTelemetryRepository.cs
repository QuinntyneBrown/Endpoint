// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using HistoricalTelemetry.Core.Entities;

namespace HistoricalTelemetry.Core.Interfaces;

public interface IHistoricalTelemetryRepository
{
    Task<HistoricalTelemetryRecord> AddAsync(HistoricalTelemetryRecord record, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<HistoricalTelemetryRecord> records, CancellationToken cancellationToken = default);
    Task<IEnumerable<HistoricalTelemetryRecord>> GetBySourceAsync(string source, DateTime startTime, DateTime endTime, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<IEnumerable<HistoricalTelemetryRecord>> GetByMetricAsync(string metricName, DateTime startTime, DateTime endTime, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<IEnumerable<HistoricalTelemetryRecord>> GetBySourceAndMetricAsync(string source, string metricName, DateTime startTime, DateTime endTime, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetCountAsync(string? source, string? metricName, DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default);
}