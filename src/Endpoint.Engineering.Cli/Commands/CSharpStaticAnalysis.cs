// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Endpoint.Engineering.StaticAnalysis.CSharp;
using Endpoint.Engineering.StaticAnalysis.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.Cli.Commands;

[Verb("csharp-static-analysis")]
public class CSharpStaticAnalysisRequest : IRequest
{
    [Option('p', "path", Required = false,
        HelpText = "Path to analyze (solution, project, directory, or file). Defaults to current directory.")]
    public string? Path { get; set; }

    [Option('o', "output", Required = false,
        HelpText = "Output file path. If not specified, results are printed to console.")]
    public string? Output { get; set; }

    [Option("severity", Required = false, Default = "all",
        HelpText = "Minimum severity level to report: info, warning, error, or all")]
    public string Severity { get; set; } = "all";

    [Option('c', "categories", Required = false,
        HelpText = "Comma-separated list of categories to analyze: naming, style, codequality, unusedcode, documentation, design, performance, security, maintainability")]
    public string? Categories { get; set; }

    [Option("include-tests", Required = false, Default = false,
        HelpText = "Include test files in analysis")]
    public bool IncludeTests { get; set; }

    [Option("max-issues", Required = false, Default = 0,
        HelpText = "Maximum number of issues to report. 0 means unlimited.")]
    public int MaxIssues { get; set; }

    [Option("json", Required = false, Default = false,
        HelpText = "Output results in JSON format")]
    public bool Json { get; set; }
}

public class CSharpStaticAnalysisRequestHandler : IRequestHandler<CSharpStaticAnalysisRequest>
{
    private readonly ILogger<CSharpStaticAnalysisRequestHandler> _logger;
    private readonly ICSharpStaticAnalysisService _staticAnalysisService;

    public CSharpStaticAnalysisRequestHandler(
        ILogger<CSharpStaticAnalysisRequestHandler> logger,
        ICSharpStaticAnalysisService staticAnalysisService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _staticAnalysisService = staticAnalysisService ?? throw new ArgumentNullException(nameof(staticAnalysisService));
    }

    public async Task Handle(CSharpStaticAnalysisRequest request, CancellationToken cancellationToken)
    {
        var options = BuildOptions(request);

        // Determine the path to analyze
        var pathToAnalyze = string.IsNullOrWhiteSpace(request.Path)
            ? Environment.CurrentDirectory
            : System.IO.Path.GetFullPath(request.Path);

        // Find the root of the solution/project/directory
        var (rootPath, targetType) = _staticAnalysisService.FindRoot(pathToAnalyze);

        _logger.LogInformation("Running C# static analysis on {TargetType}: {RootPath}", targetType, rootPath);

        // Run the analysis
        var result = await _staticAnalysisService.AnalyzeAsync(rootPath, options, cancellationToken);

        // Generate output
        string output;
        if (request.Json)
        {
            output = GenerateJsonOutput(result);
        }
        else
        {
            output = result.ToFormattedString();
        }

        // Output results
        if (!string.IsNullOrEmpty(request.Output))
        {
            var outputPath = request.Output;
            await File.WriteAllTextAsync(outputPath, output, cancellationToken);
            _logger.LogInformation("Analysis results written to: {OutputPath}", outputPath);

            // Also print summary to console
            PrintSummaryToConsole(result);
        }
        else
        {
            Console.WriteLine(output);
        }

        // Log completion
        _logger.LogInformation(
            "Analysis completed: {FileCount} files, {IssueCount} issues ({ErrorCount} errors, {WarningCount} warnings, {InfoCount} info)",
            result.Summary.TotalFiles,
            result.Summary.TotalIssues,
            result.Summary.ErrorCount,
            result.Summary.WarningCount,
            result.Summary.InfoCount);
    }

    private CSharpStaticAnalysisOptions BuildOptions(CSharpStaticAnalysisRequest request)
    {
        var options = new CSharpStaticAnalysisOptions
        {
            IncludeTests = request.IncludeTests,
            MaxIssues = request.MaxIssues
        };

        // Parse severity level
        switch (request.Severity.ToLowerInvariant())
        {
            case "error":
            case "errors":
                options.IncludeInfo = false;
                options.IncludeWarnings = false;
                options.IncludeErrors = true;
                break;
            case "warning":
            case "warnings":
                options.IncludeInfo = false;
                options.IncludeWarnings = true;
                options.IncludeErrors = true;
                break;
            case "info":
            case "all":
            default:
                options.IncludeInfo = true;
                options.IncludeWarnings = true;
                options.IncludeErrors = true;
                break;
        }

        // Parse categories
        if (!string.IsNullOrWhiteSpace(request.Categories))
        {
            var categories = request.Categories
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(ParseCategory)
                .Where(c => c.HasValue)
                .Select(c => c!.Value)
                .ToHashSet();

            options.Categories = categories;
        }

        return options;
    }

