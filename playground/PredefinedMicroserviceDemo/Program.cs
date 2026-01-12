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

// Add MediatR and scan the assembly containing PredefinedMicroserviceAddRequest
services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<PredefinedMicroserviceAddRequest>());

// Add core services from Endpoint assembly
services.AddCoreServices(typeof(IArtifactGenerator).Assembly);

// Add DotNet services
services.AddDotNetServices();

// Add Engineering services (includes MicroserviceFactory)
services.AddEngineeringServices();

var serviceProvider = services.BuildServiceProvider();

var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
var mediator = serviceProvider.GetRequiredService<IMediator>();

try
{
    // Output directory for the demo solution
    var demoDirectory = Path.Combine(Directory.GetCurrentDirectory(), "generated-output");

    // Clean up existing demo directory if it exists
    if (Directory.Exists(demoDirectory))
    {
        logger.LogInformation("Cleaning up existing demo directory: {Directory}", demoDirectory);
        Directory.Delete(demoDirectory, recursive: true);
    }

    Directory.CreateDirectory(demoDirectory);

    logger.LogInformation("=== PredefinedMicroserviceAdd Demo ===");
    logger.LogInformation("Output directory: {Directory}", demoDirectory);

    // First, list all available predefined microservices
    logger.LogInformation("Listing available predefined microservices...");

    var listRequest = new PredefinedMicroserviceAddRequest
    {
        ListAvailable = true
    };

    await mediator.Send(listRequest);

    // Now add an Identity microservice
    logger.LogInformation("Adding Identity microservice to demo solution...");

    var addIdentityRequest = new PredefinedMicroserviceAddRequest
    {
        Name = "Identity",
        Directory = demoDirectory
    };

    await mediator.Send(addIdentityRequest);

    logger.LogInformation("=== Demo Complete ===");
    logger.LogInformation("Identity microservice created successfully at: {Directory}", demoDirectory);
    logger.LogInformation("The microservice includes:");
    logger.LogInformation("  - Identity.Core: Domain entities, interfaces, and events");
    logger.LogInformation("  - Identity.Infrastructure: Data access with EF Core, repositories");
    logger.LogInformation("  - Identity.Api: REST API endpoints for authentication and user management");
}
catch (Exception ex)
{
    logger.LogError(ex, "Error during PredefinedMicroserviceAdd demo");
}
