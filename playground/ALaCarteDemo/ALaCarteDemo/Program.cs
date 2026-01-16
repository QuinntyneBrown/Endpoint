// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint;
using Endpoint.Artifacts.Abstractions;
using Endpoint.Engineering.ALaCarte;
using Endpoint.Engineering.ALaCarte.Models;
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

// Add MediatR and scan the assembly containing ALaCarte commands
services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<IALaCarteService>());

// Add core services from Endpoint assembly
services.AddCoreServices(typeof(IArtifactGenerator).Assembly);

// Add DotNet services
services.AddDotNetServices();

// Add Engineering services (includes ALaCarte services)
services.AddEngineeringServices();

var serviceProvider = services.BuildServiceProvider();

var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
var alaCarteService = serviceProvider.GetRequiredService<IALaCarteService>();

try
{
    logger.LogInformation("=== A La Carte Demo ===");
    logger.LogInformation("This demo will clone repositories and extract selected folders to create a custom workspace.");
    logger.LogInformation("");

    // Get the output directory
    var outputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "generated-output");

    // Clean up any existing demo directory
    if (Directory.Exists(outputDirectory))
    {
        logger.LogInformation("Cleaning up existing output directory...");
        Directory.Delete(outputDirectory, recursive: true);
    }

    // Create the output directory
    Directory.CreateDirectory(outputDirectory);

    logger.LogInformation("Creating ALaCarte request with repository configurations...");
    logger.LogInformation("");

    // Create the ALaCarte request
    var request = new ALaCarteRequest
    {
        Directory = outputDirectory,
        OutputType = OutputType.MixDotNetSolutionWithOtherFolders,
        SolutionName = "ALaCarteWorkspace.sln",
        Repositories = new List<RepositoryConfiguration>
        {
            // Repository 1: Commitments
            new RepositoryConfiguration
            {
                Url = "https://github.com/QuinntyneBrown/Commitments",
                Branch = "master",
                Folders = new List<FolderConfiguration>
                {
                    new FolderConfiguration
                    {
                        From = "src/Commitments.App/projects/commitments-app",
                        To = "src/ALaCarteWorkspace/projects"
                    },
                    new FolderConfiguration
                    {
                        From = "src/Commitments.Core",
                        To = "src/Commitments.Core"
                    }
                }
            },
            // Repository 2: AWSSDK.Extensions
            new RepositoryConfiguration
            {
                Url = "https://github.com/QuinntyneBrown/AWSSDK.Extensions",
                Branch = "main",
                Folders = new List<FolderConfiguration>
                {
                    new FolderConfiguration
                    {
                        From = "src/AWSSDK.Extensions",
                        To = "src/AWSSDK.Extensions"
                    }
                }
            }
        }
    };

    logger.LogInformation("Repository 1: https://github.com/QuinntyneBrown/Commitments (branch: master)");
    logger.LogInformation("  - Folder: src/Commitments.App/projects/commitments-app -> src/ALaCarteWorkspace/projects");
    logger.LogInformation("  - Folder: src/Commitments.Core -> src/Commitments.Core");
    logger.LogInformation("");
    logger.LogInformation("Repository 2: https://github.com/QuinntyneBrown/AWSSDK.Extensions (branch: main)");
    logger.LogInformation("  - Folder: src/AWSSDK.Extensions -> src/AWSSDK.Extensions");
    logger.LogInformation("");
    logger.LogInformation("Output Type: MixDotNetSolutionWithOtherFolders");
    logger.LogInformation("Solution Name: ALaCarteWorkspace.sln");
    logger.LogInformation("Output Directory: {OutputDirectory}", outputDirectory);
    logger.LogInformation("");

    logger.LogInformation("Processing ALaCarte request...");
    logger.LogInformation("");

    var result = await alaCarteService.ProcessAsync(request);

    logger.LogInformation("");
    logger.LogInformation("=== Results ===");
    logger.LogInformation("");

    logger.LogInformation("Output Directory: {OutputDirectory}", result.OutputDirectory);

    if (result.SolutionPath != null)
    {
        logger.LogInformation("Solution Created: {SolutionPath}", result.SolutionPath);
    }

    if (result.CsprojFiles.Count > 0)
    {
        logger.LogInformation("Projects Found: {Count}", result.CsprojFiles.Count);
        foreach (var csproj in result.CsprojFiles)
        {
            logger.LogInformation("  - {Project}", csproj);
        }
    }

    if (result.AngularWorkspacesCreated.Count > 0)
    {
        logger.LogInformation("Angular Workspaces Created: {Count}", result.AngularWorkspacesCreated.Count);
        foreach (var workspace in result.AngularWorkspacesCreated)
        {
            logger.LogInformation("  - {Workspace}", workspace);
        }
    }

    // Display warnings
    if (result.Warnings.Count > 0)
    {
        logger.LogInformation("");
        logger.LogWarning("Warnings ({Count}):", result.Warnings.Count);
        foreach (var warning in result.Warnings)
        {
            logger.LogWarning("  - {Warning}", warning);
        }
    }

    // Display errors
    if (result.Errors.Count > 0)
    {
        logger.LogInformation("");
        logger.LogError("Errors ({Count}):", result.Errors.Count);
        foreach (var error in result.Errors)
        {
            logger.LogError("  - {Error}", error);
        }
    }

    // Summary
    logger.LogInformation("");
    if (result.Success)
    {
        logger.LogInformation("=== Demo Completed Successfully! ===");
        logger.LogInformation("");
        logger.LogInformation("The generated workspace contains:");
        logger.LogInformation("  - Mixed .NET solution with other folders");
        logger.LogInformation("  - C# projects from Commitments.Core");
        logger.LogInformation("  - Angular workspace from commitments-app");
        logger.LogInformation("  - AWSSDK.Extensions library");
        logger.LogInformation("");
        logger.LogInformation("You can explore the generated output in: {OutputDirectory}", outputDirectory);
    }
    else
    {
        logger.LogError("Demo completed with errors. Please review the output above.");
    }
}
catch (Exception ex)
{
    logger.LogError(ex, "Error during ALaCarte demo");
}
