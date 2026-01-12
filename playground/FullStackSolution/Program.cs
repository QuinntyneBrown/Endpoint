// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint;
using Endpoint.Artifacts.Abstractions;
using Endpoint.DotNet;
using Endpoint.DotNet.Artifacts.Projects;
using Endpoint.DotNet.Artifacts.Projects.Factories;
using Endpoint.DotNet.Artifacts.Solutions;
using Endpoint.DotNet.Artifacts.Solutions.Factories;
using Endpoint.DotNet.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.CommandLine.Invocation;

// Parse command line arguments
var solutionNameOption = new Option<string>(
    "--name",
    description: "The name of the solution to create")
{
    IsRequired = true
};

var directoryOption = new Option<string>(
    "--directory",
    description: "The directory where the solution will be created",
    getDefaultValue: () => Environment.CurrentDirectory);

var rootCommand = new RootCommand("FullStack Solution Generator - Creates a complete solution with Core, Infrastructure, and API projects")
{
    solutionNameOption,
    directoryOption
};

rootCommand.SetHandler(async (InvocationContext context) =>
{
    var solutionName = context.ParseResult.GetValueForOption(solutionNameOption)!;
    var directory = context.ParseResult.GetValueForOption(directoryOption)!;

    // Setup dependency injection
    var services = new ServiceCollection();

    // Add logging
    services.AddLogging(builder =>
    {
        builder.AddConsole();
        builder.SetMinimumLevel(LogLevel.Information);
    });

    // Add core services
    services.AddCoreServices(typeof(IArtifactGenerator).Assembly);

    // Add DotNet services
    services.AddDotNetServices();

    var serviceProvider = services.BuildServiceProvider();

    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
    var solutionFactory = serviceProvider.GetRequiredService<ISolutionFactory>();
    var projectFactory = serviceProvider.GetRequiredService<IProjectFactory>();
    var artifactGenerator = serviceProvider.GetRequiredService<IArtifactGenerator>();

    try
    {
        logger.LogInformation("Creating solution: {SolutionName} in {Directory}", solutionName, directory);

        // Create the solution model
        var solution = new SolutionModel(solutionName, directory);

        // Create src folder projects
        logger.LogInformation("Creating Core project...");
        var coreProject = await projectFactory.CreateCore(solutionName, solution.SrcDirectory);
        solution.Projects.Add(coreProject);

        logger.LogInformation("Creating Infrastructure project...");
        var infrastructureProject = await projectFactory.CreateInfrastructure(solutionName, solution.SrcDirectory);

        // Add reference from Infrastructure to Core
        infrastructureProject.References.Add(Path.Combine("..", solutionName + ".Core", solutionName + ".Core.csproj"));

        solution.Projects.Add(infrastructureProject);

        logger.LogInformation("Creating API project...");
        var apiProject = await projectFactory.CreateApi(solutionName, solution.SrcDirectory);

        // Add reference from API to Infrastructure
        apiProject.References.Add(Path.Combine("..", solutionName + ".Infrastructure", solutionName + ".Infrastructure.csproj"));

        solution.Projects.Add(apiProject);

        // Create tests folder projects
        logger.LogInformation("Creating test projects...");

        var coreTestProject = await projectFactory.Create("xunit", solutionName + ".Core.Tests", solution.TestDirectory);
        coreTestProject.References.Add(Path.Combine("..", "..", "src", solutionName + ".Core", solutionName + ".Core.csproj"));
        solution.Projects.Add(coreTestProject);

        var infrastructureTestProject = await projectFactory.Create("xunit", solutionName + ".Infrastructure.Tests", solution.TestDirectory);
        infrastructureTestProject.References.Add(Path.Combine("..", "..", "src", solutionName + ".Infrastructure", solutionName + ".Infrastructure.csproj"));
        solution.Projects.Add(infrastructureTestProject);

        var apiTestProject = await projectFactory.Create("xunit", solutionName + ".Api.Tests", solution.TestDirectory);
        apiTestProject.References.Add(Path.Combine("..", "..", "src", solutionName + ".Api", solutionName + ".Api.csproj"));
        solution.Projects.Add(apiTestProject);

        // Generate the solution and all projects
        logger.LogInformation("Generating solution files...");
        await artifactGenerator.GenerateAsync(solution);

        logger.LogInformation("Solution created successfully at: {SolutionPath}", solution.SolutionPath);

        context.ExitCode = 0;
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error creating solution");
        context.ExitCode = 1;
    }
});

return await rootCommand.InvokeAsync(args);
