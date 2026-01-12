// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CommandLine;
using Endpoint.Engineering.AI.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.Cli.Commands;

[Verb("code-parse")]
public class CodeParseRequest : IRequest
{
    [Option('n', "name", HelpText = "Name for the output file (optional)")]
    public string? Name { get; set; }

    [Option('d', "directory", Required = false, HelpText = "Directory to parse (defaults to current directory)")]
    public string Directory { get; set; } = Environment.CurrentDirectory;

    [Option('o', "output", Required = false, HelpText = "Output file path (optional, prints to console if not specified)")]
    public string? Output { get; set; }

    [Option('e', "efficiency", Required = false, Default = "medium",
        HelpText = "Token efficiency level: low (full detail), medium (balanced), high (compact), max (minimal)")]
    public string Efficiency { get; set; } = "medium";
}

public class CodeParseRequestHandler : IRequestHandler<CodeParseRequest>
{
    private readonly ILogger<CodeParseRequestHandler> _logger;
    private readonly ICodeParser _codeParser;

    public CodeParseRequestHandler(
        ILogger<CodeParseRequestHandler> logger,
        ICodeParser codeParser)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _codeParser = codeParser ?? throw new ArgumentNullException(nameof(codeParser));
    }

    public async Task Handle(CodeParseRequest request, CancellationToken cancellationToken)
    {
        var efficiency = ParseEfficiency(request.Efficiency);
        _logger.LogInformation("Parsing code in directory: {Directory} with efficiency: {Efficiency}",
            request.Directory, efficiency);

        var summary = await _codeParser.ParseDirectoryAsync(request.Directory, efficiency, cancellationToken);
        var output = summary.ToLlmString();

        if (!string.IsNullOrEmpty(request.Output))
        {
            var outputPath = request.Output;
            if (!string.IsNullOrEmpty(request.Name))
            {
                outputPath = Path.Combine(
                    Path.GetDirectoryName(request.Output) ?? request.Directory,
                    $"{request.Name}.txt");
            }

            await File.WriteAllTextAsync(outputPath, output, cancellationToken);
            _logger.LogInformation("Output written to: {OutputPath}", outputPath);
        }
        else
        {
            Console.WriteLine(output);
        }

        _logger.LogInformation("Parsed {FileCount} files with {Efficiency} efficiency ({OutputLength} chars)",
            summary.TotalFiles, efficiency, output.Length);
    }

    private static CodeParseEfficiency ParseEfficiency(string value)
    {
        return value.ToLowerInvariant() switch
        {
            "low" or "l" or "1" => CodeParseEfficiency.Low,
            "medium" or "med" or "m" or "2" => CodeParseEfficiency.Medium,
            "high" or "h" or "3" => CodeParseEfficiency.High,
            "max" or "maximum" or "x" or "4" => CodeParseEfficiency.Max,
            _ => CodeParseEfficiency.Medium
        };
    }
}
