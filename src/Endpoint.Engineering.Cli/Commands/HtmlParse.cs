// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text;
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
    /// If not provided, the command will read from standard input.
    /// </summary>
    [Option('p', "path", Required = false, HelpText = "Path to the HTML file to parse. If not provided, reads from stdin.")]
    public string? Path { get; set; }

    /// <summary>
    /// Gets or sets whether to output the result to a file instead of console.
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

        string parsedContent;

        if (!string.IsNullOrWhiteSpace(request.Path))
        {
            // Parse from file
            _logger.LogInformation("Parsing HTML from file: {Path}", request.Path);
            parsedContent = await _htmlParserService.ParseHtmlFromFileAsync(request.Path, cancellationToken);
        }
        else
        {
            // Read from stdin
            _logger.LogInformation("Reading HTML from stdin...");
            var htmlContent = await ReadFromStdinAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(htmlContent))
            {
                _logger.LogWarning("No HTML content provided. Use --path to specify a file or pipe HTML content to stdin.");
                Console.WriteLine("No HTML content provided. Use --path to specify a file or pipe HTML content to stdin.");
                return;
            }

            parsedContent = _htmlParserService.ParseHtml(htmlContent);
        }

        // Output the result
        if (!string.IsNullOrWhiteSpace(request.OutputPath))
        {
            await File.WriteAllTextAsync(request.OutputPath, parsedContent, cancellationToken);
            _logger.LogInformation("Parsed content written to: {OutputPath}", request.OutputPath);
            Console.WriteLine($"Parsed content written to: {request.OutputPath}");
        }
        else
        {
            Console.WriteLine();
            Console.WriteLine("=== Parsed HTML Content (LLM-Optimized) ===");
            Console.WriteLine();
            Console.WriteLine(parsedContent);
        }

        _logger.LogInformation("HTML parse command completed successfully.");
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
