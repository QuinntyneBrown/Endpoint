// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace HistoricalTelemetry.Core.DTOs;

public class PagedTelemetryResponse
{
    public IEnumerable<HistoricalTelemetryDto> Data { get; set; } = new List<HistoricalTelemetryDto>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}