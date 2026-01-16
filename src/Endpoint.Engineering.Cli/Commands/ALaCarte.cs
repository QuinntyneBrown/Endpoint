// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Endpoint.Engineering.ALaCarte;
using Endpoint.Engineering.ALaCarte.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.Cli.Commands;

[Verb("a-la-carte", HelpText = "Clone git repositories, extract select folders, and create a new folder structure based on mapping configuration.")]
public class ALaCarteCommandRequest : IRequest
{
    [Option('c', "config", Required = false,
        HelpText = "Path to a JSON configuration file containing the ALaCarte request.")]
    public string? ConfigFile { get; set; }

    [Option('d', "directory", Required = false,
        HelpText = "Output directory where files will be copied to. Defaults to current directory.")]
    public string? Directory { get; set; }

    [Option('o', "output-type", Required = false, Default = "NotSpecified",
        HelpText = "Output type: DotNetSolution, MixDotNetSolutionWithOtherFolders, or NotSpecified (default).")]
    public string OutputType { get; set; } = "NotSpecified";

    [Option('s', "solution-name", Required = false, Default = "ALaCarte.sln",
        HelpText = "Name of the .NET solution to create (default: ALaCarte.sln).")]
    public string SolutionName { get; set; } = "ALaCarte.sln";

    [Option('u', "url", Required = false,
        HelpText = "Git repository URL (for single repository usage).")]
    public string? Url { get; set; }

    [Option('b', "branch", Required = false, Default = "main",
        HelpText = "Branch to clone (for single repository usage, default: main).")]
    public string Branch { get; set; } = "main";

    [Option('f', "folders", Required = false, Separator = ';',
        HelpText = "Folder mappings in 'from:to' format, separated by semicolons (for single repository usage). Example: 'src/lib:lib;src/core:core'.")]
    public IEnumerable<string>? Folders { get; set; }

    [Option('v', "verbose", Required = false, Default = false,
        HelpText = "Show verbose output.")]
    public bool Verbose { get; set; }

    [Option("no-color", Required = false, Default = false,
        HelpText = "Disable colorized output.")]
    public bool NoColor { get; set; }
}

public class ALaCarteCommandRequestHandler : IRequestHandler<ALaCarteCommandRequest>
{
    private readonly ILogger<ALaCarteCommandRequestHandler> _logger;
    private readonly IALaCarteService _alaCarteService;

    public ALaCarteCommandRequestHandler(
        ILogger<ALaCarteCommandRequestHandler> logger,
        IALaCarteService alaCarteService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _alaCarteService = alaCarteService ?? throw new ArgumentNullException(nameof(alaCarteService));
    }

    public async Task Handle(ALaCarteCommandRequest request, CancellationToken cancellationToken)
    {
        WriteHeader(request.NoColor);

        try
        {
            ALaCarteRequest alaCarteRequest;

            if (!string.IsNullOrEmpty(request.ConfigFile))
            {
                // Load configuration from file
                alaCarteRequest = await LoadConfigurationFromFileAsync(request.ConfigFile);

                // Override with command line options if provided
                if (!string.IsNullOrEmpty(request.Directory))
                {
                    alaCarteRequest.Directory = request.Directory;
                }

                if (request.OutputType != "NotSpecified")
                {
                    alaCarteRequest.OutputType = ParseOutputType(request.OutputType);
                }

                if (request.SolutionName != "ALaCarte.sln")
                {
                    alaCarteRequest.SolutionName = request.SolutionName;
                }
            }
            else if (!string.IsNullOrEmpty(request.Url))
            {
                // Build configuration from command line options
                alaCarteRequest = BuildRequestFromCommandLine(request);
            }
            else
            {
                WriteError("Either --config or --url must be specified.", request.NoColor);
                Environment.ExitCode = 1;
                return;
            }

            // Ensure directory is set
            if (string.IsNullOrEmpty(alaCarteRequest.Directory))
            {
                alaCarteRequest.Directory = Environment.CurrentDirectory;
            }

            if (request.Verbose)
            {
                DisplayConfiguration(alaCarteRequest, request.NoColor);
            }

            // Process the request
            Console.WriteLine("Processing ALaCarte request...");
            Console.WriteLine();

            var result = await _alaCarteService.ProcessAsync(alaCarteRequest, cancellationToken);

            DisplayResult(result, request.Verbose, request.NoColor);

            if (!result.Success)
            {
                Environment.ExitCode = 1;
            }
        }
        catch (FileNotFoundException ex)
        {
            WriteError($"Configuration file not found: {ex.FileName}", request.NoColor);
            Environment.ExitCode = 1;
        }
        catch (JsonException ex)
        {
            WriteError($"Invalid JSON in configuration file: {ex.Message}", request.NoColor);
            Environment.ExitCode = 1;
        }
        catch (Exception ex)
        {
            WriteError($"Unexpected error: {ex.Message}", request.NoColor);
            _logger.LogError(ex, "Error executing ALaCarte command");
            Environment.ExitCode = 1;
        }
    }

    private async Task<ALaCarteRequest> LoadConfigurationFromFileAsync(string configFilePath)
    {
        if (!File.Exists(configFilePath))
        {
            throw new FileNotFoundException("Configuration file not found.", configFilePath);
        }

        var json = await File.ReadAllTextAsync(configFilePath);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var config = JsonSerializer.Deserialize<ALaCarteRequest>(json, options)
            ?? throw new JsonException("Failed to deserialize configuration file.");

        return config;
    }

