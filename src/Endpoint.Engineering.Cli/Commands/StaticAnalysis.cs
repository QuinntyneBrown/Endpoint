// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Endpoint.Engineering.StaticAnalysis;
using Endpoint.Engineering.StaticAnalysis.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.Cli.Commands;

[Verb("static-analysis", HelpText = "Perform static analysis on a codebase based on specification rules.")]
public class StaticAnalysisRequest : IRequest
{
    [Option('p', "path", Required = false,
        HelpText = "Path to analyze (file or directory). Defaults to current directory.")]
    public string? Path { get; set; }

    [Option('v', "verbose", Required = false, Default = false,
        HelpText = "Show verbose output including informational messages.")]
    public bool Verbose { get; set; }

    [Option("json", Required = false, Default = false,
        HelpText = "Output results in JSON format.")]
    public bool Json { get; set; }

    [Option("fail-on-warning", Required = false, Default = false,
        HelpText = "Exit with error code if warnings are found.")]
    public bool FailOnWarning { get; set; }
}

public class StaticAnalysisRequestHandler : IRequestHandler<StaticAnalysisRequest>
{
    private readonly ILogger<StaticAnalysisRequestHandler> _logger;
    private readonly IStaticAnalysisService _staticAnalysisService;

    public StaticAnalysisRequestHandler(
        ILogger<StaticAnalysisRequestHandler> logger,
        IStaticAnalysisService staticAnalysisService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _staticAnalysisService = staticAnalysisService ?? throw new ArgumentNullException(nameof(staticAnalysisService));
    }

    public async Task Handle(StaticAnalysisRequest request, CancellationToken cancellationToken)
    {
        var path = request.Path ?? Environment.CurrentDirectory;

        // Determine project root
        var projectRoot = _staticAnalysisService.DetermineProjectRoot(path);

        if (projectRoot == null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Error: Unable to determine project root.");
            Console.WriteLine("The path must be within a Git repository, .NET solution, Angular workspace, or Node environment.");
            Console.ResetColor();
            Environment.ExitCode = 1;
            return;
        }

        var (rootDirectory, projectType) = projectRoot.Value;

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("=== Endpoint Static Analysis ===");
        Console.ResetColor();
        Console.WriteLine();
        Console.WriteLine($"Project Type: {projectType}");
        Console.WriteLine($"Root Directory: {rootDirectory}");
        Console.WriteLine();

        _logger.LogInformation("Starting static analysis on {ProjectType} at: {RootDirectory}", projectType, rootDirectory);

        AnalysisResult result;
        try
        {
            result = await _staticAnalysisService.AnalyzeAsync(path, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error during analysis: {ex.Message}");
            Console.ResetColor();
            Environment.ExitCode = 1;
            return;
        }

        if (request.Json)
        {
            OutputJson(result);
        }
        else
        {
            OutputConsole(result, request.Verbose);
        }

        // Set exit code based on results
        if (!result.Passed)
        {
            Environment.ExitCode = 1;
        }
        else if (request.FailOnWarning && result.Warnings.Count > 0)
        {
            Environment.ExitCode = 1;
        }
    }

    private static void OutputConsole(AnalysisResult result, bool verbose)
    {
        Console.WriteLine($"Files Analyzed: {result.TotalFilesAnalyzed}");
        Console.WriteLine($"Analysis Time: {result.Timestamp:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine();

        // Show violations
        if (result.Violations.Count > 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"=== VIOLATIONS ({result.Violations.Count}) ===");
            Console.ResetColor();
            Console.WriteLine();

            foreach (var violation in result.Violations)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write($"[{violation.RuleId}] ");
                Console.ResetColor();
                Console.WriteLine(violation.Message);

                if (!string.IsNullOrEmpty(violation.FilePath))
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write("  File: ");
                    Console.ResetColor();
                    Console.WriteLine(violation.FilePath + (violation.LineNumber.HasValue ? $":{violation.LineNumber}" : ""));
                }

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("  Spec: ");
                Console.ResetColor();
                Console.WriteLine(violation.SpecSource);

                if (!string.IsNullOrEmpty(violation.SuggestedFix))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("  Fix: ");
                    Console.ResetColor();
                    Console.WriteLine(violation.SuggestedFix);
                }

                Console.WriteLine();
            }
        }

        // Show warnings
        if (result.Warnings.Count > 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"=== WARNINGS ({result.Warnings.Count}) ===");
            Console.ResetColor();
            Console.WriteLine();

            foreach (var warning in result.Warnings)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"[{warning.RuleId}] ");
                Console.ResetColor();
                Console.WriteLine(warning.Message);

                if (!string.IsNullOrEmpty(warning.FilePath))
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write("  File: ");
                    Console.ResetColor();
                    Console.WriteLine(warning.FilePath + (warning.LineNumber.HasValue ? $":{warning.LineNumber}" : ""));
                }

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("  Spec: ");
                Console.ResetColor();
                Console.WriteLine(warning.SpecSource);

                if (!string.IsNullOrEmpty(warning.Recommendation))
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write("  Recommendation: ");
                    Console.ResetColor();
                    Console.WriteLine(warning.Recommendation);
                }

                Console.WriteLine();
            }
        }

        // Show info (only in verbose mode)
        if (verbose && result.Info.Count > 0)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"=== INFO ({result.Info.Count}) ===");
            Console.ResetColor();
            Console.WriteLine();

            foreach (var info in result.Info)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write($"[{info.Category}] ");
                Console.ResetColor();
                Console.WriteLine(info.Message);

                if (!string.IsNullOrEmpty(info.FilePath))
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write("  File: ");
                    Console.ResetColor();
                    Console.WriteLine(info.FilePath);
                }

                Console.WriteLine();
            }
        }

        // Summary
        Console.WriteLine("=== SUMMARY ===");
        Console.WriteLine();

        if (result.Passed && result.Warnings.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("All checks passed!");
            Console.ResetColor();
        }
        else if (result.Passed)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Passed with {result.Warnings.Count} warning(s).");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"FAILED: {result.Violations.Count} violation(s), {result.Warnings.Count} warning(s).");
            Console.ResetColor();
        }

        Console.WriteLine();
    }

    private static void OutputJson(AnalysisResult result)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(result, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        });
        Console.WriteLine(json);
    }
}
