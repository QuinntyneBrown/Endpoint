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

// Add MediatR and scan the assembly containing CyclicRandomizrCreateRequest
services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<CyclicRandomizrCreateRequest>());

// Add core services from Endpoint assembly
services.AddCoreServices(typeof(IArtifactGenerator).Assembly);

// Add DotNet services
services.AddDotNetServices();

// Add Engineering services (includes CyclicRandomizr services)
services.AddEngineeringServices();

var serviceProvider = services.BuildServiceProvider();

var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
var mediator = serviceProvider.GetRequiredService<IMediator>();

try
{
    logger.LogInformation("=== Cyclic Randomizer Demo ===");
    logger.LogInformation("This demo shows how to generate a cyclic randomizer class for a .NET type.");
    logger.LogInformation("");

    // Get the output directory
    var outputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "generated-output");

    // Clean up any existing output directory
    if (Directory.Exists(outputDirectory))
    {
        logger.LogInformation("Cleaning up existing output directory...");
        Directory.Delete(outputDirectory, recursive: true);
    }

    // Create the output directory
    Directory.CreateDirectory(outputDirectory);
    logger.LogInformation("Output directory created: {Directory}", outputDirectory);
    logger.LogInformation("");

    // Define a sample type to generate a randomizer for
    var sampleTypeName = "CyclicRandomizrDemo.SensorReading";

    logger.LogInformation("--- Demo: Generate Cyclic Randomizer for {TypeName} ---", sampleTypeName);
    logger.LogInformation("");

    // Create request to generate cyclic randomizer
    var request = new CyclicRandomizrCreateRequest
    {
        TypeName = sampleTypeName,
        Directory = outputDirectory
    };

    logger.LogInformation("Generating cyclic randomizer...");
    logger.LogInformation("- Type: {TypeName}", sampleTypeName);
    logger.LogInformation("- Output Directory: {Directory}", outputDirectory);
    logger.LogInformation("");

    await mediator.Send(request);

    logger.LogInformation("");
    logger.LogInformation("=== Demo Completed Successfully! ===");
    logger.LogInformation("");
    logger.LogInformation("Generated files:");

    // List the generated files
    if (Directory.Exists(outputDirectory))
    {
        foreach (var file in Directory.GetFiles(outputDirectory, "*.*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(outputDirectory, file);
            logger.LogInformation("  - {FilePath}", relativePath);
            
            // Show a snippet of the generated file
            logger.LogInformation("");
            logger.LogInformation("Generated file content:");
            logger.LogInformation("--------------------");
            var content = await File.ReadAllTextAsync(file);
            Console.WriteLine(content);
            logger.LogInformation("--------------------");
        }
    }

    logger.LogInformation("");
    logger.LogInformation("The generated randomizer class includes:");
    logger.LogInformation("  - CreateRandomInstance() method to generate random instances of the type");
    logger.LogInformation("  - StartAsync() method to begin cyclic generation at a specified Hz rate");
    logger.LogInformation("  - Stop() and StopAsync() methods to halt generation");
    logger.LogInformation("  - Hz property to control generation frequency");
    logger.LogInformation("  - ExcludedProperties to skip specific properties during randomization");
    logger.LogInformation("");
    logger.LogInformation("The randomizer supports types like: int, long, string, DateTime, Guid, bool, double, and enums.");
}
catch (Exception ex)
{
    logger.LogError(ex, "Error during Cyclic Randomizer demo");
}

// Define a sample type for demonstration purposes
namespace CyclicRandomizrDemo
{
    public class SensorReading
    {
        public Guid Id { get; set; }
        public string SensorName { get; set; } = string.Empty;
        public double Temperature { get; set; }
        public double Humidity { get; set; }
        public double Pressure { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsActive { get; set; }
    }
}
