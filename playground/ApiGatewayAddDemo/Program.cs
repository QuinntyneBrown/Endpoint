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

// Add MediatR and scan the assembly containing ApiGatewayAddRequest
services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<ApiGatewayAddRequest>());

// Add core services from Endpoint assembly
services.AddCoreServices(typeof(IArtifactGenerator).Assembly);

// Add DotNet services
services.AddDotNetServices();

// Add Engineering services (includes API Gateway services)
services.AddEngineeringServices();

var serviceProvider = services.BuildServiceProvider();

var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
var mediator = serviceProvider.GetRequiredService<IMediator>();

try
{
    logger.LogInformation("=== API Gateway Project Demo ===");
    logger.LogInformation("This demo will create an API Gateway project with YARP reverse proxy support.");
    logger.LogInformation("");

    // Get the output directory
    var demoSolutionDirectory = Path.Combine(Directory.GetCurrentDirectory(), "generated-output");

    // Clean up any existing demo directory
    if (Directory.Exists(demoSolutionDirectory))
    {
        logger.LogInformation("Cleaning up existing demo directory...");
        Directory.Delete(demoSolutionDirectory, recursive: true);
    }

    // Create the demo solution directory
    Directory.CreateDirectory(demoSolutionDirectory);

    // Create a basic solution file for the demo
    var solutionName = "ApiGatewayDemoSolution";
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

    // Create request to add API Gateway project
    var request = new ApiGatewayAddRequest
    {
        Name = solutionName,
        Directory = demoSolutionDirectory
    };

    logger.LogInformation("Adding API Gateway project to solution...");
    logger.LogInformation("- Solution: {SolutionName}", solutionName);
    logger.LogInformation("- Project: {ProjectName}", $"{solutionName}.ApiGateway");
    logger.LogInformation("");

    await mediator.Send(request);

    logger.LogInformation("");
    logger.LogInformation("=== Demo Completed Successfully! ===");
    logger.LogInformation("");
    logger.LogInformation("Generated files:");

    // List the generated files
    var apiGatewayProjectDir = Path.Combine(demoSolutionDirectory, $"{solutionName}.ApiGateway");
    if (Directory.Exists(apiGatewayProjectDir))
    {
        foreach (var file in Directory.GetFiles(apiGatewayProjectDir, "*.*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(demoSolutionDirectory, file);
            logger.LogInformation("  - {FilePath}", relativePath);
        }
    }

    logger.LogInformation("");
    logger.LogInformation("The API Gateway project includes:");
    logger.LogInformation("  - YARP Reverse Proxy for intelligent routing");
    logger.LogInformation("  - JWT Bearer authentication for security");
    logger.LogInformation("  - CORS management for cross-origin requests");
    logger.LogInformation("  - Rate limiting for resource protection");
    logger.LogInformation("  - Configuration for REST and WebSocket routing");
}
catch (Exception ex)
{
    logger.LogError(ex, "Error during API Gateway demo");
}

