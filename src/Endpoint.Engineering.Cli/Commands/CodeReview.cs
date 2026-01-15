// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Endpoint.Engineering.StaticAnalysis;
using Endpoint.Engineering.StaticAnalysis.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.Cli.Commands;

[Verb("code-review", HelpText = "Perform a code review by comparing the current branch against a target branch and running static analysis.")]
public class CodeReviewRequest : IRequest
{
    [Option('d', "directory", Required = false,
        HelpText = "Directory containing the git repository. Defaults to current directory.")]
    public string? Directory { get; set; }

    [Option('t', "target-branch", Required = false, Default = "main",
        HelpText = "Target branch to compare against (default: main).")]
    public string TargetBranch { get; set; } = "main";

    [Option('a', "analyze", Required = false, Default = true,
        HelpText = "Run static analysis on changed files (default: true).")]
    public bool RunStaticAnalysis { get; set; } = true;

    [Option('v', "verbose", Required = false, Default = false,
        HelpText = "Show verbose output including full diff.")]
    public bool Verbose { get; set; }

    [Option("no-color", Required = false, Default = false,
        HelpText = "Disable colorized output.")]
    public bool NoColor { get; set; }
}

public class CodeReviewRequestHandler : IRequestHandler<CodeReviewRequest>
{
    private readonly ILogger<CodeReviewRequestHandler> _logger;
    private readonly ICodeReviewService _codeReviewService;

    public CodeReviewRequestHandler(
        ILogger<CodeReviewRequestHandler> logger,
        ICodeReviewService codeReviewService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _codeReviewService = codeReviewService ?? throw new ArgumentNullException(nameof(codeReviewService));
    }

    public async Task Handle(CodeReviewRequest request, CancellationToken cancellationToken)
    {
        var directory = request.Directory ?? Environment.CurrentDirectory;

        WriteHeader(request.NoColor);

        try
        {
            var result = await _codeReviewService.ReviewAsync(
                directory,
                request.TargetBranch,
                request.RunStaticAnalysis,
                cancellationToken);

            DisplayResults(result, request.Verbose, request.NoColor);

            // Set exit code based on results
            if (result.HasIssues)
            {
                Environment.ExitCode = 1;
            }
        }
        catch (DirectoryNotFoundException ex)
        {
            WriteError($"Error: {ex.Message}", request.NoColor);
            Environment.ExitCode = 1;
        }
        catch (InvalidOperationException ex)
        {
            WriteError($"Error: {ex.Message}", request.NoColor);
            Environment.ExitCode = 1;
        }
        catch (Exception ex)
        {
            WriteError($"Unexpected error: {ex.Message}", request.NoColor);
            _logger.LogError(ex, "Error during code review");
            Environment.ExitCode = 1;
        }
    }

