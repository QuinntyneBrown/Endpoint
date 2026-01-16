// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Endpoint.Engineering.AI.Services;
using Endpoint.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.Cli.Commands;

[Verb("code-parse")]
public class CodeParseRequest : IRequest
{
    [Option('n', "name", HelpText = "Name for the output file (optional)")]
    public string? Name { get; set; }

    [Option('d', "directory", Required = false,
        HelpText = "Comma-separated list of directories to parse (defaults to current directory)")]
    public string? Directories { get; set; }

    [Option('o', "output", Required = false, HelpText = "Output file path (optional, prints to console if not specified)")]
    public string? Output { get; set; }

    [Option('e', "efficiency", Required = false, Default = 50,
        HelpText = "Token efficiency level: 0 (verbatim/full code) to 100 (minimal). Values in between produce progressively smaller output.")]
    public int Efficiency { get; set; } = 50;

    [Option("ignore-tests", Required = false, Default = false,
        HelpText = "Ignore test files and test projects (xUnit, NUnit, Jest, pytest, etc.)")]
    public bool IgnoreTests { get; set; } = false;

    [Option("tests-only", Required = false, Default = false,
        HelpText = "Only parse test files and test projects")]
    public bool TestsOnly { get; set; } = false;
}

public class CodeParseRequestHandler : IRequestHandler<CodeParseRequest>
{
    private readonly ILogger<CodeParseRequestHandler> _logger;
    private readonly ICodeParser _codeParser;
    private readonly IClipboardService _clipboardService;

    public CodeParseRequestHandler(
        ILogger<CodeParseRequestHandler> logger,
        ICodeParser codeParser,
        IClipboardService clipboardService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _codeParser = codeParser ?? throw new ArgumentNullException(nameof(codeParser));
        _clipboardService = clipboardService ?? throw new ArgumentNullException(nameof(clipboardService));
    }

    public async Task Handle(CodeParseRequest request, CancellationToken cancellationToken)
    {
        // Validate options
        if (request.IgnoreTests && request.TestsOnly)
        {
            _logger.LogWarning("Both --ignore-tests and --tests-only specified. Using --tests-only.");
        }

        // Validate and clamp efficiency to 0-100 range
        var efficiency = Math.Clamp(request.Efficiency, 0, 100);
        if (efficiency != request.Efficiency)
        {
            _logger.LogWarning("Efficiency value {Value} clamped to {ClampedValue} (valid range: 0-100)", request.Efficiency, efficiency);
        }

        var options = new CodeParseOptions
        {
            Efficiency = efficiency,
            IgnoreTests = request.IgnoreTests && !request.TestsOnly,
            TestsOnly = request.TestsOnly
        };

        // Parse directories - default to current directory if not specified
        var directories = ParseDirectories(request.Directories);

        var filterMode = request.TestsOnly ? " (tests only)" :
                         request.IgnoreTests ? " (ignoring tests)" : "";

        _logger.LogInformation("Parsing {DirectoryCount} directory(s){FilterMode} with efficiency: {Efficiency}",
            directories.Count, filterMode, options.Efficiency);

        // Parse all directories and merge results
        var mergedSummary = new CodeSummary
        {
            RootDirectory = directories.Count == 1 ? directories[0] : string.Join(", ", directories.Select(Path.GetFileName)),
            Efficiency = options.Efficiency
        };

        foreach (var directory in directories)
        {
            if (!Directory.Exists(directory))
            {
                _logger.LogWarning("Directory not found: {Directory}", directory);
                continue;
            }

            _logger.LogInformation("Parsing directory: {Directory}", directory);
            var summary = await _codeParser.ParseDirectoryAsync(directory, options, cancellationToken);

            // Merge files into combined summary
            mergedSummary.Files.AddRange(summary.Files);
            mergedSummary.TotalFiles += summary.TotalFiles;
        }

        var output = mergedSummary.ToLlmString();

        if (!string.IsNullOrEmpty(request.Output))
        {
            var outputPath = request.Output;
            if (!string.IsNullOrEmpty(request.Name))
            {
                outputPath = Path.Combine(
                    Path.GetDirectoryName(request.Output) ?? directories[0],
                    $"{request.Name}.txt");
            }

            await File.WriteAllTextAsync(outputPath, output, cancellationToken);
            _logger.LogInformation("Output written to: {OutputPath}", outputPath);
        }
        else
        {
            Console.WriteLine(output);
        }

        _logger.LogInformation("Parsed {FileCount} files from {DirectoryCount} directory(s) with efficiency {Efficiency} ({OutputLength} chars)",
            mergedSummary.Files.Count, directories.Count, options.Efficiency, output.Length);

        // Copy result to clipboard
        _clipboardService.SetText(output);
        _logger.LogInformation("Result copied to clipboard");
    }

    private static List<string> ParseDirectories(string? directoriesArg)
    {
        if (string.IsNullOrWhiteSpace(directoriesArg))
        {
            return [Environment.CurrentDirectory];
        }

        var directories = directoriesArg
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(d => Path.GetFullPath(d.Trim()))
            .Distinct()
            .ToList();

        return directories.Count > 0 ? directories : [Environment.CurrentDirectory];
    }
}
