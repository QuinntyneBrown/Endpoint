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
    // Output directory for the demo solution (repo root/generated-output)
    var repoRoot = Path.Combine(Directory.GetCurrentDirectory(), "..", "..");
    var demoDirectory = Path.Combine(repoRoot, "generated-output");

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

    // Test generating ALL 24 microservices
    var microservicesToTest = new[]
    {
        "Identity",
        "Tenant",
        "Notification",
        "DocumentStorage",
        "Search",
        "Analytics",
        "Billing",
        "OcrVision",
        "Scheduling",
        "Audit",
        "Export",
        "Email",
        "Integration",
        "Media",
        "Geolocation",
        "Tagging",
        "Collaboration",
        "Calculation",
        "Import",
        "Cache",
        "RateLimiting",
        "Localization",
        "Workflow",
        "Backup"
    };

    foreach (var microserviceName in microservicesToTest)
    {
        logger.LogInformation("Adding {Name} microservice...", microserviceName);

        var addRequest = new PredefinedMicroserviceAddRequest
        {
            Name = microserviceName,
            Directory = demoDirectory
        };

        await mediator.Send(addRequest);
    }

    logger.LogInformation("=== Demo Complete ===");
    logger.LogInformation("Generated {Count} microservices at: {Directory}", microservicesToTest.Length, demoDirectory);
}
catch (Exception ex)
{
    logger.LogError(ex, "Error during PredefinedMicroserviceAdd demo");
}
