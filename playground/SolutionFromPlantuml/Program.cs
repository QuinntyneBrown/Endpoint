// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint;
using Endpoint.Artifacts.Abstractions;
using Endpoint.Cli.Commands;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Configuration
const string SolutionName = "ToDo";
const string PlantUmlSourcePath = @"C:\demo-plantuml-files";
const string OutputDirectory = @"C:\demo-out";

// Setup dependency injection
var services = new ServiceCollection();

// Add logging
services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});

// Add MediatR and scan the assembly containing SolutionCreateFromPlantUmlRequest
services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<SolutionCreateFromPlantUmlRequest>());

// Add core services from Endpoint assembly
services.AddCoreServices(typeof(IArtifactGenerator).Assembly);

// Add DotNet services (includes PlantUML parser services)
services.AddDotNetServices();

var serviceProvider = services.BuildServiceProvider();

var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
var mediator = serviceProvider.GetRequiredService<IMediator>();

try
{
    logger.LogInformation("===========================================");
    logger.LogInformation("  PlantUML Solution Generator Playground");
    logger.LogInformation("===========================================");
    logger.LogInformation("");
    logger.LogInformation("Configuration:");
    logger.LogInformation("  Solution Name:      {SolutionName}", SolutionName);
    logger.LogInformation("  PlantUML Source:    {PlantUmlSourcePath}", PlantUmlSourcePath);
    logger.LogInformation("  Output Directory:   {OutputDirectory}", OutputDirectory);
    logger.LogInformation("");

    // Verify PlantUML source directory exists
    if (!Directory.Exists(PlantUmlSourcePath))
    {
        logger.LogError("PlantUML source directory does not exist: {PlantUmlSourcePath}", PlantUmlSourcePath);
        logger.LogInformation("Please create the directory and add .puml files before running this playground.");
        return 1;
    }

    // List PlantUML files found
    var pumlFiles = Directory.GetFiles(PlantUmlSourcePath, "*.puml", SearchOption.AllDirectories);
    logger.LogInformation("Found {Count} PlantUML file(s):", pumlFiles.Length);
    foreach (var file in pumlFiles)
    {
        logger.LogInformation("  - {FileName}", Path.GetFileName(file));
    }
    logger.LogInformation("");

    // Clean output directory if it exists
    if (Directory.Exists(OutputDirectory))
    {
        logger.LogInformation("Cleaning output directory: {OutputDirectory}", OutputDirectory);
        Directory.Delete(OutputDirectory, recursive: true);
    }

    Directory.CreateDirectory(OutputDirectory);

    // Create request to generate solution from PlantUML
    var request = new SolutionCreateFromPlantUmlRequest
    {
        Name = SolutionName,
        PlantUmlSourcePath = PlantUmlSourcePath,
        Directory = OutputDirectory
    };

    logger.LogInformation("Generating solution from PlantUML files...");
    logger.LogInformation("");

    await mediator.Send(request);

    logger.LogInformation("");
    logger.LogInformation("===========================================");
    logger.LogInformation("  Solution created successfully!");
    logger.LogInformation("===========================================");
    logger.LogInformation("");
    logger.LogInformation("Output location: {OutputDirectory}", Path.Combine(OutputDirectory, SolutionName));
    logger.LogInformation("");
    logger.LogInformation("Expected solution structure:");
    logger.LogInformation("├── {SolutionName}/", SolutionName);
    logger.LogInformation("│   ├── {SolutionName}.sln", SolutionName);
    logger.LogInformation("│   └── src/");
    logger.LogInformation("│       ├── {SolutionName}.Core/", SolutionName);
    logger.LogInformation("│       │   ├── Aggregates/");
    logger.LogInformation("│       │   │   ├── ToDoItem/");
    logger.LogInformation("│       │   │   └── ToDoList/");
    logger.LogInformation("│       │   ├── ToDoItem/ (CQRS operations)");
    logger.LogInformation("│       │   └── ToDoList/ (CQRS operations)");
    logger.LogInformation("│       ├── {SolutionName}.Infrastructure/", SolutionName);
    logger.LogInformation("│       │   └── Data/");
    logger.LogInformation("│       │       └── ToDoDbContext.cs");
    logger.LogInformation("│       └── {SolutionName}.Api/", SolutionName);
    logger.LogInformation("│           ├── Controllers/");
    logger.LogInformation("│           │   ├── ToDoItemController.cs");
    logger.LogInformation("│           │   └── ToDoListController.cs");
    logger.LogInformation("│           └── Program.cs");
    logger.LogInformation("");
    logger.LogInformation("To build the solution:");
    logger.LogInformation("  cd {OutputDirectory}\\{SolutionName}", OutputDirectory, SolutionName);
    logger.LogInformation("  dotnet build");

    return 0;
}
catch (Exception ex)
{
    logger.LogError(ex, "Error creating solution from PlantUML");
    return 1;
}
