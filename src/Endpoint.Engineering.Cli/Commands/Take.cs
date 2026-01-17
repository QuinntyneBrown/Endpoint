// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Endpoint.Engineering.ALaCarte;
using Endpoint.Engineering.ALaCarte.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.Cli.Commands;

/// <summary>
/// Command to take a folder from a Git repository or local directory and copy it to a target directory.
/// Supports GitHub, GitLab, Bitbucket, Azure DevOps, Gitea, and private/self-hosted git hosts.
/// If the folder contains a .csproj, it will create/update a solution.
/// If the folder is an Angular workspace project, it will create/update the workspace.
/// </summary>
[Verb("take")]
public class TakeRequest : IRequest
{
    /// <summary>
    /// The full Git URL to a folder.
    /// Supports GitHub, GitLab, Bitbucket, Azure DevOps, Gitea, and private/self-hosted git hosts.
    /// This URL will be parsed to extract the repository URL, branch, and folder path.
    /// Not required if FromDirectory is specified.
    /// </summary>
    [Option('u', "url", Required = false, HelpText = "Full Git URL to the folder (supports GitHub, GitLab, Bitbucket, Azure DevOps, Gitea, and private git hosts).")]
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

    /// <summary>
    /// The root path for Angular projects within the angular.json workspace configuration.
    /// When specified, this value is used as the "root" property in angular.json instead of
    /// deriving it from the destination path. This is useful when the Angular library root
    /// has multiple segments (e.g., "nike/utils" instead of just "utils").
    /// </summary>
    [Option('r', "root", Required = false, HelpText = "The Angular library root path for angular.json (e.g., \"nike/utils\").")]
    public string? Root { get; set; }

    /// <summary>
    /// The local directory path to copy from instead of cloning from a Git repository.
    /// When specified, the folder is copied from this local directory instead of cloning from Git.
    /// </summary>
    [Option('f', "from-directory", Required = false, HelpText = "The local directory to copy from (instead of cloning from Git).")]
    public string? FromDirectory { get; set; }
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
        // Validate that either URL or FromDirectory is provided
        if (string.IsNullOrEmpty(request.Url) && string.IsNullOrEmpty(request.FromDirectory))
        {
            _logger.LogError("Either URL or FromDirectory is required.");
            _logger.LogInformation("Provide a Git URL using -u or a local directory using -f");
            _logger.LogInformation("Git URL example: https://github.com/owner/repo/tree/branch/path/to/folder");
            _logger.LogInformation("Local directory example: -f /path/to/local/directory");
            return;
        }

        // If FromDirectory is provided, use it directly
        if (!string.IsNullOrEmpty(request.FromDirectory))
        {
            if (!Directory.Exists(request.FromDirectory))
            {
                _logger.LogError("FromDirectory does not exist: {FromDirectory}", request.FromDirectory);
                return;
            }

            _logger.LogInformation("Taking folder from local directory: {FromDirectory}", request.FromDirectory);

            var localTakeRequest = new ALaCarteTakeRequest
            {
                FromDirectory = request.FromDirectory,
                Directory = request.Directory,
                SolutionName = request.SolutionName,
                Root = request.Root
            };

            var localResult = await _alaCarteService.TakeAsync(localTakeRequest, cancellationToken);

            DisplayResult(localResult);
            return;
        }

        // Otherwise, parse the URL to extract repository URL, branch, and folder path
        var parsedUrl = GitUrlParser.Parse(request.Url);
        if (parsedUrl == null)
        {
            _logger.LogError("Invalid URL format. The URL must be a valid Git URL to a folder.");
            _logger.LogInformation("GitHub/GitHub Enterprise: https://github.com/owner/repo/tree/branch/path");
            _logger.LogInformation("GitLab/Self-hosted GitLab: https://gitlab.com/owner/repo/-/tree/branch/path");
            _logger.LogInformation("Bitbucket Cloud: https://bitbucket.org/owner/repo/src/branch/path");
            _logger.LogInformation("Bitbucket Server: https://bitbucket.company.com/projects/PROJ/repos/repo/browse/path?at=branch");
            _logger.LogInformation("Azure DevOps: https://dev.azure.com/org/project/_git/repo?path=/path&version=GBbranch");
            _logger.LogInformation("Gitea/Gogs: https://gitea.company.com/owner/repo/src/branch/branch-name/path");
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
            SolutionName = request.SolutionName,
            Root = request.Root
        };

        _logger.LogInformation(
            "Taking folder '{FromPath}' from repository '{Url}' (branch: {Branch})",
            string.IsNullOrEmpty(takeRequest.FromPath) ? "(root)" : takeRequest.FromPath,
            takeRequest.Url,
            takeRequest.Branch);

        var result = await _alaCarteService.TakeAsync(takeRequest, cancellationToken);

        DisplayResult(result);
    }

    private void DisplayResult(ALaCarteTakeResult result)
    {
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
