// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace HistoricalTelemetry.Core.DTOs;

public class HistoricalTelemetryDto
{
    public Guid RecordId { get; set; }
    public required string Source { get; set; }
    public required string MetricName { get; set; }
    public required string Value { get; set; }
    public string? Unit { get; set; }
    public DateTime Timestamp { get; set; }
    public DateTime StoredAt { get; set; }
}