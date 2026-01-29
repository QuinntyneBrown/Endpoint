// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.CommandLine;
using System.IO.Abstractions;
using Endpoint.Engineering.Testing.Cli.Services;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.Testing.Cli.Commands;

public class UnitTestsGenerateCommand
{
    private readonly ILogger<UnitTestsGenerateCommand> _logger;
    private readonly IFileSystem _fileSystem;
    private readonly IAngularTestGeneratorService _testGeneratorService;

    public UnitTestsGenerateCommand(
        ILogger<UnitTestsGenerateCommand> logger,
        IFileSystem fileSystem,
        IAngularTestGeneratorService testGeneratorService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _testGeneratorService = testGeneratorService ?? throw new ArgumentNullException(nameof(testGeneratorService));
    }

    public Command Create()
    {
        var command = new Command("unit-tests-generate", "Generates Jest unit tests for Angular/TypeScript files");

        var pathArgument = new Argument<string>(
            name: "path",
            description: "Path to an Angular/TypeScript file or folder containing Angular code");

        var outputOption = new Option<string?>(
            aliases: ["-o", "--output"],
            description: "Output directory for generated test files (defaults to same directory as source)");

        var overwriteOption = new Option<bool>(
            aliases: ["--overwrite"],
            description: "Overwrite existing test files",
            getDefaultValue: () => false);

        var dryRunOption = new Option<bool>(
            aliases: ["--dry-run"],
            description: "Preview what would be generated without writing files",
            getDefaultValue: () => false);

        command.AddArgument(pathArgument);
        command.AddOption(outputOption);
        command.AddOption(overwriteOption);
        command.AddOption(dryRunOption);

        command.SetHandler(ExecuteAsync, pathArgument, outputOption, overwriteOption, dryRunOption);

        return command;
    }

    public async Task ExecuteAsync(string path, string? outputDirectory, bool overwrite, bool dryRun)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path cannot be null or empty.", nameof(path));
        }

        var fullPath = _fileSystem.Path.GetFullPath(path);

        if (!_fileSystem.File.Exists(fullPath) && !_fileSystem.Directory.Exists(fullPath))
        {
            throw new FileNotFoundException($"Path not found: {fullPath}");
        }

        try
        {
            _logger.LogInformation("Generating unit tests for: {Path}", fullPath);

            var files = GetTypeScriptFiles(fullPath);

            if (files.Count == 0)
            {
                _logger.LogWarning("No TypeScript files found in: {Path}", fullPath);
                Console.WriteLine("No TypeScript files found.");
                return;
            }

            _logger.LogInformation("Found {Count} TypeScript file(s) to process", files.Count);

            var generatedCount = 0;
            var skippedCount = 0;

            foreach (var file in files)
            {
                var result = await _testGeneratorService.GenerateTestAsync(file, outputDirectory, overwrite, dryRun);

                if (result.Generated)
                {
                    generatedCount++;
                    if (dryRun)
                    {
                        Console.WriteLine($"[DRY RUN] Would generate: {result.TestFilePath}");
                    }
                    else
                    {
                        Console.WriteLine($"Generated: {result.TestFilePath}");
                    }
                }
                else
                {
                    skippedCount++;
                    _logger.LogDebug("Skipped: {File} - {Reason}", file, result.SkipReason);
                }
            }

            Console.WriteLine();
            Console.WriteLine($"Summary: {generatedCount} test file(s) generated, {skippedCount} skipped");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating unit tests");
            Console.Error.WriteLine($"Error: {ex.Message}");
            throw;
        }
    }

    private List<string> GetTypeScriptFiles(string path)
    {
        var files = new List<string>();

        if (_fileSystem.File.Exists(path))
        {
            if (IsAngularSourceFile(path))
            {
                files.Add(path);
            }
        }
        else if (_fileSystem.Directory.Exists(path))
        {
            var tsFiles = _fileSystem.Directory
                .GetFiles(path, "*.ts", SearchOption.AllDirectories)
                .Where(IsAngularSourceFile)
                .ToList();

            files.AddRange(tsFiles);
        }

        return files;
    }

    private bool IsAngularSourceFile(string filePath)
    {
        var fileName = _fileSystem.Path.GetFileName(filePath);

        // Skip test files, spec files, and module files
        if (fileName.EndsWith(".spec.ts", StringComparison.OrdinalIgnoreCase) ||
            fileName.EndsWith(".test.ts", StringComparison.OrdinalIgnoreCase) ||
            fileName.EndsWith(".module.ts", StringComparison.OrdinalIgnoreCase) ||
            fileName.EndsWith(".routes.ts", StringComparison.OrdinalIgnoreCase) ||
            fileName.EndsWith(".config.ts", StringComparison.OrdinalIgnoreCase) ||
            fileName.EndsWith(".d.ts", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // Include Angular source files
        return fileName.EndsWith(".component.ts", StringComparison.OrdinalIgnoreCase) ||
               fileName.EndsWith(".service.ts", StringComparison.OrdinalIgnoreCase) ||
               fileName.EndsWith(".directive.ts", StringComparison.OrdinalIgnoreCase) ||
               fileName.EndsWith(".pipe.ts", StringComparison.OrdinalIgnoreCase) ||
               fileName.EndsWith(".guard.ts", StringComparison.OrdinalIgnoreCase) ||
               fileName.EndsWith(".interceptor.ts", StringComparison.OrdinalIgnoreCase) ||
               fileName.EndsWith(".resolver.ts", StringComparison.OrdinalIgnoreCase) ||
               fileName.EndsWith(".validator.ts", StringComparison.OrdinalIgnoreCase) ||
               fileName.EndsWith(".ts", StringComparison.OrdinalIgnoreCase);
    }
}
