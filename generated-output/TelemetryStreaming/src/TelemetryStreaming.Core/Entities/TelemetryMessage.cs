// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace TelemetryStreaming.Core.Entities;

public class TelemetryMessage
{
    public Guid TelemetryMessageId { get; set; }
    public required string Source { get; set; }
    public required string MetricName { get; set; }
    public required string Value { get; set; }
    public string? Unit { get; set; }
    public TelemetryType Type { get; set; } = TelemetryType.Metric;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, string>? Tags { get; set; }
}

public enum TelemetryType { Metric, Event, Trace, Log }