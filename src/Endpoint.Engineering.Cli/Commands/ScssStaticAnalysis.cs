// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text;
using CommandLine;
using Endpoint.Engineering.StaticAnalysis.Scss;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.Cli.Commands;

/// <summary>
/// Request to perform static analysis on SCSS files.
/// </summary>
[Verb("scss-static-analysis")]
public class ScssStaticAnalysisRequest : IRequest
{
    /// <summary>
    /// Gets or sets the directory path to analyze SCSS files.
    /// </summary>
    [Option('d', "directory", Required = false, HelpText = "Directory path to analyze SCSS files. Defaults to current directory.")]
    public string Directory { get; set; } = Environment.CurrentDirectory;

    /// <summary>
    /// Gets or sets a single file path to analyze.
    /// </summary>
    [Option('f', "file", Required = false, HelpText = "Single SCSS file path to analyze.")]
    public string? FilePath { get; set; }

    /// <summary>
    /// Gets or sets whether to search directories recursively.
    /// </summary>
    [Option('r', "recursive", Required = false, Default = true, HelpText = "Search directories recursively for SCSS files.")]
    public bool Recursive { get; set; } = true;

    /// <summary>
    /// Gets or sets the output format for the analysis results.
    /// </summary>
    [Option('o', "output", Required = false, Default = "console", HelpText = "Output format: console, json, or summary.")]
    public string OutputFormat { get; set; } = "console";

    /// <summary>
    /// Gets or sets whether to only show errors.
    /// </summary>
    [Option("errors-only", Required = false, Default = false, HelpText = "Only show errors, hide warnings and info messages.")]
    public bool ErrorsOnly { get; set; }

    /// <summary>
    /// Gets or sets whether to fail on warnings.
    /// </summary>
    [Option("fail-on-warnings", Required = false, Default = false, HelpText = "Exit with non-zero code if warnings are found.")]
    public bool FailOnWarnings { get; set; }
}

/// <summary>
/// Handler for performing static analysis on SCSS files.
/// </summary>
public class ScssStaticAnalysisRequestHandler : IRequestHandler<ScssStaticAnalysisRequest>
{
    private readonly ILogger<ScssStaticAnalysisRequestHandler> _logger;
    private readonly IScssStaticAnalysisService _scssStaticAnalysisService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScssStaticAnalysisRequestHandler"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="scssStaticAnalysisService">The SCSS static analysis service.</param>
    public ScssStaticAnalysisRequestHandler(
        ILogger<ScssStaticAnalysisRequestHandler> logger,
        IScssStaticAnalysisService scssStaticAnalysisService)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(scssStaticAnalysisService);

