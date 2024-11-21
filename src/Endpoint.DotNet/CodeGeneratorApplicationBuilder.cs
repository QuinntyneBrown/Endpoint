// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Reflection;
using Endpoint.Core.Internal;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Endpoint.DotNet;

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
        services.AddCoreServices(typeof(CodeGeneratorApplication).Assembly);
        services.AddDotNetServices();
        services.AddMediatR(configuration => configuration.RegisterServicesFromAssembly(entryAssembly));
        services.AddSingleton(new Observable<INotification>());

        var serviceProvider = services.BuildServiceProvider();

        var application = ActivatorUtilities.CreateInstance<CodeGeneratorApplication>(serviceProvider);

        return application;
    }
}
