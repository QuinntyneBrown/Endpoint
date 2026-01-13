// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using HistoricalTelemetry.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace HistoricalTelemetry.Infrastructure.Data;

public class HistoricalTelemetryDbContext : DbContext
{
    public HistoricalTelemetryDbContext(DbContextOptions<HistoricalTelemetryDbContext> options)
        : base(options)
    {
    }

    public DbSet<HistoricalTelemetryRecord> TelemetryRecords => Set<HistoricalTelemetryRecord>();
    public DbSet<TelemetryAggregation> TelemetryAggregations => Set<TelemetryAggregation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HistoricalTelemetryDbContext).Assembly);
    }
}