    private static IssueCategory? ParseCategory(string value)
    {
        return value.ToLowerInvariant() switch
        {
            "naming" => IssueCategory.Naming,
            "style" => IssueCategory.Style,
            "codequality" or "code-quality" or "quality" => IssueCategory.CodeQuality,
            "unusedcode" or "unused-code" or "unused" => IssueCategory.UnusedCode,
            "documentation" or "docs" or "doc" => IssueCategory.Documentation,
            "design" => IssueCategory.Design,
            "performance" or "perf" => IssueCategory.Performance,
            "security" or "sec" => IssueCategory.Security,
            "maintainability" or "maintain" => IssueCategory.Maintainability,
            _ => null
        };
    }

    private static string GenerateJsonOutput(CSharpStaticAnalysisResult result)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("{");
        sb.AppendLine($"  \"rootPath\": \"{EscapeJson(result.RootPath)}\",");
        sb.AppendLine($"  \"targetType\": \"{result.TargetType}\",");
        sb.AppendLine($"  \"success\": {result.Success.ToString().ToLower()},");

        // Summary
        sb.AppendLine("  \"summary\": {");
        sb.AppendLine($"    \"totalFiles\": {result.Summary.TotalFiles},");
        sb.AppendLine($"    \"totalLinesOfCode\": {result.Summary.TotalLinesOfCode},");
        sb.AppendLine($"    \"totalClasses\": {result.Summary.TotalClasses},");
        sb.AppendLine($"    \"totalInterfaces\": {result.Summary.TotalInterfaces},");
        sb.AppendLine($"    \"totalMethods\": {result.Summary.TotalMethods},");
        sb.AppendLine($"    \"totalIssues\": {result.Summary.TotalIssues},");
        sb.AppendLine($"    \"errorCount\": {result.Summary.ErrorCount},");
        sb.AppendLine($"    \"warningCount\": {result.Summary.WarningCount},");
        sb.AppendLine($"    \"infoCount\": {result.Summary.InfoCount},");
        sb.AppendLine($"    \"analysisDurationMs\": {result.Summary.AnalysisDurationMs}");
        sb.AppendLine("  },");

        // Issues
        sb.AppendLine("  \"issues\": [");
        for (int i = 0; i < result.Issues.Count; i++)
        {
            var issue = result.Issues[i];
            sb.AppendLine("    {");
            sb.AppendLine($"      \"filePath\": \"{EscapeJson(issue.FilePath)}\",");
            sb.AppendLine($"      \"line\": {issue.Line},");
            sb.AppendLine($"      \"column\": {issue.Column},");
            sb.AppendLine($"      \"severity\": \"{issue.Severity}\",");
            sb.AppendLine($"      \"category\": \"{issue.Category}\",");
            sb.AppendLine($"      \"ruleId\": \"{issue.RuleId}\",");
            sb.AppendLine($"      \"message\": \"{EscapeJson(issue.Message)}\"");
            sb.Append("    }");
            if (i < result.Issues.Count - 1)
                sb.AppendLine(",");
            else
                sb.AppendLine();
        }
        sb.AppendLine("  ]");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private static string EscapeJson(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t");
    }

    private static void PrintSummaryToConsole(CSharpStaticAnalysisResult result)
    {
        Console.WriteLine();
        Console.WriteLine("Analysis Summary:");
        Console.WriteLine($"  Files:    {result.Summary.TotalFiles}");
        Console.WriteLine($"  Issues:   {result.Summary.TotalIssues}");
        Console.WriteLine($"    Errors:   {result.Summary.ErrorCount}");
        Console.WriteLine($"    Warnings: {result.Summary.WarningCount}");
        Console.WriteLine($"    Info:     {result.Summary.InfoCount}");
        Console.WriteLine($"  Duration: {result.Summary.AnalysisDurationMs}ms");
    }
}