    private ALaCarteRequest BuildRequestFromCommandLine(ALaCarteCommandRequest request)
    {
        var alaCarteRequest = new ALaCarteRequest
        {
            Directory = request.Directory ?? Environment.CurrentDirectory,
            OutputType = ParseOutputType(request.OutputType),
            SolutionName = request.SolutionName
        };

        var repoConfig = new RepositoryConfiguration
        {
            Url = request.Url!,
            Branch = request.Branch
        };

        // Parse folder mappings
        if (request.Folders != null)
        {
            foreach (var folder in request.Folders)
            {
                var parts = folder.Split(':');
                if (parts.Length == 2)
                {
                    repoConfig.Folders.Add(new FolderConfiguration
                    {
                        From = parts[0].Trim(),
                        To = parts[1].Trim()
                    });
                }
                else if (parts.Length == 1)
                {
                    // Use same path for from and to
                    repoConfig.Folders.Add(new FolderConfiguration
                    {
                        From = parts[0].Trim(),
                        To = parts[0].Trim()
                    });
                }
            }
        }

        alaCarteRequest.Repositories.Add(repoConfig);
        return alaCarteRequest;
    }

    private static OutputType ParseOutputType(string outputType)
    {
        return outputType.ToLowerInvariant() switch
        {
            "dotnetsolution" => OutputType.DotNetSolution,
            "mixdotnetsolutionwithotherfolders" => OutputType.MixDotNetSolutionWithOtherFolders,
            _ => OutputType.NotSpecified
        };
    }

    private static void WriteHeader(bool noColor)
    {
        Console.WriteLine();
        if (!noColor)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
        }
        Console.WriteLine("=== ALaCarte ===");
        if (!noColor)
        {
            Console.ResetColor();
        }
        Console.WriteLine();
    }

    private static void WriteError(string message, bool noColor)
    {
        if (!noColor)
        {
            Console.ForegroundColor = ConsoleColor.Red;
        }
        Console.WriteLine($"Error: {message}");
        if (!noColor)
        {
            Console.ResetColor();
        }
    }

    private static void DisplayConfiguration(ALaCarteRequest request, bool noColor)
    {
        if (!noColor)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
        }
        Console.WriteLine("=== Configuration ===");
        if (!noColor)
        {
            Console.ResetColor();
        }
        Console.WriteLine();

        Console.WriteLine($"Output Directory: {request.Directory}");
        Console.WriteLine($"Output Type: {request.OutputType}");
        Console.WriteLine($"Solution Name: {request.SolutionName}");
        Console.WriteLine($"Repositories: {request.Repositories.Count}");
        Console.WriteLine();

        foreach (var repo in request.Repositories)
        {
            Console.WriteLine($"  Repository: {repo.Url}");
            Console.WriteLine($"    Branch: {repo.Branch}");
            Console.WriteLine($"    Folder Mappings:");
            foreach (var folder in repo.Folders)
            {
                Console.WriteLine($"      {folder.From} -> {folder.To}");
            }
            Console.WriteLine();
        }
    }

    private static void DisplayResult(ALaCarteResult result, bool verbose, bool noColor)
    {
        if (!noColor)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
        }
        Console.WriteLine("=== Results ===");
        if (!noColor)
        {
            Console.ResetColor();
        }
        Console.WriteLine();

        Console.WriteLine($"Output Directory: {result.OutputDirectory}");

        if (result.SolutionPath != null)
        {
            Console.WriteLine($"Solution Created: {result.SolutionPath}");
        }

        if (result.CsprojFiles.Count > 0)
        {
            Console.WriteLine($"Projects Found: {result.CsprojFiles.Count}");
            if (verbose)
            {
                foreach (var csproj in result.CsprojFiles)
                {
                    Console.WriteLine($"  - {csproj}");
                }
            }
        }

        if (result.AngularWorkspacesCreated.Count > 0)
        {
            if (!noColor)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
            }
            Console.WriteLine($"Angular Workspaces Created: {result.AngularWorkspacesCreated.Count}");
            if (!noColor)
            {
                Console.ResetColor();
            }
            foreach (var workspace in result.AngularWorkspacesCreated)
            {
                Console.WriteLine($"  - {workspace}");
            }
        }

        // Display warnings
        if (result.Warnings.Count > 0)
        {
            Console.WriteLine();
            if (!noColor)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
            }
            Console.WriteLine($"Warnings ({result.Warnings.Count}):");
            if (!noColor)
            {
                Console.ResetColor();
            }
            foreach (var warning in result.Warnings)
            {
                Console.WriteLine($"  - {warning}");
            }
        }

        // Display errors
        if (result.Errors.Count > 0)
        {
            Console.WriteLine();
            if (!noColor)
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }
            Console.WriteLine($"Errors ({result.Errors.Count}):");
            if (!noColor)
            {
                Console.ResetColor();
            }
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"  - {error}");
            }
        }

        // Summary
        Console.WriteLine();
        if (result.Success)
        {
            if (!noColor)
            {
                Console.ForegroundColor = ConsoleColor.Green;
            }
            Console.WriteLine("ALaCarte operation completed successfully.");
            if (!noColor)
            {
                Console.ResetColor();
            }
        }
        else
        {
            if (!noColor)
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }
            Console.WriteLine("ALaCarte operation completed with errors.");
            if (!noColor)
            {
                Console.ResetColor();
            }
        }
        Console.WriteLine();
    }
}
