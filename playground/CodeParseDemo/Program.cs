// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.AI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Configuration - modify these paths as needed
var targetDirectory = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();
var outputFile = args.Length > 1 ? args[1] : null;

Console.WriteLine("=== CodeParse Demo ===");
Console.WriteLine($"Parsing directory: {targetDirectory}");
Console.WriteLine();

// Setup dependency injection
var services = new ServiceCollection();

// Add logging
services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});

// Add Engineering services (includes ICodeParser)
services.AddEngineeringServices();

var serviceProvider = services.BuildServiceProvider();

var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
var codeParser = serviceProvider.GetRequiredService<ICodeParser>();

try
{
    // Parse the directory
    var summary = await codeParser.ParseDirectoryAsync(targetDirectory);

    // Generate token-efficient LLM output
    var llmOutput = summary.ToLlmString();

    if (!string.IsNullOrEmpty(outputFile))
    {
        // Write to file
        await File.WriteAllTextAsync(outputFile, llmOutput);
        logger.LogInformation("Output written to: {OutputFile}", outputFile);
        Console.WriteLine($"Output written to: {outputFile}");
    }
    else
    {
        // Print to console
        Console.WriteLine(llmOutput);
    }

    Console.WriteLine();
    Console.WriteLine($"=== Summary ===");
    Console.WriteLine($"Total files parsed: {summary.TotalFiles}");
    Console.WriteLine($"Output size: {llmOutput.Length} characters");

    // Estimate token count (rough approximation: ~4 chars per token)
    var estimatedTokens = llmOutput.Length / 4;
    Console.WriteLine($"Estimated tokens: ~{estimatedTokens}");
}
catch (Exception ex)
{
    logger.LogError(ex, "Error parsing directory");
    Console.WriteLine($"Error: {ex.Message}");
}
