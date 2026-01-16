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
using Endpoint.Engineering.ALaCarte;
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

    [Option('u', "url", Required = false,
        HelpText = "Git repository URL to clone and parse (supports GitHub, GitLab, Bitbucket, Azure DevOps, Gitea, and private git hosts)")]
    public string? Url { get; set; }

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
    private readonly ICommandService _commandService;

    public CodeParseRequestHandler(
        ILogger<CodeParseRequestHandler> logger,
        ICodeParser codeParser,
        IClipboardService clipboardService,
        ICommandService commandService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _codeParser = codeParser ?? throw new ArgumentNullException(nameof(codeParser));
        _clipboardService = clipboardService ?? throw new ArgumentNullException(nameof(clipboardService));
        _commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
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

        string? tempDirectory = null;
        List<string> directories;

        try
        {
            // Check if URL is provided - if so, clone the repository
            if (!string.IsNullOrWhiteSpace(request.Url))
            {
                var cloneResult = CloneRepository(request.Url);
                if (cloneResult == null)
                {
                    return;
                }

                tempDirectory = cloneResult.Value.TempDirectory;
                directories = [cloneResult.Value.ParseDirectory];
            }
            else
            {
                // Parse directories - default to current directory if not specified
                directories = ParseDirectories(request.Directories);
            }

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
        finally
        {
            // Clean up temp directory if it was created
            if (!string.IsNullOrEmpty(tempDirectory) && Directory.Exists(tempDirectory))
            {
                try
                {
                    Directory.Delete(tempDirectory, recursive: true);
                    _logger.LogInformation("Cleaned up temporary directory: {TempDirectory}", tempDirectory);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to clean up temporary directory: {TempDirectory}", tempDirectory);
                }
            }
        }
    }

    /// <summary>
    /// Clones a git repository to a temporary directory.
    /// </summary>
    /// <param name="url">The git repository URL (can be a direct clone URL or a tree URL).</param>
    /// <returns>A tuple with the temp directory and the directory to parse, or null if cloning fails.</returns>
    private (string TempDirectory, string ParseDirectory)? CloneRepository(string url)
    {
        // Try to parse as a tree URL (e.g., GitHub/GitLab folder URL)
        var parsedUrl = GitUrlParser.Parse(url);

        string repoUrl;
        string? branch = null;
        string? folderPath = null;

        if (parsedUrl != null)
        {
            repoUrl = parsedUrl.Value.RepositoryUrl;
            branch = parsedUrl.Value.Branch;
            folderPath = parsedUrl.Value.FolderPath;

            _logger.LogInformation(
                "Parsed URL - Repository: {Url}, Branch: {Branch}, Folder: {Path}",
                repoUrl,
                branch,
                string.IsNullOrEmpty(folderPath) ? "(root)" : folderPath);
        }
        else
        {
            // Assume it's a direct clone URL
            repoUrl = url;
            _logger.LogInformation("Using URL as direct clone URL: {Url}", repoUrl);
        }

        // Create temp directory
        var tempDirectory = Path.Combine(Path.GetTempPath(), $"code-parse-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDirectory);

        _logger.LogInformation("Cloning repository to temporary directory: {TempDirectory}", tempDirectory);

        // Build git clone command
        var cloneCommand = $"git clone --depth 1";

        if (!string.IsNullOrEmpty(branch))
        {
            cloneCommand += $" --branch {branch}";
        }

        cloneCommand += $" \"{repoUrl}\" \"{tempDirectory}\"";

        // Execute git clone
        var exitCode = _commandService.Start(cloneCommand);

        if (exitCode != 0)
        {
            _logger.LogError("Failed to clone repository. Exit code: {ExitCode}", exitCode);

            // Clean up temp directory on failure
            try
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
            catch
            {
                // Ignore cleanup errors
            }

            return null;
        }

        _logger.LogInformation("Successfully cloned repository");

        // Determine the directory to parse
        var parseDirectory = tempDirectory;
        if (!string.IsNullOrEmpty(folderPath))
        {
            parseDirectory = Path.Combine(tempDirectory, folderPath);
            if (!Directory.Exists(parseDirectory))
            {
                _logger.LogError("Folder path not found in repository: {FolderPath}", folderPath);

                // Clean up temp directory on failure
                try
                {
                    Directory.Delete(tempDirectory, recursive: true);
                }
                catch
                {
                    // Ignore cleanup errors
                }

                return null;
            }
        }

        return (tempDirectory, parseDirectory);
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
