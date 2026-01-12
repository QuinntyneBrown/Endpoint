// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint;
using Endpoint.Artifacts.Abstractions;
using Endpoint.Engineering.Cli.Commands;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Setup dependency injection
var services = new ServiceCollection();

// Add logging
services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});

// Add MediatR and scan the assembly containing EventDrivenMicroservicesCreateRequest
services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<EventDrivenMicroservicesCreateRequest>());

// Add core services from Endpoint assembly
services.AddCoreServices(typeof(IArtifactGenerator).Assembly);

// Add DotNet services
services.AddDotNetServices();

var serviceProvider = services.BuildServiceProvider();

var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
var mediator = serviceProvider.GetRequiredService<IMediator>();

try
{
    logger.LogInformation("Creating Event Driven Microservices solution");

    // Get output directory from command line args or use default
    var outputDirectory = args.Length > 0 ? args[0] : Path.Combine(Directory.GetCurrentDirectory(), "output");

    logger.LogInformation("Output directory: {OutputDirectory}", outputDirectory);

    // Create request to generate Event Driven Microservices solution
    var request = new EventDrivenMicroservicesCreateRequest
    {
        Name = "EventDrivenDemo",
        Services = "Orders,Inventory,Shipping",
        Directory = outputDirectory
    };

    await mediator.Send(request);

    logger.LogInformation("Event Driven Microservices solution created successfully at: {OutputDirectory}", outputDirectory);
}
catch (Exception ex)
{
    logger.LogError(ex, "Error creating Event Driven Microservices solution");
}
