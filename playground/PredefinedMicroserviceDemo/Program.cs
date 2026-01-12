// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.CommandLine;
using Endpoint;
using Endpoint.Artifacts.Abstractions;
using Endpoint.Engineering.Cli.Commands;
using Endpoint.Engineering.Microservices;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// ============================================================================
// Predefined Microservice Demo
// ============================================================================
// This playground demonstrates the MicroserviceFactory functionality which
// creates pre-configured microservice solutions with industry-standard
// packages and project structures.
//
// Each microservice includes:
// - {Name}.Core       - Domain logic, entities, CQRS handlers
// - {Name}.Infrastructure - Data access and external services
// - {Name}.Api        - ASP.NET Core Web API
//
// Usage:
//   dotnet run                              # Creates Identity microservice in ./output
//   dotnet run -- --list                    # List all available microservices
//   dotnet run -- --name Notification       # Create Notification microservice
//   dotnet run -- --name Billing --dir C:\projects  # Create in specific directory
// ============================================================================

var rootCommand = new RootCommand("Predefined Microservice Demo - Creates pre-configured microservice solutions");

var nameOption = new Option<string>(
    aliases: ["--name", "-n"],
    description: "Name of the predefined microservice to create",
    getDefaultValue: () => "Identity");

var directoryOption = new Option<string>(
    aliases: ["--dir", "-d"],
    description: "Output directory for the microservice solution",
    getDefaultValue: () => Path.Combine(Directory.GetCurrentDirectory(), "output"));

var listOption = new Option<bool>(
    aliases: ["--list", "-l"],
    description: "List all available predefined microservices");

rootCommand.AddOption(nameOption);
rootCommand.AddOption(directoryOption);
rootCommand.AddOption(listOption);

rootCommand.SetHandler(async (name, directory, list) =>
{
    await RunDemoAsync(name, directory, list);
}, nameOption, directoryOption, listOption);

return await rootCommand.InvokeAsync(args);

async Task RunDemoAsync(string microserviceName, string outputDirectory, bool listOnly)
{
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
    var microserviceFactory = serviceProvider.GetRequiredService<IMicroserviceFactory>();

    Console.WriteLine();
    Console.WriteLine("========================================");
    Console.WriteLine("  Predefined Microservice Demo");
    Console.WriteLine("========================================");
    Console.WriteLine();

    if (listOnly)
    {
        // List all available predefined microservices
        logger.LogInformation("Available predefined microservices:");
        Console.WriteLine();

        var microservices = microserviceFactory.GetAvailableMicroservices();

        Console.WriteLine("| Microservice     | Description                                        |");
        Console.WriteLine("|-----------------|---------------------------------------------------|");
        Console.WriteLine("| Identity        | User authentication, authorization, identity mgmt  |");
        Console.WriteLine("| Tenant          | Multi-tenant architecture management               |");
        Console.WriteLine("| Notification    | Multi-channel notifications (push, SMS, email)     |");
        Console.WriteLine("| DocumentStorage | File uploads and document storage                  |");
        Console.WriteLine("| Search          | Full-text search and indexing                      |");
        Console.WriteLine("| Analytics       | Usage metrics and business intelligence            |");
        Console.WriteLine("| Billing         | Subscription and payment processing                |");
        Console.WriteLine("| OcrVision       | OCR and image analysis                             |");
        Console.WriteLine("| Scheduling      | Appointments and calendar sync                     |");
        Console.WriteLine("| Audit           | Domain events and user action recording            |");
        Console.WriteLine("| Export          | Report generation (PDF, Excel, CSV)                |");
        Console.WriteLine("| Email           | Transactional and marketing emails                 |");
        Console.WriteLine("| Integration     | Third-party API integrations and webhooks          |");
        Console.WriteLine("| Media           | Image, video, and audio processing                 |");
        Console.WriteLine("| Geolocation     | Mapping and geocoding services                     |");
        Console.WriteLine("| Tagging         | Tags and hierarchical classification               |");
        Console.WriteLine("| Collaboration   | Real-time sharing and collaboration                |");
        Console.WriteLine("| Calculation     | Complex financial calculations                     |");
        Console.WriteLine("| Import          | Bulk data import from various sources              |");
        Console.WriteLine("| Cache           | Distributed caching (Redis)                        |");
        Console.WriteLine("| RateLimiting    | API usage control and quotas                       |");
        Console.WriteLine("| Localization    | Translations and internationalization              |");
        Console.WriteLine("| Workflow        | Multi-step business process orchestration          |");
        Console.WriteLine("| Backup          | Data backup and disaster recovery                  |");
        Console.WriteLine();
        Console.WriteLine($"Total: {microservices.Count} predefined microservices available");
        Console.WriteLine();
        Console.WriteLine("Usage: dotnet run -- --name <MicroserviceName> [--dir <OutputDirectory>]");
        return;
    }

    try
    {
        logger.LogInformation("Creating {Name} microservice", microserviceName);
        logger.LogInformation("Output directory: {Directory}", outputDirectory);
        Console.WriteLine();

        // Ensure output directory exists
        if (Directory.Exists(outputDirectory))
        {
            logger.LogInformation("Cleaning existing output directory...");
            Directory.Delete(outputDirectory, recursive: true);
        }

        Directory.CreateDirectory(outputDirectory);

        // Create request to generate the predefined microservice
        var request = new PredefinedMicroserviceAddRequest
        {
            Name = microserviceName,
            Directory = outputDirectory
        };

        await mediator.Send(request);

        Console.WriteLine();
        Console.WriteLine("========================================");
        Console.WriteLine("  Generation Complete!");
        Console.WriteLine("========================================");
        Console.WriteLine();
        Console.WriteLine($"Microservice: {microserviceName}");
        Console.WriteLine($"Location: {outputDirectory}");
        Console.WriteLine();
        Console.WriteLine("Generated project structure:");
        Console.WriteLine($"  {microserviceName}/");
        Console.WriteLine($"    {microserviceName}.sln");
        Console.WriteLine($"    src/");
        Console.WriteLine($"      {microserviceName}.Core/         - Domain logic, entities, CQRS handlers");
        Console.WriteLine($"      {microserviceName}.Infrastructure/ - Data access, external services");
        Console.WriteLine($"      {microserviceName}.Api/          - ASP.NET Core Web API");
        Console.WriteLine();
        Console.WriteLine("Next steps:");
        Console.WriteLine($"  cd {outputDirectory}/{microserviceName}");
        Console.WriteLine("  dotnet restore");
        Console.WriteLine("  dotnet build");
        Console.WriteLine();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error creating microservice");
    }
}
