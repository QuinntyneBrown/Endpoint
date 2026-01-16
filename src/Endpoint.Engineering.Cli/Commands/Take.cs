// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Endpoint.Engineering.ALaCarte;
using Endpoint.Engineering.ALaCarte.Models;
using Endpoint.Services;
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
    /// The URL of the git/gitlab repository.
    /// </summary>
    [Option('u', "url", Required = false, HelpText = "The git/gitlab repository URL.")]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// The branch to clone from.
    /// </summary>
    [Option('b', "branch", Required = false, HelpText = "The branch to clone from (default: main).")]
    public string Branch { get; set; } = "main";

    /// <summary>
    /// The path within the repository to the folder to copy.
    /// </summary>
    [Option('f', "from", Required = false, HelpText = "The path to the folder in the repository to copy.")]
    public string FromPath { get; set; } = string.Empty;

    /// <summary>
    /// The target directory where the folder will be copied to.
    /// </summary>
    [Option('d', Required = false, HelpText = "The target directory (default: current directory).")]
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
    private readonly IUserInputService _userInputService;

    public TakeRequestHandler(
        ILogger<TakeRequestHandler> logger,
        IALaCarteService alaCarteService,
        IUserInputService userInputService)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(alaCarteService);
        ArgumentNullException.ThrowIfNull(userInputService);

        _logger = logger;
        _alaCarteService = alaCarteService;
        _userInputService = userInputService;
    }

    public async Task Handle(TakeRequest request, CancellationToken cancellationToken)
    {
        ALaCarteTakeRequest takeRequest;

        // If URL is not provided, use interactive JSON input
        if (string.IsNullOrEmpty(request.Url))
        {
            _logger.LogInformation("No parameters provided. Starting interactive mode...");

            var sampleConfig = CreateSampleConfig();
            var jsonElement = await _userInputService.ReadJsonAsync(sampleConfig);

            takeRequest = ParseJsonConfig(jsonElement, request.Directory);
        }
        else
        {
            takeRequest = new ALaCarteTakeRequest
            {
                Url = request.Url,
                Branch = request.Branch,
                FromPath = request.FromPath,
                Directory = request.Directory,
                SolutionName = request.SolutionName
            };
        }

        // Validate required fields
        if (string.IsNullOrEmpty(takeRequest.Url))
        {
            _logger.LogError("Repository URL is required.");
            return;
        }

        if (string.IsNullOrEmpty(takeRequest.FromPath))
        {
            _logger.LogError("From path is required.");
            return;
        }

        _logger.LogInformation(
            "Taking folder '{FromPath}' from repository '{Url}' (branch: {Branch})",
            takeRequest.FromPath,
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

    private static string CreateSampleConfig()
    {
        var sampleConfig = new TakeConfigModel
        {
            Url = "https://github.com/username/repository",
            Branch = "main",
            FromPath = "src/ProjectFolder",
            SolutionName = "MySolution"
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        return JsonSerializer.Serialize(sampleConfig, options);
    }

    private static ALaCarteTakeRequest ParseJsonConfig(JsonElement jsonElement, string defaultDirectory)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        var config = JsonSerializer.Deserialize<TakeConfigModel>(jsonElement.GetRawText(), options)
            ?? new TakeConfigModel();

        return new ALaCarteTakeRequest
        {
            Url = config.Url ?? string.Empty,
            Branch = config.Branch ?? "main",
            FromPath = config.FromPath ?? string.Empty,
            Directory = config.Directory ?? defaultDirectory,
            SolutionName = config.SolutionName
        };
    }
}

/// <summary>
/// Model for JSON configuration input.
/// </summary>
internal class TakeConfigModel
{
    /// <summary>
    /// The URL of the git/gitlab repository.
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// The branch to clone from.
    /// </summary>
    public string? Branch { get; set; }

    /// <summary>
    /// The path within the repository to the folder to copy.
    /// </summary>
    public string? FromPath { get; set; }

    /// <summary>
    /// The target directory where the folder will be copied to.
    /// </summary>
    public string? Directory { get; set; }

    /// <summary>
    /// The name of the solution to create/update (if applicable).
    /// </summary>
    public string? SolutionName { get; set; }
}
