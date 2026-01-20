// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO.Abstractions;
using Endpoint.DotNet.SharedLibrary.Configuration;
using Endpoint.DotNet.SharedLibrary.Services.Generators;
using Endpoint.Internal;
using Microsoft.Extensions.Logging;

namespace Endpoint.DotNet.SharedLibrary.Services;

/// <summary>
/// Orchestrates the generation of shared library projects.
/// </summary>
public class SharedLibraryGeneratorService : ISharedLibraryGeneratorService
{
    private readonly ILogger<SharedLibraryGeneratorService> _logger;
    private readonly IFileSystem _fileSystem;
    private readonly ICommandService _commandService;
    private readonly IEnumerable<IProjectGenerator> _projectGenerators;

    public SharedLibraryGeneratorService(
        ILogger<SharedLibraryGeneratorService> logger,
        IFileSystem fileSystem,
        ICommandService commandService,
        IEnumerable<IProjectGenerator> projectGenerators)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
        _projectGenerators = projectGenerators ?? throw new ArgumentNullException(nameof(projectGenerators));
    }

    /// <inheritdoc />
    public async Task GenerateAsync(SharedLibraryConfig config, CancellationToken cancellationToken = default)
    {
        var solutionName = config.Solution.Name;
        var libraryName = config.Solution.LibraryName ?? "Shared";
        var outputPath = Path.GetFullPath(config.Solution.OutputPath);
        var solutionDirectory = _fileSystem.Path.Combine(outputPath, solutionName);
        var srcDirectory = _fileSystem.Path.Combine(solutionDirectory, "src");
        var sharedDirectory = _fileSystem.Path.Combine(srcDirectory, libraryName);

        _logger.LogInformation("Generating shared library '{SolutionName}' at '{Path}' with library name '{LibraryName}'", solutionName, solutionDirectory, libraryName);

        // Create directory structure
        _fileSystem.Directory.CreateDirectory(solutionDirectory);
        _fileSystem.Directory.CreateDirectory(srcDirectory);
        _fileSystem.Directory.CreateDirectory(sharedDirectory);

        // Create solution file
        await CreateSolutionFileAsync(solutionDirectory, solutionName, cancellationToken);

        var context = new GeneratorContext
        {
            Config = config,
            SolutionDirectory = solutionDirectory,
            SrcDirectory = srcDirectory,
            SharedDirectory = sharedDirectory,
            SolutionName = solutionName,
            Namespace = config.Solution.Namespace ?? solutionName,
            TargetFramework = config.Solution.TargetFramework,
            LibraryName = libraryName,
        };

        // Run each generator that's applicable
        foreach (var generator in _projectGenerators)
        {
            if (generator.ShouldGenerate(config))
            {
                _logger.LogInformation("Running generator: {GeneratorName}", generator.GetType().Name);
                await generator.GenerateAsync(context, cancellationToken);
            }
        }

        _logger.LogInformation("Shared library generation complete.");
    }

    /// <inheritdoc />
    public async Task<GenerationPreview> PreviewAsync(SharedLibraryConfig config, CancellationToken cancellationToken = default)
    {
        var preview = new GenerationPreview();

        var solutionName = config.Solution.Name;
        var libraryName = config.Solution.LibraryName ?? "Shared";
        var outputPath = Path.GetFullPath(config.Solution.OutputPath);
        var solutionDirectory = _fileSystem.Path.Combine(outputPath, solutionName);
        var srcDirectory = _fileSystem.Path.Combine(solutionDirectory, "src");
        var sharedDirectory = _fileSystem.Path.Combine(srcDirectory, libraryName);

        var context = new GeneratorContext
        {
            Config = config,
            SolutionDirectory = solutionDirectory,
            SrcDirectory = srcDirectory,
            SharedDirectory = sharedDirectory,
            SolutionName = solutionName,
            Namespace = config.Solution.Namespace ?? solutionName,
            TargetFramework = config.Solution.TargetFramework,
            IsPreview = true,
            LibraryName = libraryName,
        };

        // Add solution file
        preview.Files.Add($"{solutionName}/{solutionName}.sln");

        // Collect preview from each applicable generator
        foreach (var generator in _projectGenerators)
        {
            if (generator.ShouldGenerate(config))
            {
                var generatorPreview = await generator.PreviewAsync(context, cancellationToken);
                preview.Projects.AddRange(generatorPreview.Projects);
                preview.Files.AddRange(generatorPreview.Files);
            }
        }

        return preview;
    }

    private async Task CreateSolutionFileAsync(string solutionDirectory, string solutionName, CancellationToken cancellationToken)
    {
        var slnPath = _fileSystem.Path.Combine(solutionDirectory, $"{solutionName}.sln");

        if (!_fileSystem.File.Exists(slnPath))
        {
            _commandService.Start($"dotnet new sln -n {solutionName}", solutionDirectory);
            _logger.LogInformation("Created solution file: {SlnPath}", slnPath);
        }
    }
}
