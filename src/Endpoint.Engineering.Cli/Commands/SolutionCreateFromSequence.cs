// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Endpoint.Artifacts.Abstractions;
using Endpoint.DotNet.Artifacts.PlantUml.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.Cli.Commands;

[Verb("solution-create-from-sequence")]
public class SolutionCreateFromSequenceRequest : IRequest {
    [Option('n',"name")]
    public string Name { get; set; }


    [Option('d', Required = false)]
    public string Directory { get; set; } = System.Environment.CurrentDirectory;
}

public class SolutionCreateFromSequenceRequestHandler : IRequestHandler<SolutionCreateFromSequenceRequest>
{
    private readonly ILogger<SolutionCreateFromSequenceRequestHandler> _logger;
    private readonly IUserInputService _userInputService;
    private readonly ISequenceToSolutionPlantUmlService _sequenceToSolutionService;
    private readonly IPlantUmlParserService _plantUmlParserService;
    private readonly IPlantUmlSolutionModelFactory _plantUmlSolutionModelFactory;
    private readonly IArtifactGenerator _artifactGenerator;
    private readonly IFileSystem _fileSystem;

    public SolutionCreateFromSequenceRequestHandler(
        ILogger<SolutionCreateFromSequenceRequestHandler> logger,
        IUserInputService userInputService,
        ISequenceToSolutionPlantUmlService sequenceToSolutionService,
        IPlantUmlParserService plantUmlParserService,
        IPlantUmlSolutionModelFactory plantUmlSolutionModelFactory,
        IArtifactGenerator artifactGenerator,
        IFileSystem fileSystem)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _userInputService = userInputService ?? throw new ArgumentNullException(nameof(userInputService));
        _sequenceToSolutionService = sequenceToSolutionService ?? throw new ArgumentNullException(nameof(sequenceToSolutionService));
        _plantUmlParserService = plantUmlParserService ?? throw new ArgumentNullException(nameof(plantUmlParserService));
        _plantUmlSolutionModelFactory = plantUmlSolutionModelFactory ?? throw new ArgumentNullException(nameof(plantUmlSolutionModelFactory));
        _artifactGenerator = artifactGenerator ?? throw new ArgumentNullException(nameof(artifactGenerator));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    public async Task Handle(SolutionCreateFromSequenceRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting solution creation from sequence diagram: {Name}", request.Name);

        // Step 1: Get sequence diagram input from user
        _logger.LogInformation("Requesting PlantUML sequence diagram input from user...");
        Console.WriteLine();
        Console.WriteLine("=================================================================");
        Console.WriteLine("Please enter your PlantUML sequence diagram.");
        Console.WriteLine("Format:");
        Console.WriteLine("  ' {type of service}");
        Console.WriteLine("  participant \"{project name}\" as alias");
        Console.WriteLine();
        Console.WriteLine("Example:");
        Console.WriteLine("  @startuml");
        Console.WriteLine("  actor User as u");
        Console.WriteLine("  ' Angular");
        Console.WriteLine("  participant \"Admin Dashboard\" as admin");
        Console.WriteLine("  ' Worker");
        Console.WriteLine("  participant \"Email Sender\" as es");
        Console.WriteLine("  ' Microservice");
        Console.WriteLine("  participant \"Order Management\" as om");
        Console.WriteLine("  @enduml");
        Console.WriteLine();
        Console.WriteLine("Enter the diagram content (press Ctrl+D or Ctrl+Z when done):");
        Console.WriteLine("=================================================================");
        Console.WriteLine();

        // Read multiline input from user
        var sequenceDiagram = await ReadMultilineInputAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(sequenceDiagram))
        {
            _logger.LogError("No sequence diagram content provided");
            throw new InvalidOperationException("Sequence diagram content is required");
        }

        _logger.LogInformation("Received sequence diagram with {Length} characters", sequenceDiagram.Length);

        // Step 2: Generate valid solution PlantUML from sequence diagram
        _logger.LogInformation("Generating solution PlantUML specification from sequence diagram...");
        var solutionPlantUml = _sequenceToSolutionService.GenerateSolutionPlantUml(sequenceDiagram, request.Name);

        // Save the generated PlantUML for debugging/reference
        var tempDir = _fileSystem.Path.Combine(_fileSystem.Path.GetTempPath(), "endpoint-plantuml", request.Name);
        _fileSystem.Directory.CreateDirectory(tempDir);
        var plantUmlPath = _fileSystem.Path.Combine(tempDir, "generated-solution.puml");
        await _fileSystem.File.WriteAllTextAsync(plantUmlPath, solutionPlantUml, cancellationToken);
        _logger.LogInformation("Generated PlantUML saved to: {Path}", plantUmlPath);

        Console.WriteLine();
        Console.WriteLine("Generated PlantUML specification:");
        Console.WriteLine("=================================================================");
        Console.WriteLine(solutionPlantUml);
        Console.WriteLine("=================================================================");
        Console.WriteLine();

        // Step 3: Parse the generated PlantUML
        _logger.LogInformation("Parsing generated PlantUML specification...");
        var plantUmlModel = _plantUmlParserService.ParseContent(solutionPlantUml, plantUmlPath);
        
        var plantUmlSolutionModel = new DotNet.Artifacts.PlantUml.Models.PlantUmlSolutionModel
        {
            Name = request.Name,
            SourcePath = tempDir
        };
        plantUmlSolutionModel.Documents.Add(plantUmlModel);

        _logger.LogInformation("Parsed PlantUML: {EntityCount} entities, {EnumCount} enums",
            plantUmlSolutionModel.GetAllClasses().Count(),
            plantUmlSolutionModel.GetAllEnums().Count());

        // Step 4: Create solution model from parsed PlantUML
        _logger.LogInformation("Building solution model...");
        var outputDirectory = _fileSystem.Path.GetFullPath(request.Directory);
        var solutionModel = await _plantUmlSolutionModelFactory.CreateAsync(
            plantUmlSolutionModel,
            request.Name,
            outputDirectory,
            cancellationToken);

        // Step 5: Generate the solution using the artifact generator
        _logger.LogInformation("Generating solution: {SolutionPath}", solutionModel.SolutionPath);
        await _artifactGenerator.GenerateAsync(solutionModel);

        Console.WriteLine();
        Console.WriteLine("=================================================================");
        Console.WriteLine($"Solution created successfully at: {solutionModel.SolutionDirectory}");
        Console.WriteLine("=================================================================");
        
        _logger.LogInformation("Solution creation completed successfully");
    }

    private async Task<string> ReadMultilineInputAsync(CancellationToken cancellationToken)
    {
        var lines = new System.Collections.Generic.List<string>();
        string line;

        while ((line = Console.ReadLine()) != null)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            lines.Add(line);
        }

        return string.Join(Environment.NewLine, lines);
    }
}