// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Endpoint.DotNet;
using Endpoint.Internal;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Endpoint.DotNet.Cli;

public class CodeGeneratorApplicationBuilder
{
    private readonly IServiceCollection services = new ServiceCollection();

    public CodeGeneratorApplicationBuilder ConfigureServices(Action<IServiceCollection> services)
    {
        services.Invoke(this.services);

        return this;
    }

    public CodeGeneratorApplication Build()
    {
        var entryAssembly = Assembly.GetEntryAssembly();
        services.AddCoreServices(typeof(Constants).Assembly);
        services.AddDotNetServices();
        services.AddMediatR(configuration => configuration.RegisterServicesFromAssembly(entryAssembly));
        services.AddSingleton(new Observable<INotification>());

        var serviceProvider = services.BuildServiceProvider();

        var application = ActivatorUtilities.CreateInstance<CodeGeneratorApplication>(serviceProvider);

        return application;
    }
}