    private static void WriteHeader(bool noColor)
    {
        Console.WriteLine();
        if (!noColor)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
        }
        Console.WriteLine("=== Code Review ===");
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
        Console.WriteLine(message);
        if (!noColor)
        {
            Console.ResetColor();
        }
    }

    private static void DisplayResults(CodeReviewResult result, bool verbose, bool noColor)
    {
        // Display repository information
        Console.WriteLine($"Repository: {result.RepositoryRoot}");
        Console.WriteLine($"Current Branch: {result.GitDiff.CurrentBranch}");
        Console.WriteLine($"Target Branch: {result.GitDiff.TargetBranch}");
        Console.WriteLine();

        // Display changed files
        if (result.GitDiff.ChangedFiles.Count > 0)
        {
            if (!noColor)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
            }
            Console.WriteLine($"=== Changed Files ({result.GitDiff.ChangedFiles.Count}) ===");
            if (!noColor)
            {
                Console.ResetColor();
            }
            Console.WriteLine();

            foreach (var file in result.GitDiff.ChangedFiles)
            {
                Console.WriteLine($"  {file}");
            }
            Console.WriteLine();
        }
        else
        {
            if (!noColor)
            {
                Console.ForegroundColor = ConsoleColor.Green;
            }
            Console.WriteLine("No changes detected between branches.");
            if (!noColor)
            {
                Console.ResetColor();
            }
            Console.WriteLine();
            return;
        }

        // Display diff
        if (verbose && !string.IsNullOrWhiteSpace(result.GitDiff.RawDiff))
        {
            if (!noColor)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
            }
            Console.WriteLine("=== Diff ===");
            if (!noColor)
            {
                Console.ResetColor();
            }
            Console.WriteLine();

            DisplayColorizedDiff(result.GitDiff.RawDiff, noColor);
            Console.WriteLine();
        }

        // Display static analysis results
        if (result.AnalysisResult != null)
        {
            DisplayAnalysisResults(result.AnalysisResult, noColor);
        }

        // Display summary
        DisplaySummary(result, noColor);
    }

    private static void DisplayColorizedDiff(string diff, bool noColor)
    {
        var lines = diff.Split('\n');

        foreach (var line in lines)
        {
            if (!noColor)
            {
                if (line.StartsWith("+") && !line.StartsWith("+++"))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                else if (line.StartsWith("-") && !line.StartsWith("---"))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }
                else if (line.StartsWith("@@"))
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                }
                else if (line.StartsWith("diff --git"))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                }
            }

            Console.WriteLine(line);

            if (!noColor)
            {
                Console.ResetColor();
            }
        }
    }

    private static void DisplayAnalysisResults(AnalysisResult analysisResult, bool noColor)
    {
        if (!noColor)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
        }
        Console.WriteLine("=== Static Analysis Results ===");
        if (!noColor)
        {
            Console.ResetColor();
        }
        Console.WriteLine();

        Console.WriteLine($"Files Analyzed: {analysisResult.TotalFilesAnalyzed}");
        Console.WriteLine();

        // Display violations
        if (analysisResult.Violations.Count > 0)
        {
            if (!noColor)
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }
            Console.WriteLine($"VIOLATIONS ({analysisResult.Violations.Count}):");
            if (!noColor)
            {
                Console.ResetColor();
            }
            Console.WriteLine();

            foreach (var violation in analysisResult.Violations)
            {
                if (!noColor)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }
                Console.Write($"  [{violation.RuleId}] ");
                if (!noColor)
                {
                    Console.ResetColor();
                }
                Console.WriteLine(violation.Message);

                if (!string.IsNullOrEmpty(violation.FilePath))
                {
                    if (!noColor)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                    }
                    Console.Write("    File: ");
                    if (!noColor)
                    {
                        Console.ResetColor();
                    }
                    Console.WriteLine(violation.FilePath + (violation.LineNumber.HasValue ? $":{violation.LineNumber}" : ""));
                }

                if (!string.IsNullOrEmpty(violation.SuggestedFix))
                {
                    if (!noColor)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                    }
                    Console.Write("    Fix: ");
                    if (!noColor)
                    {
                        Console.ResetColor();
                    }
                    Console.WriteLine(violation.SuggestedFix);
                }

                Console.WriteLine();
            }
        }

        // Display warnings
        if (analysisResult.Warnings.Count > 0)
        {
            if (!noColor)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
            }
            Console.WriteLine($"WARNINGS ({analysisResult.Warnings.Count}):");
            if (!noColor)
            {
                Console.ResetColor();
            }
            Console.WriteLine();

            foreach (var warning in analysisResult.Warnings)
            {
                if (!noColor)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                }
                Console.Write($"  [{warning.RuleId}] ");
                if (!noColor)
                {
                    Console.ResetColor();
                }
                Console.WriteLine(warning.Message);

                if (!string.IsNullOrEmpty(warning.FilePath))
                {
                    if (!noColor)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                    }
                    Console.Write("    File: ");
                    if (!noColor)
                    {
                        Console.ResetColor();
                    }
                    Console.WriteLine(warning.FilePath + (warning.LineNumber.HasValue ? $":{warning.LineNumber}" : ""));
                }

                if (!string.IsNullOrEmpty(warning.Recommendation))
                {
                    if (!noColor)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                    }
                    Console.Write("    Recommendation: ");
                    if (!noColor)
                    {
                        Console.ResetColor();
                    }
                    Console.WriteLine(warning.Recommendation);
                }

                Console.WriteLine();
            }
        }

        if (analysisResult.Violations.Count == 0 && analysisResult.Warnings.Count == 0)
        {
            if (!noColor)
            {
                Console.ForegroundColor = ConsoleColor.Green;
            }
            Console.WriteLine("No violations or warnings found.");
            if (!noColor)
            {
                Console.ResetColor();
            }
            Console.WriteLine();
        }
    }

    private static void DisplaySummary(CodeReviewResult result, bool noColor)
    {
        if (!noColor)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
        }
        Console.WriteLine("=== Summary ===");
        if (!noColor)
        {
            Console.ResetColor();
        }
        Console.WriteLine();

        if (result.HasIssues)
        {
            if (!noColor)
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }
            Console.WriteLine($"Review FAILED: {result.AnalysisResult?.Violations.Count ?? 0} violation(s) found.");
            if (!noColor)
            {
                Console.ResetColor();
            }
        }
        else if (result.HasWarnings)
        {
            if (!noColor)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
            }
            Console.WriteLine($"Review passed with {result.AnalysisResult?.Warnings.Count ?? 0} warning(s).");
            if (!noColor)
            {
                Console.ResetColor();
            }
        }
        else
        {
            if (!noColor)
            {
                Console.ForegroundColor = ConsoleColor.Green;
            }
            Console.WriteLine("Review PASSED: No issues found.");
            if (!noColor)
            {
                Console.ResetColor();
            }
        }

        Console.WriteLine();
    }
}
