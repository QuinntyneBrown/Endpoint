// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Endpoint.Engineering.ALaCarte;
using Endpoint.Engineering.ALaCarte.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.Cli.Commands;

/// <summary>
/// Command to take a folder from a git/gitlab repository and copy it to a target directory.
/// If the folder contains a .csproj, it will create/update a solution.
/// If the folder is an Angular workspace project, it will create/update the workspace.
/// </summary>
[Verb("take")]
public class TakeRequest : IRequest
{
    /// <summary>
    /// The full GitHub or GitLab URL to a folder (e.g., https://github.com/owner/repo/tree/branch/path/to/folder).
    /// This URL will be parsed to extract the repository URL, branch, and folder path.
    /// </summary>
    [Option('u', "url", Required = true, HelpText = "Full GitHub or GitLab URL to the folder (e.g., https://github.com/owner/repo/tree/branch/path/to/folder).")]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// The target directory where the folder will be copied to.
    /// </summary>
    [Option('d', "directory", Required = false, HelpText = "The target directory (default: current directory).")]
    public string Directory { get; set; } = Environment.CurrentDirectory;

    /// <summary>
    /// The name of the solution to create/update (if applicable).
    /// </summary>
    [Option('s', "solution", Required = false, HelpText = "The name of the solution to create/update.")]
    public string? SolutionName { get; set; }
}

/// <summary>
/// Handler for the Take command.
/// </summary>
public class TakeRequestHandler : IRequestHandler<TakeRequest>
{
    private readonly ILogger<TakeRequestHandler> _logger;
    private readonly IALaCarteService _alaCarteService;

    public TakeRequestHandler(
        ILogger<TakeRequestHandler> logger,
        IALaCarteService alaCarteService)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(alaCarteService);

        _logger = logger;
        _alaCarteService = alaCarteService;
    }

    public async Task Handle(TakeRequest request, CancellationToken cancellationToken)
    {
        // Validate URL is provided
        if (string.IsNullOrEmpty(request.Url))
        {
            _logger.LogError("URL is required. Please provide a full GitHub or GitLab URL to a folder.");
            _logger.LogInformation("Example: https://github.com/owner/repo/tree/branch/path/to/folder");
            return;
        }

        // Parse the URL to extract repository URL, branch, and folder path
        var parsedUrl = GitUrlParser.Parse(request.Url);
        if (parsedUrl == null)
        {
            _logger.LogError("Invalid URL format. The URL must be a GitHub or GitLab URL to a folder.");
            _logger.LogInformation("GitHub example: https://github.com/owner/repo/tree/branch/path/to/folder");
            _logger.LogInformation("GitLab example: https://gitlab.com/owner/repo/-/tree/branch/path/to/folder");
            return;
        }

        var (repositoryUrl, branch, folderPath) = parsedUrl.Value;

        _logger.LogInformation(
            "Parsed URL - Repository: {Url}, Branch: {Branch}, Folder: {Path}",
            repositoryUrl,
            branch,
            string.IsNullOrEmpty(folderPath) ? "(root)" : folderPath);

        var takeRequest = new ALaCarteTakeRequest
        {
            Url = repositoryUrl,
            Branch = branch,
            FromPath = folderPath,
            Directory = request.Directory,
            SolutionName = request.SolutionName
        };

        _logger.LogInformation(
            "Taking folder '{FromPath}' from repository '{Url}' (branch: {Branch})",
            string.IsNullOrEmpty(takeRequest.FromPath) ? "(root)" : takeRequest.FromPath,
            takeRequest.Url,
            takeRequest.Branch);

        var result = await _alaCarteService.TakeAsync(takeRequest, cancellationToken);

        if (result.Success)
        {
            _logger.LogInformation("Successfully copied folder to: {OutputDirectory}", result.OutputDirectory);

            if (!string.IsNullOrEmpty(result.SolutionPath))
            {
                _logger.LogInformation("Solution created/updated: {SolutionPath}", result.SolutionPath);
            }

            if (!string.IsNullOrEmpty(result.AngularWorkspacePath))
            {
                _logger.LogInformation("Angular workspace created/updated: {WorkspacePath}", result.AngularWorkspacePath);
            }

            foreach (var csproj in result.CsprojFiles)
            {
                _logger.LogInformation("Found .csproj: {CsprojPath}", csproj);
            }
        }
        else
        {
            _logger.LogError("Failed to take folder from repository.");
            foreach (var error in result.Errors)
            {
                _logger.LogError("Error: {Error}", error);
            }
        }

        foreach (var warning in result.Warnings)
        {
            _logger.LogWarning("Warning: {Warning}", warning);
        }
    }
}
