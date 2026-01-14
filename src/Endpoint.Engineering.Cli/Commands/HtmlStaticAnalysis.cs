// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CommandLine;
using Endpoint.Engineering.StaticAnalysis.Html;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.Cli.Commands;

[Verb("html-static-analysis")]
public class HtmlStaticAnalysisRequest : IRequest
{
    [Option('p', "path", Required = false,
        HelpText = "Directory path to analyze (defaults to current directory)")]
    public string? Path { get; set; }

    [Option('o', "output", Required = false,
        HelpText = "Output file path (optional, prints to console if not specified)")]
    public string? Output { get; set; }

    [Option('r', "recursive", Required = false, Default = true,
        HelpText = "Include subdirectories in analysis")]
    public bool Recursive { get; set; } = true;

    [Option("no-accessibility", Required = false, Default = false,
        HelpText = "Skip accessibility checks")]
    public bool NoAccessibility { get; set; } = false;

    [Option("no-seo", Required = false, Default = false,
        HelpText = "Skip SEO checks")]
    public bool NoSeo { get; set; } = false;

    [Option("no-security", Required = false, Default = false,
        HelpText = "Skip security checks")]
    public bool NoSecurity { get; set; } = false;

    [Option("no-best-practices", Required = false, Default = false,
        HelpText = "Skip best practices checks")]
    public bool NoBestPractices { get; set; } = false;

    [Option('e', "extensions", Required = false, Default = ".html,.htm",
        HelpText = "Comma-separated list of file extensions to analyze")]
    public string Extensions { get; set; } = ".html,.htm";
}

public class HtmlStaticAnalysisRequestHandler : IRequestHandler<HtmlStaticAnalysisRequest>
{
    private readonly ILogger<HtmlStaticAnalysisRequestHandler> _logger;
    private readonly IHtmlStaticAnalysisService _htmlStaticAnalysisService;

    public HtmlStaticAnalysisRequestHandler(
        ILogger<HtmlStaticAnalysisRequestHandler> logger,
        IHtmlStaticAnalysisService htmlStaticAnalysisService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _htmlStaticAnalysisService = htmlStaticAnalysisService ?? throw new ArgumentNullException(nameof(htmlStaticAnalysisService));
    }

    public async Task Handle(HtmlStaticAnalysisRequest request, CancellationToken cancellationToken)
    {
        var directoryPath = string.IsNullOrWhiteSpace(request.Path)
            ? Environment.CurrentDirectory
            : System.IO.Path.GetFullPath(request.Path);

        if (!Directory.Exists(directoryPath))
        {
            _logger.LogError("Directory not found: {DirectoryPath}", directoryPath);
            Console.WriteLine($"Error: Directory not found: {directoryPath}");
            return;
        }

        var options = new HtmlAnalysisOptions
        {
            Recursive = request.Recursive,
            Extensions = ParseExtensions(request.Extensions),
            CheckAccessibility = !request.NoAccessibility,
            CheckSeo = !request.NoSeo,
            CheckSecurity = !request.NoSecurity,
            CheckBestPractices = !request.NoBestPractices
        };

        _logger.LogInformation("Starting HTML static analysis for: {DirectoryPath}", directoryPath);
        _logger.LogInformation("Options: Recursive={Recursive}, Extensions={Extensions}",
            options.Recursive, string.Join(", ", options.Extensions));

        var result = await _htmlStaticAnalysisService.AnalyzeDirectoryAsync(
            directoryPath,
            options,
            cancellationToken);

        var output = result.ToFormattedString();

        if (!string.IsNullOrEmpty(request.Output))
        {
            var outputPath = System.IO.Path.GetFullPath(request.Output);
            await File.WriteAllTextAsync(outputPath, output, cancellationToken);
            _logger.LogInformation("Results written to: {OutputPath}", outputPath);
            Console.WriteLine($"Results written to: {outputPath}");
        }
        else
        {
            Console.WriteLine(output);
        }

        // Log summary
        _logger.LogInformation(
            "Analysis complete: {FileCount} files, {IssueCount} issues ({Errors} errors, {Warnings} warnings, {Info} info)",
            result.TotalFilesAnalyzed,
            result.Summary.TotalIssueCount,
            result.Summary.ErrorCount,
            result.Summary.WarningCount,
            result.Summary.InfoCount);
    }

    private static string[] ParseExtensions(string extensions)
    {
        return extensions
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(e => e.StartsWith('.') ? e : $".{e}")
            .ToArray();
    }
}
