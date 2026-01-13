// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using HistoricalTelemetry.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HistoricalTelemetry.Infrastructure.Data.Configurations;

public class TelemetryAggregationConfiguration : IEntityTypeConfiguration<TelemetryAggregation>
{
    public void Configure(EntityTypeBuilder<TelemetryAggregation> builder)
    {
        builder.HasKey(x => x.AggregationId);
        builder.Property(x => x.Source).IsRequired().HasMaxLength(200);
        builder.Property(x => x.MetricName).IsRequired().HasMaxLength(200);
        builder.HasIndex(x => new { x.Source, x.MetricName, x.PeriodStart, x.AggregationType });
    }
}