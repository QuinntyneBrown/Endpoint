// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace HistoricalTelemetry.Core.Events;

public record TelemetryPersistedEvent(Guid RecordId, string Source, string MetricName, DateTime Timestamp, DateTime StoredAt);