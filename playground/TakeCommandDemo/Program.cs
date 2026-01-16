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
    logger.LogInformation("=== Take Command Demo ===");
    logger.LogInformation("This demo shows how to use the Take command to extract a folder from a git repository.");
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

    // Demo 1: Take a .NET project folder
    logger.LogInformation("=== Demo 1: Taking a .NET project folder ===");
    logger.LogInformation("");

    var dotNetRequest = new ALaCarteTakeRequest
    {
        Url = "https://github.com/QuinntyneBrown/AWSSDK.Extensions",
        Branch = "main",
        FromPath = "src/AWSSDK.Extensions",
        Directory = outputDirectory,
        SolutionName = "TakeDemo"
    };

    logger.LogInformation("Repository: {Url}", dotNetRequest.Url);
    logger.LogInformation("Branch: {Branch}", dotNetRequest.Branch);
    logger.LogInformation("Folder to Take: {FromPath}", dotNetRequest.FromPath);
    logger.LogInformation("Output Directory: {Directory}", dotNetRequest.Directory);
    logger.LogInformation("Solution Name: {SolutionName}", dotNetRequest.SolutionName);
    logger.LogInformation("");

    logger.LogInformation("Processing Take request...");
    var dotNetResult = await alaCarteService.TakeAsync(dotNetRequest);

    DisplayResults(logger, dotNetResult);

    // Demo 2: Take an Angular project folder
    logger.LogInformation("");
    logger.LogInformation("=== Demo 2: Taking an Angular project folder ===");
    logger.LogInformation("");

    var angularOutputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "generated-output-angular");

    // Clean up any existing demo directory
    if (Directory.Exists(angularOutputDirectory))
    {
        logger.LogInformation("Cleaning up existing angular output directory...");
        Directory.Delete(angularOutputDirectory, recursive: true);
    }

    // Create the output directory
    Directory.CreateDirectory(angularOutputDirectory);

    var angularRequest = new ALaCarteTakeRequest
    {
        Url = "https://github.com/QuinntyneBrown/Commitments",
        Branch = "master",
        FromPath = "src/Commitments.App/projects/commitments-app",
        Directory = angularOutputDirectory
    };

    logger.LogInformation("Repository: {Url}", angularRequest.Url);
    logger.LogInformation("Branch: {Branch}", angularRequest.Branch);
    logger.LogInformation("Folder to Take: {FromPath}", angularRequest.FromPath);
    logger.LogInformation("Output Directory: {Directory}", angularRequest.Directory);
    logger.LogInformation("");

    logger.LogInformation("Processing Take request...");
    var angularResult = await alaCarteService.TakeAsync(angularRequest);

    DisplayResults(logger, angularResult);

    // Summary
    logger.LogInformation("");
    logger.LogInformation("=== Demo Complete ===");
    logger.LogInformation("");
    logger.LogInformation("The Take command allows you to:");
    logger.LogInformation("  1. Clone a git/gitlab repository");
    logger.LogInformation("  2. Extract a specific folder from any branch");
    logger.LogInformation("  3. Automatically detect .NET or Angular projects");
    logger.LogInformation("  4. Create/update .NET solutions for C# projects");
    logger.LogInformation("  5. Create/update Angular workspaces for Angular projects");
    logger.LogInformation("");
    logger.LogInformation("CLI Usage:");
    logger.LogInformation("  endpoint take -u <repo-url> -b <branch> -f <folder-path> [-d <directory>] [-s <solution-name>]");
    logger.LogInformation("");
    logger.LogInformation("Examples:");
    logger.LogInformation("  endpoint take -u https://github.com/user/repo -b main -f src/MyProject");
    logger.LogInformation("  endpoint take -u https://github.com/user/repo -b develop -f src/MyLib -s MySolution");
}
catch (Exception ex)
{
    logger.LogError(ex, "Error during Take command demo");
}

void DisplayResults(ILogger logger, ALaCarteTakeResult result)
{
    logger.LogInformation("");
    logger.LogInformation("--- Results ---");
    logger.LogInformation("");

    logger.LogInformation("Success: {Success}", result.Success);
    logger.LogInformation("Output Directory: {OutputDirectory}", result.OutputDirectory);
    logger.LogInformation("Copied Folder Path: {CopiedFolderPath}", result.CopiedFolderPath);

    if (result.IsDotNetProject)
    {
        logger.LogInformation("Project Type: .NET Project");
    }

    if (result.IsAngularProject)
    {
        logger.LogInformation("Project Type: Angular Project");
    }

    if (!string.IsNullOrEmpty(result.SolutionPath))
    {
        logger.LogInformation("Solution Created/Updated: {SolutionPath}", result.SolutionPath);
    }

    if (!string.IsNullOrEmpty(result.AngularWorkspacePath))
    {
        logger.LogInformation("Angular Workspace Created/Updated: {AngularWorkspacePath}", result.AngularWorkspacePath);
    }

    if (result.CsprojFiles.Count > 0)
    {
        logger.LogInformation("Projects Found: {Count}", result.CsprojFiles.Count);
        foreach (var csproj in result.CsprojFiles)
        {
            logger.LogInformation("  - {Project}", csproj);
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
}