        _logger = logger;
        _scssStaticAnalysisService = scssStaticAnalysisService;
    }

    /// <inheritdoc/>
    public async Task Handle(ScssStaticAnalysisRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting SCSS static analysis...");

        try
        {
            if (!string.IsNullOrWhiteSpace(request.FilePath))
            {
                // Analyze single file
                var result = await _scssStaticAnalysisService.AnalyzeFileAsync(request.FilePath, cancellationToken);
                OutputSingleFileResult(result, request);
            }
            else
            {
                // Analyze directory
                var result = await _scssStaticAnalysisService.AnalyzeDirectoryAsync(
                    request.Directory,
                    request.Recursive,
                    cancellationToken);

                OutputDirectoryResult(result, request);
            }

            _logger.LogInformation("SCSS static analysis completed.");
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogError(ex, "File not found: {Path}", ex.FileName);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: File not found - {ex.FileName}");
            Console.ResetColor();
        }
        catch (DirectoryNotFoundException ex)
        {
            _logger.LogError(ex, "Directory not found");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: {ex.Message}");
            Console.ResetColor();
        }
    }

    private void OutputSingleFileResult(ScssAnalysisResult result, ScssStaticAnalysisRequest request)
    {
        if (request.OutputFormat.Equals("json", StringComparison.OrdinalIgnoreCase))
        {
            OutputJson(result);
            return;
        }

        if (request.OutputFormat.Equals("summary", StringComparison.OrdinalIgnoreCase))
        {
            OutputSummary(new ScssDirectoryAnalysisResult { FileResults = new List<ScssAnalysisResult> { result } });
            return;
        }

        Console.WriteLine();
        Console.WriteLine($"SCSS Static Analysis Results for: {result.FilePath}");
        Console.WriteLine(new string('=', 60));
        Console.WriteLine();

        OutputFileMetrics(result);
        OutputIssues(result.Issues, request.ErrorsOnly);
    }

    private void OutputDirectoryResult(ScssDirectoryAnalysisResult result, ScssStaticAnalysisRequest request)
    {
        if (request.OutputFormat.Equals("json", StringComparison.OrdinalIgnoreCase))
        {
            OutputJson(result);
            return;
        }

        if (request.OutputFormat.Equals("summary", StringComparison.OrdinalIgnoreCase))
        {
            OutputSummary(result);
            return;
        }

        Console.WriteLine();
        Console.WriteLine($"SCSS Static Analysis Results");
        Console.WriteLine($"Directory: {result.DirectoryPath}");
        Console.WriteLine(new string('=', 60));
        Console.WriteLine();

        Console.WriteLine($"Files analyzed: {result.TotalFiles}");
        Console.WriteLine($"Total lines: {result.TotalLines:N0}");
        Console.WriteLine();

        foreach (var fileResult in result.FileResults)
        {
            if (!fileResult.HasIssues && request.ErrorsOnly)
            {
                continue;
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"File: {fileResult.FilePath}");
            Console.ResetColor();
            Console.WriteLine(new string('-', 40));

            OutputFileMetrics(fileResult);

            if (fileResult.HasIssues)
            {
                OutputIssues(fileResult.Issues, request.ErrorsOnly);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("  No issues found.");
                Console.ResetColor();
            }

            Console.WriteLine();
        }

        OutputSummary(result);
    }

    private static void OutputFileMetrics(ScssAnalysisResult result)
    {
        Console.WriteLine($"  Lines: {result.TotalLines}");
        Console.WriteLine($"  Selectors: {result.SelectorCount}");
        Console.WriteLine($"  Variables: {result.VariableCount}");
        Console.WriteLine($"  Mixins: {result.MixinCount}");
        Console.WriteLine($"  Max nesting depth: {result.MaxNestingDepth}");
        Console.WriteLine();
    }

    private static void OutputIssues(List<ScssIssue> issues, bool errorsOnly)
    {
        var filteredIssues = errorsOnly
            ? issues.Where(i => i.Severity == IssueSeverity.Error).ToList()
            : issues;

        if (!filteredIssues.Any())
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  No issues found.");
            Console.ResetColor();
            return;
        }

        Console.WriteLine("  Issues:");

        foreach (var issue in filteredIssues.OrderBy(i => i.Line))
        {
            var severityColor = issue.Severity switch
            {
                IssueSeverity.Error => ConsoleColor.Red,
                IssueSeverity.Warning => ConsoleColor.Yellow,
                IssueSeverity.Info => ConsoleColor.Blue,
                _ => ConsoleColor.White
            };

            var severitySymbol = issue.Severity switch
            {
                IssueSeverity.Error => "✗",
                IssueSeverity.Warning => "⚠",
                IssueSeverity.Info => "ℹ",
                _ => "•"
            };

            Console.Write("    ");
            Console.ForegroundColor = severityColor;
            Console.Write($"{severitySymbol} ");
            Console.ResetColor();

            Console.WriteLine($"[{issue.Code}] Line {issue.Line}: {issue.Message}");

            if (!string.IsNullOrWhiteSpace(issue.SourceSnippet))
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"      > {TruncateSnippet(issue.SourceSnippet, 60)}");
                Console.ResetColor();
            }
        }
    }

    private static void OutputSummary(ScssDirectoryAnalysisResult result)
    {
        Console.WriteLine();
        Console.WriteLine("Summary");
        Console.WriteLine(new string('-', 40));

        Console.Write("  Errors: ");
        if (result.TotalErrors > 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(result.TotalErrors);
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("0");
            Console.ResetColor();
        }

        Console.Write("  Warnings: ");
        if (result.TotalWarnings > 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(result.TotalWarnings);
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("0");
            Console.ResetColor();
        }

        var infoCount = result.FileResults.Sum(r => r.InfoCount);
        Console.Write("  Info: ");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine(infoCount);
        Console.ResetColor();

        Console.WriteLine();

        if (result.TotalErrors > 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Analysis completed with errors.");
            Console.ResetColor();
        }
        else if (result.TotalWarnings > 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Analysis completed with warnings.");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Analysis completed successfully. No issues found.");
            Console.ResetColor();
        }
    }

    private static void OutputJson(object result)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(result, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        });
        Console.WriteLine(json);
    }

    private static string TruncateSnippet(string snippet, int maxLength)
    {
        snippet = snippet.Replace("\n", " ").Replace("\r", "").Trim();
        if (snippet.Length <= maxLength)
        {
            return snippet;
        }

        return snippet.Substring(0, maxLength - 3) + "...";
    }
}
