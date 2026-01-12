// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Endpoint.Engineering.AI.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.Cli.Commands;

/// <summary>
/// Request to parse HTML content and extract a token-efficient summary for LLM consumption.
/// </summary>
[Verb("html-parse")]
public class HtmlParseRequest : IRequest
{
    /// <summary>
    /// Gets or sets the path to the HTML file to parse.
    /// </summary>
    [Option('p', "path", Required = false, HelpText = "Path to the HTML file to parse.")]
    public string? Path { get; set; }

    /// <summary>
    /// Gets or sets the URL to fetch and parse HTML from.
    /// </summary>
    [Option('u', "url", Required = false, HelpText = "URL to fetch and parse HTML from.")]
    public string? Url { get; set; }

    /// <summary>
    /// Gets or sets a list of URLs to fetch and parse HTML from.
    /// </summary>
    [Option("urls", Required = false, Separator = ',', HelpText = "Comma-separated list of URLs to fetch and parse HTML from.")]
    public IEnumerable<string>? Urls { get; set; }

    /// <summary>
    /// Gets or sets a list of directories containing HTML files to parse.
    /// </summary>
    [Option('d', "directories", Required = false, Separator = ',', HelpText = "Comma-separated list of directories containing HTML files to parse.")]
    public IEnumerable<string>? Directories { get; set; }

    /// <summary>
    /// Gets or sets the search pattern for HTML files in directories.
    /// </summary>
    [Option("pattern", Required = false, Default = "*.html", HelpText = "Search pattern for HTML files in directories (default: *.html).")]
    public string SearchPattern { get; set; } = "*.html";

    /// <summary>
    /// Gets or sets whether to search directories recursively.
    /// </summary>
    [Option('r', "recursive", Required = false, Default = false, HelpText = "Search directories recursively for HTML files.")]
    public bool Recursive { get; set; }

    /// <summary>
    /// Gets or sets the output file path to write the parsed content.
    /// </summary>
    [Option('o', "output", Required = false, HelpText = "Optional output file path to write the parsed content.")]
    public string? OutputPath { get; set; }
}

/// <summary>
/// Handler for parsing HTML content and extracting a token-efficient summary.
/// </summary>
public class HtmlParseRequestHandler : IRequestHandler<HtmlParseRequest>
{
    private readonly ILogger<HtmlParseRequestHandler> _logger;
    private readonly IHtmlParserService _htmlParserService;

    /// <summary>
    /// Initializes a new instance of the <see cref="HtmlParseRequestHandler"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="htmlParserService">The HTML parser service.</param>
    public HtmlParseRequestHandler(
        ILogger<HtmlParseRequestHandler> logger,
        IHtmlParserService htmlParserService)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(htmlParserService);

