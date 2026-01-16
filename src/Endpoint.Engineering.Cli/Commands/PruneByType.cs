// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Endpoint.Engineering.SolutionPruning;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.Cli.Commands;

[Verb("prune-by-type", HelpText = "Prune a .NET solution to only include a specific type and its dependencies/references.")]
public class PruneByTypeRequest : IRequest
{
    [Option('t', "type", Required = true,
        HelpText = "The fully qualified name of the .NET type to prune around (e.g., 'MyNamespace.MyClass').")]
    public string TypeName { get; set; } = string.Empty;

    [Option('s', "solution", Required = true,
        HelpText = "Path to the .NET solution file (.sln).")]
    public string SolutionPath { get; set; } = string.Empty;

    [Option('o', "output", Required = false,
        HelpText = "Output directory for the pruned solution. Defaults to '<SolutionName>.Pruned' in the solution directory.")]
    public string? OutputDirectory { get; set; }

    [Option('v', "verbose", Required = false, Default = false,
        HelpText = "Show verbose output including all included types.")]
    public bool Verbose { get; set; }

    [Option("no-color", Required = false, Default = false,
        HelpText = "Disable colorized output.")]
    public bool NoColor { get; set; }
}

public class PruneByTypeRequestHandler : IRequestHandler<PruneByTypeRequest>
{
    private readonly ILogger<PruneByTypeRequestHandler> _logger;
    private readonly ISolutionPruneService _solutionPruneService;

    public PruneByTypeRequestHandler(
        ILogger<PruneByTypeRequestHandler> logger,
        ISolutionPruneService solutionPruneService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _solutionPruneService = solutionPruneService ?? throw new ArgumentNullException(nameof(solutionPruneService));
    }

    public async Task Handle(PruneByTypeRequest request, CancellationToken cancellationToken)
    {
        WriteHeader(request.NoColor);

        try
        {
            // Validate solution path
            var solutionPath = Path.GetFullPath(request.SolutionPath);
            if (!File.Exists(solutionPath))
            {
                WriteError($"Solution file not found: {solutionPath}", request.NoColor);
                Environment.ExitCode = 1;
                return;
            }

            WriteInfo($"Solution: {solutionPath}", request.NoColor);
            WriteInfo($"Target Type: {request.TypeName}", request.NoColor);
            Console.WriteLine();

            WriteProgress("Loading solution and analyzing types...", request.NoColor);

            var result = await _solutionPruneService.PruneAsync(
                solutionPath,
                request.TypeName,
                request.OutputDirectory,
                cancellationToken);

            Console.WriteLine();

            if (result.Success)
            {
                DisplayResults(result, request.Verbose, request.NoColor);
            }
            else
            {
                WriteError($"Error: {result.ErrorMessage}", request.NoColor);
                Environment.ExitCode = 1;
            }
        }
        catch (Exception ex)
        {
            WriteError($"Unexpected error: {ex.Message}", request.NoColor);
            _logger.LogError(ex, "Error during prune-by-type operation");
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
        Console.WriteLine("=== Prune By Type ===");
        if (!noColor)
        {
            Console.ResetColor();
        }
        Console.WriteLine();
    }

    private static void WriteInfo(string message, bool noColor)
    {
        Console.WriteLine(message);
    }

    private static void WriteProgress(string message, bool noColor)
    {
        if (!noColor)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
        }
        Console.WriteLine(message);
        if (!noColor)
        {
            Console.ResetColor();
        }
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

    private static void WriteSuccess(string message, bool noColor)
    {
        if (!noColor)
        {
            Console.ForegroundColor = ConsoleColor.Green;
        }
        Console.WriteLine(message);
        if (!noColor)
        {
            Console.ResetColor();
        }
    }

    private static void DisplayResults(SolutionPruneResult result, bool verbose, bool noColor)
    {
        // Success message
        WriteSuccess("Pruning completed successfully!", noColor);
        Console.WriteLine();

        // Summary
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

        Console.WriteLine($"Target Type: {result.TargetTypeName}");
        Console.WriteLine($"Output: {result.PrunedSolutionPath}");
        Console.WriteLine();

        Console.WriteLine($"Types included: {result.IncludedTypes.Count}");
        Console.WriteLine($"  - Dependencies: {result.DependencyTypeCount}");
        Console.WriteLine($"  - References: {result.DependentTypeCount}");
        Console.WriteLine();

        Console.WriteLine($"Projects included: {result.IncludedProjects.Count}");
        foreach (var project in result.IncludedProjects)
        {
            Console.WriteLine($"  - {project}");
        }
        Console.WriteLine();

        Console.WriteLine($"Files included: {result.IncludedFiles.Count}");

        if (verbose)
        {
            Console.WriteLine();
            if (!noColor)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
            }
            Console.WriteLine("=== Included Types ===");
            if (!noColor)
            {
                Console.ResetColor();
            }
            Console.WriteLine();

            foreach (var type in result.IncludedTypes)
            {
                Console.WriteLine($"  {type}");
            }
            Console.WriteLine();

            if (!noColor)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
            }
            Console.WriteLine("=== Included Files ===");
            if (!noColor)
            {
                Console.ResetColor();
            }
            Console.WriteLine();

            foreach (var file in result.IncludedFiles)
            {
                Console.WriteLine($"  {file}");
            }
        }
    }
}
