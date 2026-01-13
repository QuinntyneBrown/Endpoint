// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Artifacts;
using Endpoint.Artifacts.Abstractions;
using Endpoint.Engineering.Api;
using Endpoint.Services;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.Api.Artifacts.Strategies;

/// <summary>
/// Generation strategy for API Gateway projects.
/// </summary>
public class ApiGatewayProjectArtifactGenerationStrategy : IArtifactGenerationStrategy<ApiGatewayModel>
{
    private readonly ILogger<ApiGatewayProjectArtifactGenerationStrategy> _logger;
    private readonly ICommandService _commandService;
    private readonly IArtifactGenerator _artifactGenerator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiGatewayProjectArtifactGenerationStrategy"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="commandService">The command service.</param>
    /// <param name="artifactGenerator">The artifact generator.</param>
    public ApiGatewayProjectArtifactGenerationStrategy(
        ILogger<ApiGatewayProjectArtifactGenerationStrategy> logger,
        ICommandService commandService,
        IArtifactGenerator artifactGenerator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(commandService);
        ArgumentNullException.ThrowIfNull(artifactGenerator);

        _logger = logger;
        _commandService = commandService;
        _artifactGenerator = artifactGenerator;
    }

    /// <inheritdoc/>
    public int GetPriority() => 1;

    /// <inheritdoc/>
    public async Task GenerateAsync(ApiGatewayModel model)
    {
        _logger.LogInformation("Generating API Gateway project: {ProjectName}", model.Name);

        // Create the project using dotnet new
        _commandService.Start($"dotnet new web -n {model.Name} -o {model.Directory}", System.IO.Path.GetDirectoryName(model.Directory)!);

        // Generate all files for the project
        foreach (var file in model.Files)
        {
            await _artifactGenerator.GenerateAsync(file);
        }

        // Add packages
        foreach (var package in model.Packages)
        {
            _commandService.Start($"dotnet add {model.Path} package {package.Name} --version {package.Version}", model.Directory);
        }

        _logger.LogInformation("API Gateway project generated successfully: {ProjectName}", model.Name);
    }
}
