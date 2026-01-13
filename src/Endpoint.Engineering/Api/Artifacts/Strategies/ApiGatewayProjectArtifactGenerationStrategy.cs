// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Artifacts;
using Endpoint.Artifacts.Abstractions;
using Endpoint.DotNet.Artifacts.Projects.Services;
using Endpoint.Engineering.Api;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.Api.Artifacts.Strategies;

/// <summary>
/// Generation strategy for API Gateway projects.
/// </summary>
public class ApiGatewayProjectArtifactGenerationStrategy : IArtifactGenerationStrategy<ApiGatewayModel>
{
    private readonly ILogger<ApiGatewayProjectArtifactGenerationStrategy> _logger;
    private readonly IProjectService _projectService;
    private readonly IArtifactGenerator _artifactGenerator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiGatewayProjectArtifactGenerationStrategy"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="projectService">The project service.</param>
    /// <param name="artifactGenerator">The artifact generator.</param>
    public ApiGatewayProjectArtifactGenerationStrategy(
        ILogger<ApiGatewayProjectArtifactGenerationStrategy> logger,
        IProjectService projectService,
        IArtifactGenerator artifactGenerator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(projectService);
        ArgumentNullException.ThrowIfNull(artifactGenerator);

        _logger = logger;
        _projectService = projectService;
        _artifactGenerator = artifactGenerator;
    }

    /// <inheritdoc/>
    public int GetPriority() => 1;

    /// <inheritdoc/>
    public async Task GenerateAsync(ApiGatewayModel model)
    {
        _logger.LogInformation("Generating API Gateway project: {ProjectName}", model.Name);

        // Create the project using the project service
        await _projectService.AddProjectAsync(model);

        // Generate all files for the project
        foreach (var file in model.Files)
        {
            await _artifactGenerator.GenerateAsync(file);
        }

        // Add project to solution
        _projectService.AddToSolution(model);

        _logger.LogInformation("API Gateway project generated successfully: {ProjectName}", model.Name);
    }
}
