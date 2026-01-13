// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace HistoricalTelemetry.Core.DTOs;

public class TelemetryQueryRequest
{
    public string? Source { get; set; }
    public string? MetricName { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}