        _logger = logger;
        _htmlParserService = htmlParserService;
    }

    /// <inheritdoc/>
    public async Task Handle(HtmlParseRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting HTML parse command...");

        var results = new StringBuilder();
        var hasContent = false;

        // Process multiple URLs
        if (request.Urls?.Any() == true)
        {
            foreach (var url in request.Urls.Where(u => !string.IsNullOrWhiteSpace(u)))
            {
                try
                {
                    _logger.LogInformation("Fetching and parsing HTML from URL: {Url}", url);
                    var content = await _htmlParserService.ParseHtmlFromUrlAsync(url, cancellationToken);
                    AppendResult(results, $"URL: {url}", content);
                    hasContent = true;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse URL: {Url}", url);
                    AppendResult(results, $"URL: {url}", $"[Error: {ex.Message}]");
                }
            }
        }

        // Process single URL
        if (!string.IsNullOrWhiteSpace(request.Url))
        {
            try
            {
                _logger.LogInformation("Fetching and parsing HTML from URL: {Url}", request.Url);
                var content = await _htmlParserService.ParseHtmlFromUrlAsync(request.Url, cancellationToken);
                AppendResult(results, $"URL: {request.Url}", content);
                hasContent = true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse URL: {Url}", request.Url);
                AppendResult(results, $"URL: {request.Url}", $"[Error: {ex.Message}]");
            }
        }

        // Process directories
        if (request.Directories?.Any() == true)
        {
            var searchOption = request.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            foreach (var directory in request.Directories.Where(d => !string.IsNullOrWhiteSpace(d)))
            {
                if (!Directory.Exists(directory))
                {
                    _logger.LogWarning("Directory not found: {Directory}", directory);
                    AppendResult(results, $"Directory: {directory}", "[Error: Directory not found]");
                    continue;
                }

                _logger.LogInformation("Processing directory: {Directory} (pattern: {Pattern}, recursive: {Recursive})",
                    directory, request.SearchPattern, request.Recursive);

                var htmlFiles = Directory.GetFiles(directory, request.SearchPattern, searchOption);
                _logger.LogInformation("Found {Count} HTML files in {Directory}", htmlFiles.Length, directory);

                foreach (var filePath in htmlFiles)
                {
                    try
                    {
                        _logger.LogInformation("Parsing HTML from file: {Path}", filePath);
                        var content = await _htmlParserService.ParseHtmlFromFileAsync(filePath, cancellationToken);
                        AppendResult(results, $"File: {filePath}", content);
                        hasContent = true;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to parse file: {Path}", filePath);
                        AppendResult(results, $"File: {filePath}", $"[Error: {ex.Message}]");
                    }
                }
            }
        }

        // Process single file path
        if (!string.IsNullOrWhiteSpace(request.Path))
        {
            try
            {
                _logger.LogInformation("Parsing HTML from file: {Path}", request.Path);
                var content = await _htmlParserService.ParseHtmlFromFileAsync(request.Path, cancellationToken);
                AppendResult(results, $"File: {request.Path}", content);
                hasContent = true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse file: {Path}", request.Path);
                AppendResult(results, $"File: {request.Path}", $"[Error: {ex.Message}]");
            }
        }

        // Read from stdin if no other input provided
        if (!hasContent && results.Length == 0)
        {
            _logger.LogInformation("Reading HTML from stdin...");
            var htmlContent = await ReadFromStdinAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(htmlContent))
            {
                _logger.LogWarning("No HTML content provided. Use --url, --urls, --path, --directories, or pipe HTML content to stdin.");
                Console.WriteLine("No HTML content provided. Use --url, --urls, --path, --directories, or pipe HTML content to stdin.");
                return;
            }

            var content = _htmlParserService.ParseHtml(htmlContent);
            AppendResult(results, "stdin", content);
        }

        var finalContent = results.ToString().Trim();

        // Output the result
        if (!string.IsNullOrWhiteSpace(request.OutputPath))
        {
            await File.WriteAllTextAsync(request.OutputPath, finalContent, cancellationToken);
            _logger.LogInformation("Parsed content written to: {OutputPath}", request.OutputPath);
            Console.WriteLine($"Parsed content written to: {request.OutputPath}");
        }
        else
        {
            Console.WriteLine();
            Console.WriteLine("=== Parsed HTML Content (LLM-Optimized) ===");
            Console.WriteLine();
            Console.WriteLine(finalContent);
        }

        _logger.LogInformation("HTML parse command completed successfully.");
    }

    private static void AppendResult(StringBuilder builder, string source, string content)
    {
        if (builder.Length > 0)
        {
            builder.AppendLine();
            builder.AppendLine("---");
            builder.AppendLine();
        }

        builder.AppendLine($"## Source: {source}");
        builder.AppendLine();
        builder.AppendLine(content);
    }

    private static async Task<string> ReadFromStdinAsync(CancellationToken cancellationToken)
    {
        // Check if there's input available on stdin
        if (!Console.IsInputRedirected)
        {
            // No piped input, prompt for multiline input
            Console.WriteLine("Enter HTML content (press Ctrl+D on Unix or Ctrl+Z on Windows followed by Enter to finish):");
        }

        var builder = new StringBuilder();
        string? line;

        using var reader = new StreamReader(Console.OpenStandardInput());
        while ((line = await reader.ReadLineAsync(cancellationToken)) != null)
        {
            builder.AppendLine(line);
        }

        return builder.ToString();
    }
}
