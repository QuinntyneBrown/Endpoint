// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint;
using Endpoint.Artifacts.Abstractions;
using Endpoint.DotNet.Artifacts.PlantUml.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Configuration
var solutionName = "ECommercePlatform";
var outputDirectory = @"C:\sandbox";

// Sample sequence diagram with participants
var sequenceDiagram = @"@startuml
actor User as u

' Angular
participant ""Admin Dashboard"" as admin

' Angular
participant ""Customer Portal"" as customer

' Worker
participant ""Email Sender"" as emailWorker

' Worker
participant ""Order Processor"" as orderWorker

' Microservice
participant ""Order Management"" as orderService

' Microservice
participant ""Inventory Management"" as inventoryService

' Microservice
participant ""Customer Management"" as customerService

u -> admin : Login
admin -> orderService : GetOrders
orderService -> inventoryService : CheckInventory
inventoryService --> orderService : InventoryStatus
orderService --> admin : OrdersList

u -> customer : PlaceOrder
customer -> orderService : CreateOrder
orderService -> inventoryService : ReserveInventory
orderService -> customerService : GetCustomerInfo
orderService -> orderWorker : QueueOrderProcessing
orderWorker -> emailWorker : SendConfirmationEmail
emailWorker --> orderWorker : EmailSent
orderService --> customer : OrderConfirmation
@enduml";

// Setup dependency injection
var services = new ServiceCollection();

// Add logging
services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});

// Add core services from Endpoint assembly
services.AddCoreServices(typeof(IArtifactGenerator).Assembly);

// Add DotNet services (includes PlantUML parser services)
services.AddDotNetServices();

var serviceProvider = services.BuildServiceProvider();

var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
var parserService = serviceProvider.GetRequiredService<IPlantUmlParserService>();
var sequenceToSolutionService = serviceProvider.GetRequiredService<ISequenceToSolutionPlantUmlService>();
var solutionModelFactory = serviceProvider.GetRequiredService<IPlantUmlSolutionModelFactory>();
var artifactGenerator = serviceProvider.GetRequiredService<IArtifactGenerator>();

try
{
    logger.LogInformation("===========================================");
    logger.LogInformation("  Solution Create From Sequence Playground");
    logger.LogInformation("===========================================");
    logger.LogInformation("");

    logger.LogInformation("Input Sequence Diagram:");
    logger.LogInformation("-------------------------------------------");
    Console.WriteLine(sequenceDiagram);
    logger.LogInformation("-------------------------------------------");
    logger.LogInformation("");

    // Step 1: Parse the sequence diagram to extract participants
    logger.LogInformation("Step 1: Parsing sequence diagram...");
    var parsedDocument = parserService.ParseContent(sequenceDiagram);

    logger.LogInformation("Found {Count} participants:", parsedDocument.Participants.Count);
    foreach (var participant in parsedDocument.Participants)
    {
        logger.LogInformation("  - {Name} ({Alias}) - Type: {DotNetType}",
            participant.Name,
            participant.Alias,
            participant.DotNetType ?? "Unknown");
    }
    logger.LogInformation("");

    // Step 2: Generate solution PlantUML from sequence diagram
    logger.LogInformation("Step 2: Generating solution PlantUML specification...");
    var solutionPlantUml = sequenceToSolutionService.GenerateSolutionPlantUml(sequenceDiagram, solutionName);

    logger.LogInformation("Generated Solution PlantUML:");
    logger.LogInformation("-------------------------------------------");
    Console.WriteLine(solutionPlantUml);
    logger.LogInformation("-------------------------------------------");
    logger.LogInformation("");

    // Step 3: Parse the generated PlantUML
    logger.LogInformation("Step 3: Parsing generated PlantUML specification...");
    var solutionDocument = parserService.ParseContent(solutionPlantUml, "generated-solution.puml");

    var plantUmlSolutionModel = new Endpoint.DotNet.Artifacts.PlantUml.Models.PlantUmlSolutionModel
    {
        Name = solutionName,
        SourcePath = outputDirectory
    };
    plantUmlSolutionModel.Documents.Add(solutionDocument);

    logger.LogInformation("Parsed entities: {EntityCount}", plantUmlSolutionModel.GetAllClasses().Count());
    logger.LogInformation("Parsed enums: {EnumCount}", plantUmlSolutionModel.GetAllEnums().Count());
    logger.LogInformation("");

    // Step 4: Clean and prepare output directory
    logger.LogInformation("Step 4: Preparing output directory: {OutputDirectory}", outputDirectory);
    if (Directory.Exists(outputDirectory))
    {
        logger.LogInformation("Cleaning existing output directory...");
        try
        {
            Directory.Delete(outputDirectory, recursive: true);
        }
        catch (IOException ex)
        {
            logger.LogWarning("Could not fully clean output directory: {Message}", ex.Message);
        }
    }
    Directory.CreateDirectory(outputDirectory);
    logger.LogInformation("");

    // Step 5: Create solution model from parsed PlantUML
    logger.LogInformation("Step 5: Building solution model...");
    var solutionModel = await solutionModelFactory.CreateAsync(
        plantUmlSolutionModel,
        solutionName,
        outputDirectory,
        CancellationToken.None);
    logger.LogInformation("");

    // Step 6: Generate the solution
    logger.LogInformation("Step 6: Generating solution artifacts...");
    await artifactGenerator.GenerateAsync(solutionModel);
    logger.LogInformation("");

    logger.LogInformation("===========================================");
    logger.LogInformation("  Solution created successfully!");
    logger.LogInformation("===========================================");
    logger.LogInformation("");
    logger.LogInformation("Output location: {OutputDirectory}", Path.Combine(outputDirectory, solutionName));
    logger.LogInformation("");
    logger.LogInformation("To build the solution:");
    logger.LogInformation("  cd {OutputDirectory}\\{SolutionName} && dotnet build", outputDirectory, solutionName);

    return 0;
}
catch (Exception ex)
{
    logger.LogError(ex, "Error creating solution from sequence diagram");
    return 1;
}
