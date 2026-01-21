// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Endpoint.Engineering.StaticAnalysis;
using Endpoint.Engineering.StaticAnalysis.Models;
using Endpoint.Engineering.StaticAnalysis.SonarQube;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.Cli.Commands;

[Verb("static-analysis", HelpText = "Perform static analysis on a codebase based on specification rules or SonarQube rules.")]
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

    [Option("git-compare", Required = false, Default = false,
        HelpText = "Compare against base branch and only analyze changed/added code.")]
    public bool GitCompare { get; set; }

    [Option('b', "base-branch", Required = false, Default = "master",
        HelpText = "Base branch to compare against (default: master). Used with --git-compare.")]
    public string BaseBranch { get; set; } = "master";

    [Option("sonar-rules", Required = false,
        HelpText = "Path to SonarQube rules markdown file. Defaults to docs/sonar-qube-rules.md.")]
    public string? SonarRulesPath { get; set; }

    [Option('o', "output", Required = false,
        HelpText = "Output directory for the results report file.")]
    public string? OutputDirectory { get; set; }

    [Option("report", Required = false, Default = false,
        HelpText = "Generate a unique markdown report file.")]
    public bool GenerateReport { get; set; }
}

public class StaticAnalysisRequestHandler : IRequestHandler<StaticAnalysisRequest>
{
    private readonly ILogger<StaticAnalysisRequestHandler> _logger;
    private readonly IStaticAnalysisService _staticAnalysisService;
    private readonly ISonarQubeGitAnalyzer _sonarQubeGitAnalyzer;

