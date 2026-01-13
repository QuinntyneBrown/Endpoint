// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using TelemetryStreaming.Core.Interfaces;
using TelemetryStreaming.Infrastructure.BackgroundServices;
using TelemetryStreaming.Infrastructure.Services;

namespace TelemetryStreaming.Infrastructure;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddSingleton<ITelemetryGenerator, TelemetryGenerator>();
        services.AddSingleton<ISubscriptionManager, SubscriptionManager>();
        services.AddHostedService<TelemetryPublisherService>();

        return services;
    }
}