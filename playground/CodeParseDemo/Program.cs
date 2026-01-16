// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.AI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Parse command line arguments
var directoriesArg = (string?)null;
var efficiencyArg = "50";
var ignoreTests = false;
var testsOnly = false;
var outputFile = (string?)null;

for (var i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "-d" or "--directory" when i + 1 < args.Length:
            directoriesArg = args[++i];
            break;
        case "-e" or "--efficiency" when i + 1 < args.Length:
            efficiencyArg = args[++i];
            break;
        case "--ignore-tests":
            ignoreTests = true;
            break;
        case "--tests-only":
            testsOnly = true;
            break;
        case "-o" or "--output" when i + 1 < args.Length:
            outputFile = args[++i];
            break;
        case "-h" or "--help":
            ShowHelp();
            return;
        default:
            // Accept directory as positional argument (can be comma-separated)
            if (!args[i].StartsWith('-'))
            {
                directoriesArg = args[i];
            }
            break;
    }
}

// Parse directories - default to current directory
var targetDirectories = ParseDirectories(directoriesArg);

Console.WriteLine("=== CodeParse Demo ===");
Console.WriteLine($"Directories: {string.Join(", ", targetDirectories.Select(Path.GetFileName))}");
Console.WriteLine();

// Setup dependency injection
var services = new ServiceCollection();

services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Warning);
});

services.AddEngineeringServices();

var serviceProvider = services.BuildServiceProvider();
var codeParser = serviceProvider.GetRequiredService<ICodeParser>();

