// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Cli;
using Endpoint.Core.Internals;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection;

public static class ConfigureServices
{
    public static void AddCliServices(this IServiceCollection services)        
    {        
        services.AddLogging(o => o.AddConsole());
        services.AddCoreServices();
        services.AddMediatR(typeof(Program));
        services.AddInfrastructureServices();
        services.AddSingleton(new Observable<INotification>());
        services.AddHostedService<CommandLineArgumentsProcessor>();
    }
}


