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

    // Demo 3: Take from a local directory (FromDirectory parameter)
    logger.LogInformation("");
    logger.LogInformation("=== Demo 3: Taking from a local directory (FromDirectory) ===");
    logger.LogInformation("");

    var localOutputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "generated-output-from-local");

    // Clean up any existing demo directory
    if (Directory.Exists(localOutputDirectory))
    {
        logger.LogInformation("Cleaning up existing local output directory...");
        Directory.Delete(localOutputDirectory, recursive: true);
    }

    // Create the output directory
    Directory.CreateDirectory(localOutputDirectory);

    // First, we need to create a source directory to copy from
    var sourceDirectory = Path.Combine(Directory.GetCurrentDirectory(), "test-source");
    if (!Directory.Exists(sourceDirectory))
    {
        Directory.CreateDirectory(sourceDirectory);
        
        // Create a sample project in the source directory
        var sampleProjectDir = Path.Combine(sourceDirectory, "SampleProject");
        Directory.CreateDirectory(sampleProjectDir);
        
        // Create a simple .csproj file
        var csprojContent = @"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>
</Project>";
        File.WriteAllText(Path.Combine(sampleProjectDir, "SampleProject.csproj"), csprojContent);
        
        // Create a simple C# file
        var csContent = @"namespace SampleProject;

public class SampleClass
{
    public string GetMessage() => ""Hello from Sample Project!"";
}";
        File.WriteAllText(Path.Combine(sampleProjectDir, "SampleClass.cs"), csContent);
        
        logger.LogInformation("Created test source directory with sample project at: {SourceDirectory}", sourceDirectory);
    }

    var localRequest = new ALaCarteTakeRequest
    {
        FromDirectory = sourceDirectory,
        FromPath = "SampleProject",
        Directory = localOutputDirectory,
        SolutionName = "LocalTakeDemo"
    };

    logger.LogInformation("Source Directory: {FromDirectory}", localRequest.FromDirectory);
    logger.LogInformation("Folder to Take: {FromPath}", localRequest.FromPath);
    logger.LogInformation("Output Directory: {Directory}", localRequest.Directory);
    logger.LogInformation("Solution Name: {SolutionName}", localRequest.SolutionName);
    logger.LogInformation("");

    logger.LogInformation("Processing Take request from local directory...");
    var localResult = await alaCarteService.TakeAsync(localRequest);

    DisplayResults(logger, localResult);

    // Summary
    logger.LogInformation("");
    logger.LogInformation("=== Demo Complete ===");
    logger.LogInformation("");
    logger.LogInformation("The Take command allows you to:");
    logger.LogInformation("  1. Clone a git/gitlab repository and extract a specific folder");
    logger.LogInformation("  2. Copy a folder from a local directory (using FromDirectory parameter)");
    logger.LogInformation("  3. Extract a specific folder from any branch");
    logger.LogInformation("  4. Automatically detect .NET or Angular projects");
    logger.LogInformation("  5. Create/update .NET solutions for C# projects");
    logger.LogInformation("  6. Create/update Angular workspaces for Angular projects");
    logger.LogInformation("");
    logger.LogInformation("CLI Usage:");
    logger.LogInformation("  From Git: endpoint take -u <repo-url> [-d <directory>] [-s <solution-name>]");
    logger.LogInformation("  From Local: endpoint take -f <from-directory> [-d <directory>] [-s <solution-name>]");
    logger.LogInformation("");
    logger.LogInformation("Examples:");
    logger.LogInformation("  endpoint take -u https://github.com/user/repo/tree/main/src/MyProject");
    logger.LogInformation("  endpoint take -f /path/to/local/project -d ./output -s MySolution");
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
