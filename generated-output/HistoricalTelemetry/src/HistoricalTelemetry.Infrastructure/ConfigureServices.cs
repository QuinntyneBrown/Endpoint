// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using HistoricalTelemetry.Core.Interfaces;
using HistoricalTelemetry.Infrastructure.BackgroundServices;
using HistoricalTelemetry.Infrastructure.Data;
using HistoricalTelemetry.Infrastructure.Repositories;
using HistoricalTelemetry.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HistoricalTelemetry.Infrastructure;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<HistoricalTelemetryDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IHistoricalTelemetryRepository, HistoricalTelemetryRepository>();
        services.AddScoped<ITelemetryPersistenceService, TelemetryPersistenceService>();
        services.AddHostedService<TelemetryListenerService>();

        return services;
    }
}