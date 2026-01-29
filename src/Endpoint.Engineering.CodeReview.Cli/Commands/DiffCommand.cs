// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.CommandLine;
using System.IO;
using System.Threading.Tasks;
using Endpoint.Engineering.CodeReview.Cli.Services;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.CodeReview.Cli.Commands;

public class DiffCommand
{
    private readonly ILogger<DiffCommand> _logger;
    private readonly IGitService _gitService;

    public DiffCommand(ILogger<DiffCommand> logger, IGitService gitService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _gitService = gitService ?? throw new ArgumentNullException(nameof(gitService));
    }

    public Command Create()
    {
        var command = new Command("diff", "Gets the diff between a branch and the default branch of a repository");

        var urlOption = new Option<string>(
            aliases: ["-u", "--url"],
            description: "The URL of the git repository (GitHub, GitLab, or git)")
        {
            IsRequired = true
        };

        var branchOption = new Option<string>(
            aliases: ["-b", "--branch"],
            description: "The name of the branch to compare")
        {
            IsRequired = true
        };

        var outputOption = new Option<string>(
            aliases: ["-o", "--output"],
            description: "The output file name (defaults to diff.txt)",
            getDefaultValue: () => "diff.txt");

        command.AddOption(urlOption);
        command.AddOption(branchOption);
        command.AddOption(outputOption);

        command.SetHandler(ExecuteAsync, urlOption, branchOption, outputOption);

        return command;
    }

    public async Task ExecuteAsync(string url, string branch, string output)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("URL cannot be null or empty.", nameof(url));
        }

        if (string.IsNullOrWhiteSpace(branch))
        {
            throw new ArgumentException("Branch cannot be null or empty.", nameof(branch));
        }

        if (string.IsNullOrWhiteSpace(output))
        {
            throw new ArgumentException("Output file name cannot be null or empty.", nameof(output));
        }

        // Validate output filename to prevent path traversal
        var fileName = Path.GetFileName(output);
        if (string.IsNullOrEmpty(fileName) || fileName != output)
        {
            throw new ArgumentException("Output must be a filename only, not a path.", nameof(output));
        }

        try
        {
            _logger.LogInformation("Getting diff for repository: {Url}, branch: {Branch}", url, branch);

            var diff = await _gitService.GetDiffAsync(url, branch);

            var outputPath = Path.Combine(Directory.GetCurrentDirectory(), output);
            await File.WriteAllTextAsync(outputPath, diff);

            _logger.LogInformation("Diff saved to: {OutputPath}", outputPath);
            Console.WriteLine($"Diff saved to: {outputPath}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting diff");
            Console.Error.WriteLine($"Error: {ex.Message}");
            throw;
        }
    }
}
