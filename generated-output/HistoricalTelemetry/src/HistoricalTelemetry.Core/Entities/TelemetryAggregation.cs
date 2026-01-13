// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace HistoricalTelemetry.Core.Entities;

public class TelemetryAggregation
{
    public Guid AggregationId { get; set; }
    public required string Source { get; set; }
    public required string MetricName { get; set; }
    public AggregationType AggregationType { get; set; }
    public decimal MinValue { get; set; }
    public decimal MaxValue { get; set; }
    public decimal AverageValue { get; set; }
    public decimal SumValue { get; set; }
    public int Count { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum AggregationType { Hourly, Daily, Weekly, Monthly }