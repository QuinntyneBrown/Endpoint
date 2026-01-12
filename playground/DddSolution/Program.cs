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

// Add MediatR and scan the assembly containing DddAppCreateRequest
services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<DddAppCreateRequest>());

// Add core services from Endpoint assembly
services.AddCoreServices(typeof(IArtifactGenerator).Assembly);

// Add DotNet services
services.AddDotNetServices();

// Add ModernWebAppPattern services
services.AddModernWebAppPatternCoreServices();

var serviceProvider = services.BuildServiceProvider();

var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
var mediator = serviceProvider.GetRequiredService<IMediator>();

try
{
    logger.LogInformation("Creating DDD application with product name: Sample");

    var playgroundDirectory = Path.Combine(Directory.GetCurrentDirectory(), "..");

    // Create request to generate Sample DDD solution
    var request = new DddAppCreateRequest
    {
        ProductName = "Sample",
        BoundedContext = "ToDos",
        Aggregate = "ToDo",
        Properties = "ToDoId:Guid,Title:String,IsComplete:String",
        Directory = playgroundDirectory
    };

    await mediator.Send(request);

    logger.LogInformation("Sample DDD solution created successfully in playground folder");
}
catch (Exception ex)
{
    logger.LogError(ex, "Error creating DDD solution");
}
