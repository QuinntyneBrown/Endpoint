// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint;
using Endpoint.Artifacts.Abstractions;
using Endpoint.Cli.Commands;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Solution configurations
var solutions = new[]
{
    new SolutionConfig("ECommerce", @"C:\demo-plantuml-files", @"C:\demo-out"),
    new SolutionConfig("ToDo", @"C:\simple-demo-plantuml-files", @"C:\simple-demo-out")
};

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

    foreach (var solution in solutions)
    {
        logger.LogInformation("-------------------------------------------");
        logger.LogInformation("Processing: {SolutionName}", solution.Name);
        logger.LogInformation("-------------------------------------------");
        logger.LogInformation("  PlantUML Source:    {PlantUmlSourcePath}", solution.PlantUmlSourcePath);
        logger.LogInformation("  Output Directory:   {OutputDirectory}", solution.OutputDirectory);
        logger.LogInformation("");

        // Verify PlantUML source directory exists
        if (!Directory.Exists(solution.PlantUmlSourcePath))
        {
            logger.LogWarning("PlantUML source directory does not exist: {PlantUmlSourcePath}", solution.PlantUmlSourcePath);
            logger.LogWarning("Skipping {SolutionName}...", solution.Name);
            continue;
        }

        // List PlantUML files found
        var pumlFiles = Directory.GetFiles(solution.PlantUmlSourcePath, "*.puml", SearchOption.AllDirectories);
        logger.LogInformation("Found {Count} PlantUML file(s):", pumlFiles.Length);
        foreach (var file in pumlFiles)
        {
            logger.LogInformation("  - {FileName}", Path.GetFileName(file));
        }
        logger.LogInformation("");

        // Clean output directory if it exists
        if (Directory.Exists(solution.OutputDirectory))
        {
            logger.LogInformation("Cleaning output directory: {OutputDirectory}", solution.OutputDirectory);
            try
            {
                Directory.Delete(solution.OutputDirectory, recursive: true);
            }
            catch (IOException ex)
            {
                logger.LogWarning("Could not fully clean output directory (files may be locked): {Message}", ex.Message);
            }
        }

        Directory.CreateDirectory(solution.OutputDirectory);

        // Create request to generate solution from PlantUML
        var request = new SolutionCreateFromPlantUmlRequest
        {
            Name = solution.Name,
            PlantUmlSourcePath = solution.PlantUmlSourcePath,
            Directory = solution.OutputDirectory
        };

        logger.LogInformation("Generating solution from PlantUML files...");
        logger.LogInformation("");

        await mediator.Send(request);

        logger.LogInformation("");
        logger.LogInformation("{SolutionName} created successfully!", solution.Name);
        logger.LogInformation("Output location: {OutputDirectory}", Path.Combine(solution.OutputDirectory, solution.Name));
        logger.LogInformation("");
    }

    logger.LogInformation("===========================================");
    logger.LogInformation("  All solutions created successfully!");
    logger.LogInformation("===========================================");
    logger.LogInformation("");
    logger.LogInformation("To build the solutions:");
    foreach (var solution in solutions)
    {
        logger.LogInformation("  cd {OutputDirectory}\\{SolutionName} && dotnet build", solution.OutputDirectory, solution.Name);
    }

    return 0;
}
catch (Exception ex)
{
    logger.LogError(ex, "Error creating solution from PlantUML");
    return 1;
}

record SolutionConfig(string Name, string PlantUmlSourcePath, string OutputDirectory);
