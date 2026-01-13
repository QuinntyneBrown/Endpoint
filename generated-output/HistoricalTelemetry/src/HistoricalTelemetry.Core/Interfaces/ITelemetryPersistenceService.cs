// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using HistoricalTelemetry.Core.Entities;

namespace HistoricalTelemetry.Core.Interfaces;

public interface ITelemetryPersistenceService
{
    Task PersistTelemetryAsync(HistoricalTelemetryRecord record, CancellationToken cancellationToken = default);
    Task PersistBatchAsync(IEnumerable<HistoricalTelemetryRecord> records, CancellationToken cancellationToken = default);
}