// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.AI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Parse command line arguments
var directoriesArg = (string?)null;
var efficiencyArg = "medium";
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
    var options = new CodeParseOptions
    {
        Efficiency = ParseEfficiency(efficiencyArg),
        IgnoreTests = ignoreTests && !testsOnly,
        TestsOnly = testsOnly
    };

    var filterMode = testsOnly ? " (tests only)" :
                     ignoreTests ? " (ignoring tests)" : " (all files)";

    Console.WriteLine($"Mode: {options.Efficiency} efficiency{filterMode}");
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
    Console.WriteLine("\nComparing parsing modes:\n");

    // Parse all files
    var allOptions = new CodeParseOptions { Efficiency = CodeParseEfficiency.Medium };
    var allSummary = await codeParser.ParseDirectoryAsync(directory, allOptions);
    var allOutput = allSummary.ToLlmString();

    // Parse ignoring tests
    var noTestsOptions = new CodeParseOptions { Efficiency = CodeParseEfficiency.Medium, IgnoreTests = true };
    var noTestsSummary = await codeParser.ParseDirectoryAsync(directory, noTestsOptions);
    var noTestsOutput = noTestsSummary.ToLlmString();

    // Parse tests only
    var testsOnlyOptions = new CodeParseOptions { Efficiency = CodeParseEfficiency.Medium, TestsOnly = true };
    var testsOnlySummary = await codeParser.ParseDirectoryAsync(directory, testsOnlyOptions);
    var testsOnlyOutput = testsOnlySummary.ToLlmString();

    Console.WriteLine("COMPARISON SUMMARY");
    Console.WriteLine(new string('-', 60));
    Console.WriteLine($"{"Mode",-20} {"Files",-10} {"Chars",-10} {"Est. Tokens",-12}");
    Console.WriteLine(new string('-', 60));
    Console.WriteLine($"{"All files",-20} {allSummary.Files.Count,-10} {allOutput.Length,-10} ~{allOutput.Length / 4,-12}");
    Console.WriteLine($"{"Ignoring tests",-20} {noTestsSummary.Files.Count,-10} {noTestsOutput.Length,-10} ~{noTestsOutput.Length / 4,-12}");
    Console.WriteLine($"{"Tests only",-20} {testsOnlySummary.Files.Count,-10} {testsOnlyOutput.Length,-10} ~{testsOnlyOutput.Length / 4,-12}");
    Console.WriteLine(new string('-', 60));

    var testFileCount = allSummary.Files.Count - noTestsSummary.Files.Count;
    var productionFileCount = allSummary.Files.Count - testsOnlySummary.Files.Count;
    Console.WriteLine($"\nDetected {testFileCount} test files and {productionFileCount} production files.");

    // Show token savings
    if (noTestsOutput.Length < allOutput.Length)
    {
        var savings = (1 - (double)noTestsOutput.Length / allOutput.Length) * 100;
        Console.WriteLine($"Token savings by ignoring tests: {savings:F1}%");
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

static CodeParseEfficiency ParseEfficiency(string value)
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

static void ShowHelp()
{
    Console.WriteLine(@"CodeParse Demo - Token-efficient code summarization for LLMs

Usage: CodeParseDemo [options] [directory]

Options:
  -d, --directory <paths>   Comma-separated list of directories to parse
                            (default: current directory)
  -e, --efficiency <level>  Token efficiency: low, medium, high, max
  --ignore-tests            Ignore test files and test projects
  --tests-only              Only parse test files and test projects
  -o, --output <file>       Write output to file
  -h, --help                Show this help

Examples:
  CodeParseDemo                                    # Compare all modes
  CodeParseDemo /path/to/code                      # Parse single directory
  CodeParseDemo -d ""/src,/lib,/api""                # Parse multiple directories
  CodeParseDemo -e high --ignore-tests             # High efficiency, skip tests
  CodeParseDemo --tests-only                       # Parse only test files

Test Detection:
  - Directories: test, tests, __tests__, *.Tests, *.Spec
  - Files: *.spec.ts, *.test.js, *Tests.cs, test_*.py
  - Imports: xUnit, NUnit, Jest, pytest, Mocha, etc.
  - Attributes: [Fact], [Test], @Test, describe(), it()
");
}
