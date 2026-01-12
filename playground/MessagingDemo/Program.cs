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

// Add MediatR and scan the assembly containing MessagingAddRequest
services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<MessagingAddRequest>());

// Add core services from Endpoint assembly
services.AddCoreServices(typeof(IArtifactGenerator).Assembly);

// Add DotNet services
services.AddDotNetServices();

// Add ModernWebAppPattern services
services.AddModernWebAppPatternCoreServices();

// Add RedisPubSub services
services.AddRedisPubSubServices();

var serviceProvider = services.BuildServiceProvider();

var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
var mediator = serviceProvider.GetRequiredService<IMediator>();

try
{
    logger.LogInformation("=== Messaging Project Demo ===");
    logger.LogInformation("This demo will create a messaging project with Redis Pub/Sub support.");
    logger.LogInformation("");

    // Get the output directory (parent of playground)
    var playgroundDirectory = Path.Combine(Directory.GetCurrentDirectory(), "..");
    var demoSolutionDirectory = Path.Combine(playgroundDirectory, "MessagingDemoSolution");

    // Clean up any existing demo directory
    if (Directory.Exists(demoSolutionDirectory))
    {
        logger.LogInformation("Cleaning up existing demo directory...");
        Directory.Delete(demoSolutionDirectory, recursive: true);
    }

    // Create the demo solution directory
    Directory.CreateDirectory(demoSolutionDirectory);

    // Create a basic solution file for the demo
    var solutionName = "MessagingDemoSolution";
    var solutionPath = Path.Combine(demoSolutionDirectory, $"{solutionName}.sln");

    logger.LogInformation("Creating demo solution: {SolutionName}", solutionName);

    // Create a minimal solution file
    var solutionContent = $"""
        Microsoft Visual Studio Solution File, Format Version 12.00
        # Visual Studio Version 17
        VisualStudioVersion = 17.0.31903.59
        MinimumVisualStudioVersion = 10.0.40219.1
        Global
        	GlobalSection(SolutionProperties) = preSolution
        		HideSolutionNode = FALSE
        	EndGlobalSection
        EndGlobal
        """;

    await File.WriteAllTextAsync(solutionPath, solutionContent);

    logger.LogInformation("Solution created at: {SolutionPath}", solutionPath);
    logger.LogInformation("");

    // Create request to add messaging project
    var request = new MessagingAddRequest
    {
        Name = solutionName,
        Directory = demoSolutionDirectory,
        UseLz4Compression = true
    };

    logger.LogInformation("Adding messaging project to solution...");
    logger.LogInformation("- Solution: {SolutionName}", solutionName);
    logger.LogInformation("- Project: {ProjectName}", $"{solutionName}.Messaging");
    logger.LogInformation("- LZ4 Compression: {UseLz4}", request.UseLz4Compression);
    logger.LogInformation("");

    await mediator.Send(request);

    logger.LogInformation("");
    logger.LogInformation("=== Demo Completed Successfully! ===");
    logger.LogInformation("");
    logger.LogInformation("Generated files:");

    // List the generated files
    var messagingProjectDir = Path.Combine(demoSolutionDirectory, $"{solutionName}.Messaging");
    if (Directory.Exists(messagingProjectDir))
    {
        foreach (var file in Directory.GetFiles(messagingProjectDir, "*.*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(demoSolutionDirectory, file);
            logger.LogInformation("  - {FilePath}", relativePath);
        }
    }

    logger.LogInformation("");
    logger.LogInformation("The messaging project includes:");
    logger.LogInformation("  - IMessage, IDomainEvent, ICommand interfaces");
    logger.LogInformation("  - MessageHeader and MessageEnvelope classes with MessagePack serialization");
    logger.LogInformation("  - IMessageSerializer interface and MessagePackMessageSerializer implementation");
    logger.LogInformation("  - IMessageTypeRegistry interface and MessageTypeRegistry implementation");
    logger.LogInformation("  - ConfigureServices extension for dependency injection");
}
catch (Exception ex)
{
    logger.LogError(ex, "Error during messaging demo");
}
