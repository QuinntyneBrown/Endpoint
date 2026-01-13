// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using HistoricalTelemetry.Core.DTOs;
using HistoricalTelemetry.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HistoricalTelemetry.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HistoricalTelemetryController : ControllerBase
{
    private readonly IHistoricalTelemetryRepository repository;
    private readonly ILogger<HistoricalTelemetryController> logger;

    public HistoricalTelemetryController(
        IHistoricalTelemetryRepository repository,
        ILogger<HistoricalTelemetryController> logger)
    {
        this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet]
    public async Task<ActionResult<PagedTelemetryResponse>> Query([FromQuery] TelemetryQueryRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Querying historical telemetry: Source={Source}, Metric={MetricName}, Page={Page}", 
            request.Source, request.MetricName, request.Page);

        IEnumerable<Core.Entities.HistoricalTelemetryRecord> records;

        if (!string.IsNullOrEmpty(request.Source) && !string.IsNullOrEmpty(request.MetricName))
        {
            records = await repository.GetBySourceAndMetricAsync(
                request.Source, 
                request.MetricName, 
                request.StartTime, 
                request.EndTime, 
                request.Page, 
                request.PageSize, 
                cancellationToken);
        }
        else if (!string.IsNullOrEmpty(request.Source))
        {
            records = await repository.GetBySourceAsync(
                request.Source, 
                request.StartTime, 
                request.EndTime, 
                request.Page, 
                request.PageSize, 
                cancellationToken);
        }
        else if (!string.IsNullOrEmpty(request.MetricName))
        {
            records = await repository.GetByMetricAsync(
                request.MetricName, 
                request.StartTime, 
                request.EndTime, 
                request.Page, 
                request.PageSize, 
                cancellationToken);
        }
        else
        {
            return BadRequest("Either Source or MetricName must be specified");
        }

        var totalCount = await repository.GetCountAsync(
            request.Source, 
            request.MetricName, 
            request.StartTime, 
            request.EndTime, 
            cancellationToken);

        var dtos = records.Select(r => new HistoricalTelemetryDto
        {
            RecordId = r.RecordId,
            Source = r.Source,
            MetricName = r.MetricName,
            Value = r.Value,
            Unit = r.Unit,
            Timestamp = r.Timestamp,
            StoredAt = r.StoredAt
        });

        var response = new PagedTelemetryResponse
        {
            Data = dtos,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };

        return Ok(response);
    }
}