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

// Add MediatR and scan the assembly containing MessagingProjectAddRequest
services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<MessagingProjectAddRequest>());

// Add core services from Endpoint assembly
services.AddCoreServices(typeof(IArtifactGenerator).Assembly);

// Add DotNet services
services.AddDotNetServices();

// Add Engineering services (includes MessagingArtifactFactory)
services.AddEngineeringServices();

var serviceProvider = services.BuildServiceProvider();

var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
var mediator = serviceProvider.GetRequiredService<IMediator>();

try
{
    // Output directory for the demo solution (repo root/generated-output)
    var repoRoot = Path.Combine(Directory.GetCurrentDirectory(), "..", "..");
    var demoDirectory = Path.Combine(repoRoot, "generated-output", "messaging-demo");

    // Clean up existing demo directory if it exists
    if (Directory.Exists(demoDirectory))
    {
        logger.LogInformation("Cleaning up existing demo directory: {Directory}", demoDirectory);
        Directory.Delete(demoDirectory, recursive: true);
    }

    Directory.CreateDirectory(demoDirectory);

    logger.LogInformation("=== MessagingProjectAdd Demo ===");
    logger.LogInformation("Output directory: {Directory}", demoDirectory);

    // Get the path to the message definition files
    var messagesFilePath = Path.Combine(Directory.GetCurrentDirectory(), "messages.json");
    var notificationsFilePath = Path.Combine(Directory.GetCurrentDirectory(), "notifications-messages.json");

    logger.LogInformation("");
    logger.LogInformation("=== Demo 1: Generate sample definition file ===");

    // First, demonstrate generating a sample definition file
    var sampleRequest = new MessagingProjectAddRequest
    {
        GenerateSample = true,
        Directory = demoDirectory
    };

    await mediator.Send(sampleRequest);

    logger.LogInformation("");
    logger.LogInformation("=== Demo 2: Create messaging project from single file ===");

    // Create a messaging project from the messages.json file
    var singleFileRequest = new MessagingProjectAddRequest
    {
        FilePath = messagesFilePath,
        Directory = demoDirectory
    };

    await mediator.Send(singleFileRequest);

    // Clean up for next demo
    var projectDir = Path.Combine(demoDirectory, "src", "ECommerce.Messaging");
    if (Directory.Exists(projectDir))
    {
        Directory.Delete(projectDir, recursive: true);
    }

    logger.LogInformation("");
    logger.LogInformation("=== Demo 3: Create messaging project from multiple files ===");

    // Create a messaging project from multiple definition files
    var multiFileRequest = new MessagingProjectAddRequest
    {
        Paths = new[] { messagesFilePath, notificationsFilePath },
        Directory = demoDirectory
    };

    await mediator.Send(multiFileRequest);

    // Clean up for next demo
    if (Directory.Exists(projectDir))
    {
        Directory.Delete(projectDir, recursive: true);
    }

    logger.LogInformation("");
    logger.LogInformation("=== Demo 4: Create basic messaging project without definition file ===");

    // Create a basic messaging project without a definition file
    var basicRequest = new MessagingProjectAddRequest
    {
        Name = "MyMicroservices",
        IncludeRedisPubSub = true,
        UseLz4Compression = true,
        Directory = demoDirectory
    };

    await mediator.Send(basicRequest);

    logger.LogInformation("");
    logger.LogInformation("=== Demo Complete ===");
    logger.LogInformation("Generated files are located at: {Directory}", demoDirectory);

    // List generated files
    logger.LogInformation("");
    logger.LogInformation("Generated project structure:");
    PrintDirectoryTree(demoDirectory, "", logger);
}
catch (Exception ex)
{
    logger.LogError(ex, "Error during MessagingProjectAdd demo");
}

static void PrintDirectoryTree(string path, string indent, ILogger logger)
{
    var dirInfo = new DirectoryInfo(path);

    foreach (var dir in dirInfo.GetDirectories())
    {
        logger.LogInformation("{Indent}{DirName}/", indent, dir.Name);
        PrintDirectoryTree(dir.FullName, indent + "  ", logger);
    }

    foreach (var file in dirInfo.GetFiles())
    {
        logger.LogInformation("{Indent}{FileName}", indent, file.Name);
    }
}