try
{
    // Build options
    var efficiency = ParseEfficiency(efficiencyArg);
    var options = new CodeParseOptions
    {
        Efficiency = efficiency,
        IgnoreTests = ignoreTests && !testsOnly,
        TestsOnly = testsOnly
    };

    var filterMode = testsOnly ? " (tests only)" :
                     ignoreTests ? " (ignoring tests)" : " (all files)";

    Console.WriteLine($"Mode: efficiency={options.Efficiency}{filterMode}");
    Console.WriteLine(new string('=', 60));

    // If no specific mode requested and single directory, show comparison
    if (!ignoreTests && !testsOnly && string.IsNullOrEmpty(outputFile) && targetDirectories.Count == 1)
    {
        await ShowComparison(targetDirectories[0]);
    }
    else
    {
        await ParseAndDisplay(options);
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

async Task ShowComparison(string directory)
{
    Console.WriteLine("\nComparing efficiency levels:\n");

    // Parse at different efficiency levels
    var efficiencies = new[] { 0, 25, 50, 75, 100 };
    var results = new List<(int Efficiency, int Files, int Chars)>();

    foreach (var eff in efficiencies)
    {
        var options = new CodeParseOptions { Efficiency = eff };
        var summary = await codeParser.ParseDirectoryAsync(directory, options);
        var output = summary.ToLlmString();
        results.Add((eff, summary.Files.Count, output.Length));
    }

    Console.WriteLine("EFFICIENCY COMPARISON");
    Console.WriteLine(new string('-', 60));
    Console.WriteLine($"{"Efficiency",-15} {"Files",-10} {"Chars",-10} {"Est. Tokens",-12}");
    Console.WriteLine(new string('-', 60));

    foreach (var (eff, files, chars) in results)
    {
        var label = eff switch
        {
            0 => "0 (verbatim)",
            25 => "25 (full)",
            50 => "50 (balanced)",
            75 => "75 (compact)",
            100 => "100 (minimal)",
            _ => eff.ToString()
        };
        Console.WriteLine($"{label,-15} {files,-10} {chars,-10} ~{chars / 4,-12}");
    }
    Console.WriteLine(new string('-', 60));

    // Show token savings
    if (results.Count >= 2 && results[^1].Chars < results[0].Chars)
    {
        var savings = (1 - (double)results[^1].Chars / results[0].Chars) * 100;
        Console.WriteLine($"Token savings from efficiency 0 to 100: {savings:F1}%");
    }
}

async Task ParseAndDisplay(CodeParseOptions options)
{
    // Parse all directories and merge results
    var mergedSummary = new CodeSummary
    {
        RootDirectory = targetDirectories.Count == 1
            ? targetDirectories[0]
            : string.Join(", ", targetDirectories.Select(Path.GetFileName)),
        Efficiency = options.Efficiency
    };

    foreach (var directory in targetDirectories)
    {
        if (!Directory.Exists(directory))
        {
            Console.WriteLine($"Warning: Directory not found: {directory}");
            continue;
        }

        Console.WriteLine($"Parsing: {directory}");
        var summary = await codeParser.ParseDirectoryAsync(directory, options);
        mergedSummary.Files.AddRange(summary.Files);
        mergedSummary.TotalFiles += summary.TotalFiles;
    }

    var llmOutput = mergedSummary.ToLlmString();

    if (!string.IsNullOrEmpty(outputFile))
    {
        await File.WriteAllTextAsync(outputFile, llmOutput);
        Console.WriteLine($"Output written to: {outputFile}");
    }
    else
    {
        // Show preview
        var preview = llmOutput.Length > 1000
            ? llmOutput[..1000] + $"\n... ({llmOutput.Length - 1000} more chars)"
            : llmOutput;

        Console.WriteLine(preview);
    }

    Console.WriteLine();
    Console.WriteLine(new string('-', 40));
    Console.WriteLine($"Directories: {targetDirectories.Count}");
    Console.WriteLine($"Files: {mergedSummary.Files.Count}");
    Console.WriteLine($"Chars: {llmOutput.Length}");
    Console.WriteLine($"Est. tokens: ~{llmOutput.Length / 4}");
}

static List<string> ParseDirectories(string? directoriesArg)
{
    if (string.IsNullOrWhiteSpace(directoriesArg))
    {
        return [Directory.GetCurrentDirectory()];
    }

    var directories = directoriesArg
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Select(d => Path.GetFullPath(d.Trim()))
        .Distinct()
        .ToList();

    return directories.Count > 0 ? directories : [Directory.GetCurrentDirectory()];
}

static int ParseEfficiency(string value)
{
    // Try to parse as integer first
    if (int.TryParse(value, out var intValue))
    {
        return Math.Clamp(intValue, 0, 100);
    }

    // Support legacy string values
    return value.ToLowerInvariant() switch
    {
        "verbatim" or "v" => 0,
        "low" or "l" => 15,
        "medium" or "med" or "m" => 40,
        "high" or "h" => 65,
        "max" or "maximum" or "x" => 90,
        "minimal" => 100,
        _ => 50
    };
}

static void ShowHelp()
{
    Console.WriteLine(@"CodeParse Demo - Token-efficient code summarization for LLMs

Usage: CodeParseDemo [options] [directory]

Options:
  -d, --directory <paths>   Comma-separated list of directories to parse
                            (default: current directory)
  -e, --efficiency <level>  Token efficiency: 0-100 (or: verbatim, low, medium, high, max)
                            0 = verbatim (full code content)
                            100 = minimal (most compact summary)
  --ignore-tests            Ignore test files and test projects
  --tests-only              Only parse test files and test projects
  -o, --output <file>       Write output to file
  -h, --help                Show this help

Examples:
  CodeParseDemo                                    # Compare efficiency levels
  CodeParseDemo /path/to/code                      # Parse single directory
  CodeParseDemo -d ""/src,/lib,/api""                # Parse multiple directories
  CodeParseDemo -e 0                               # Verbatim (full code)
  CodeParseDemo -e 50                              # Balanced output
  CodeParseDemo -e 100 --ignore-tests              # Minimal, skip tests
  CodeParseDemo --tests-only                       # Parse only test files

Test Detection:
  - Directories: test, tests, __tests__, *.Tests, *.Spec
  - Files: *.spec.ts, *.test.js, *Tests.cs, test_*.py
  - Imports: xUnit, NUnit, Jest, pytest, Mocha, etc.
  - Attributes: [Fact], [Test], @Test, describe(), it()
");
}
