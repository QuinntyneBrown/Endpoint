// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.AI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Configuration - modify these paths as needed
var targetDirectory = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();
var efficiencyArg = args.Length > 1 ? args[1] : null;
var outputFile = args.Length > 2 ? args[2] : null;

Console.WriteLine("=== CodeParse Demo ===");
Console.WriteLine($"Directory: {targetDirectory}");
Console.WriteLine();

// Setup dependency injection
var services = new ServiceCollection();

// Add logging
services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Warning); // Reduce noise for demo
});

// Add Engineering services (includes ICodeParser)
services.AddEngineeringServices();

var serviceProvider = services.BuildServiceProvider();
var codeParser = serviceProvider.GetRequiredService<ICodeParser>();

try
{
    // If efficiency is specified, parse with that level
    if (!string.IsNullOrEmpty(efficiencyArg))
    {
        var efficiency = ParseEfficiency(efficiencyArg);
        await ParseAndDisplay(efficiency);
    }
    else
    {
        // Demo all efficiency levels to show the difference
        Console.WriteLine("Demonstrating all efficiency levels:");
        Console.WriteLine(new string('=', 60));

        foreach (var efficiency in Enum.GetValues<CodeParseEfficiency>())
        {
            await ParseAndDisplay(efficiency);
            Console.WriteLine();
        }

        // Show comparison summary
        Console.WriteLine(new string('=', 60));
        Console.WriteLine("COMPARISON SUMMARY");
        Console.WriteLine(new string('=', 60));

        foreach (var efficiency in Enum.GetValues<CodeParseEfficiency>())
        {
            var summary = await codeParser.ParseDirectoryAsync(targetDirectory, efficiency);
            var output = summary.ToLlmString();
            var tokens = output.Length / 4;
            Console.WriteLine($"{efficiency,-8}: {output.Length,6} chars, ~{tokens,5} tokens");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

async Task ParseAndDisplay(CodeParseEfficiency efficiency)
{
    Console.WriteLine();
    Console.WriteLine($"--- Efficiency: {efficiency} ---");

    var summary = await codeParser.ParseDirectoryAsync(targetDirectory, efficiency);
    var llmOutput = summary.ToLlmString();

    if (!string.IsNullOrEmpty(outputFile))
    {
        var path = Path.Combine(
            Path.GetDirectoryName(outputFile) ?? ".",
            $"{Path.GetFileNameWithoutExtension(outputFile)}_{efficiency.ToString().ToLower()}{Path.GetExtension(outputFile)}");
        await File.WriteAllTextAsync(path, llmOutput);
        Console.WriteLine($"Output written to: {path}");
    }
    else
    {
        // Show preview (first 500 chars for medium+, full for low on small output)
        var preview = efficiency == CodeParseEfficiency.Low && llmOutput.Length < 2000
            ? llmOutput
            : llmOutput.Length > 500
                ? llmOutput[..500] + $"\n... ({llmOutput.Length - 500} more chars)"
                : llmOutput;

        Console.WriteLine(preview);
    }

    Console.WriteLine();
    Console.WriteLine($"Files: {summary.TotalFiles} | Chars: {llmOutput.Length} | Est. tokens: ~{llmOutput.Length / 4}");
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