    public StaticAnalysisRequestHandler(
        ILogger<StaticAnalysisRequestHandler> logger,
        IStaticAnalysisService staticAnalysisService,
        ISonarQubeGitAnalyzer sonarQubeGitAnalyzer)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _staticAnalysisService = staticAnalysisService ?? throw new ArgumentNullException(nameof(staticAnalysisService));
        _sonarQubeGitAnalyzer = sonarQubeGitAnalyzer ?? throw new ArgumentNullException(nameof(sonarQubeGitAnalyzer));
    }

    public async Task Handle(StaticAnalysisRequest request, CancellationToken cancellationToken)
    {
        var path = request.Path ?? Environment.CurrentDirectory;

        // If git-compare mode is enabled, use the SonarQube git analyzer
        if (request.GitCompare)
        {
            await HandleGitCompareAnalysis(request, path, cancellationToken);
            return;
        }

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

    private async Task HandleGitCompareAnalysis(StaticAnalysisRequest request, string path, CancellationToken cancellationToken)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("=== Endpoint Static Analysis (Git Compare Mode) ===");
        Console.ResetColor();
        Console.WriteLine();

        try
        {
            var result = await _sonarQubeGitAnalyzer.AnalyzeAsync(
                path,
                request.BaseBranch,
                request.SonarRulesPath,
                cancellationToken);

            Console.WriteLine($"Current Branch: {result.CurrentBranch}");
            Console.WriteLine($"Base Branch: {result.BaseBranch}");
            Console.WriteLine($"Changed Files: {result.ChangedFiles.Count}");
            Console.WriteLine($"Issues Found: {result.Issues.Count}");
            Console.WriteLine();

            if (request.GenerateReport || request.OutputDirectory != null)
            {
                var reportPath = GenerateSonarQubeReport(result, request.OutputDirectory ?? path);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Report generated: {reportPath}");
                Console.ResetColor();
                Console.WriteLine();
            }

            if (request.Json)
            {
                OutputSonarQubeJson(result);
            }
            else
            {
                OutputSonarQubeConsole(result, request.Verbose);
            }

            // Set exit code based on results
            if (result.Issues.Any(i => i.Category == "Vulnerability" || i.Category == "Bug"))
            {
                Environment.ExitCode = 1;
            }
            else if (request.FailOnWarning && result.Issues.Any())
            {
                Environment.ExitCode = 1;
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error during analysis: {ex.Message}");
            Console.ResetColor();
            _logger.LogError(ex, "Error during git compare analysis");
            Environment.ExitCode = 1;
        }
    }

    private string GenerateSonarQubeReport(SonarQubeAnalysisResult result, string outputDir)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HHmmss");
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var fileName = $"static-analysis-report_{timestamp}_{uniqueId}.md";
        var reportPath = System.IO.Path.Combine(outputDir, fileName);

        var sb = new StringBuilder();

        sb.AppendLine("# Static Analysis Report");
        sb.AppendLine();
        sb.AppendLine($"**Generated:** {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"**Current Branch:** {result.CurrentBranch}");
        sb.AppendLine($"**Base Branch:** {result.BaseBranch}");
        sb.AppendLine($"**Files Analyzed:** {result.ChangedFiles.Count}");
        sb.AppendLine($"**Issues Found:** {result.Issues.Count}");
        sb.AppendLine();

        // Summary by category
        sb.AppendLine("## Summary by Category");
        sb.AppendLine();
        sb.AppendLine("| Category | Count |");
        sb.AppendLine("|----------|-------|");
        foreach (var group in result.Issues.GroupBy(i => i.Category).OrderByDescending(g => g.Count()))
        {
            sb.AppendLine($"| {group.Key} | {group.Count()} |");
        }
        sb.AppendLine();

        // Summary by rule
        if (result.Issues.Any())
        {
            sb.AppendLine("## Summary by Rule");
            sb.AppendLine();
            sb.AppendLine("| Rule ID | Description | Count |");
            sb.AppendLine("|---------|-------------|-------|");
            foreach (var group in result.Issues.GroupBy(i => new { i.RuleId, i.Message }).OrderByDescending(g => g.Count()))
            {
                sb.AppendLine($"| {group.Key.RuleId} | {group.Key.Message} | {group.Count()} |");
            }
            sb.AppendLine();
        }

        // Changed files
        sb.AppendLine("## Changed Files");
        sb.AppendLine();
        foreach (var file in result.ChangedFiles)
        {
            var fileIssueCount = result.Issues.Count(i => i.FilePath == file.Path);
            sb.AppendLine($"- `{file.Path}` ({file.Status}) - {fileIssueCount} issue(s)");
        }
        sb.AppendLine();

        // Detailed issues by file
        if (result.Issues.Any())
        {
            sb.AppendLine("## Detailed Issues");
            sb.AppendLine();

            foreach (var fileGroup in result.Issues.GroupBy(i => i.FilePath))
            {
                sb.AppendLine($"### {fileGroup.Key}");
                sb.AppendLine();

                foreach (var categoryGroup in fileGroup.GroupBy(i => i.Category))
                {
                    sb.AppendLine($"#### {categoryGroup.Key}");
                    sb.AppendLine();

                    foreach (var issue in categoryGroup.OrderBy(i => i.LineNumber))
                    {
                        sb.AppendLine($"**Line {issue.LineNumber}** - [{issue.RuleId}] {issue.Message}");
                        sb.AppendLine();
                        sb.AppendLine("```" + (issue.Language == "csharp" ? "csharp" : "typescript"));
                        sb.AppendLine(issue.LineContent);
                        sb.AppendLine("```");
                        sb.AppendLine();
                    }
                }
            }
        }

        // Rules reference
        sb.AppendLine("---");
        sb.AppendLine();
        sb.AppendLine("## Rules Reference");
        sb.AppendLine();
        sb.AppendLine("For detailed information about each rule, visit:");
        sb.AppendLine("- C#: https://rules.sonarsource.com/csharp/");
        sb.AppendLine("- TypeScript: https://rules.sonarsource.com/typescript/");

        File.WriteAllText(reportPath, sb.ToString());

        return reportPath;
    }

    private static void OutputSonarQubeConsole(SonarQubeAnalysisResult result, bool verbose)
    {
        // Group issues by category
        var vulnerabilities = result.Issues.Where(i => i.Category == "Vulnerability").ToList();
        var bugs = result.Issues.Where(i => i.Category == "Bug").ToList();
        var securityHotspots = result.Issues.Where(i => i.Category == "Security Hotspot").ToList();
        var codeSmells = result.Issues.Where(i => i.Category == "Code Smell").ToList();

        // Vulnerabilities
        if (vulnerabilities.Any())
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"=== VULNERABILITIES ({vulnerabilities.Count}) ===");
            Console.ResetColor();
            Console.WriteLine();

            foreach (var issue in vulnerabilities)
            {
                OutputIssue(issue);
            }
        }

        // Bugs
        if (bugs.Any())
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"=== BUGS ({bugs.Count}) ===");
            Console.ResetColor();
            Console.WriteLine();

            foreach (var issue in bugs)
            {
                OutputIssue(issue);
            }
        }

        // Security Hotspots
        if (securityHotspots.Any())
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"=== SECURITY HOTSPOTS ({securityHotspots.Count}) ===");
            Console.ResetColor();
            Console.WriteLine();

            foreach (var issue in securityHotspots)
            {
                OutputIssue(issue);
            }
        }

        // Code Smells (only in verbose mode or if no other issues)
        if (verbose || (!vulnerabilities.Any() && !bugs.Any() && !securityHotspots.Any()))
        {
            if (codeSmells.Any())
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine($"=== CODE SMELLS ({codeSmells.Count}) ===");
                Console.ResetColor();
                Console.WriteLine();

                foreach (var issue in codeSmells)
                {
                    OutputIssue(issue);
                }
            }
        }
        else if (codeSmells.Any())
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"({codeSmells.Count} code smells not shown. Use --verbose to see all.)");
            Console.ResetColor();
            Console.WriteLine();
        }

        // Summary
        Console.WriteLine("=== SUMMARY ===");
        Console.WriteLine();

        if (!result.Issues.Any())
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("No issues found!");
            Console.ResetColor();
        }
        else
        {
            Console.WriteLine($"Vulnerabilities: {vulnerabilities.Count}");
            Console.WriteLine($"Bugs: {bugs.Count}");
            Console.WriteLine($"Security Hotspots: {securityHotspots.Count}");
            Console.WriteLine($"Code Smells: {codeSmells.Count}");
        }

        Console.WriteLine();
    }

    private static void OutputIssue(SonarQubeIssue issue)
    {
        Console.ForegroundColor = issue.Category switch
        {
            "Vulnerability" => ConsoleColor.Red,
            "Bug" => ConsoleColor.Red,
            "Security Hotspot" => ConsoleColor.Yellow,
            _ => ConsoleColor.DarkYellow
        };
        Console.Write($"[{issue.RuleId}] ");
        Console.ResetColor();
        Console.WriteLine(issue.Message);

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("  File: ");
        Console.ResetColor();
        Console.WriteLine($"{issue.FilePath}:{issue.LineNumber}");

        Console.WriteLine();
    }

    private static void OutputSonarQubeJson(SonarQubeAnalysisResult result)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(result, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        });
        Console.WriteLine(json);
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
