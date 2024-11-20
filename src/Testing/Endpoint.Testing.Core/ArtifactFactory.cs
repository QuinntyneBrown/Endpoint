// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DotNet.Artifacts.AngularProjects;
using Endpoint.DotNet.Artifacts.Projects;
using Endpoint.DotNet.Artifacts.Solutions;
using Endpoint.DotNet.Artifacts.Workspaces;
using Microsoft.Extensions.Logging;
using static Endpoint.DotNet.Constants;

namespace Endpoint.Testing.Core;

public class ArtifactFactory : IArtifactFactory
{
    private readonly ILogger<ArtifactFactory> _logger;

    public ArtifactFactory(ILogger<ArtifactFactory> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;

    }

    public async Task<SolutionModel> SolutionCreateAsync(string systemName, string resourceName, string directory, CancellationToken cancellationToken)
    {
        _logger.LogInformation("SolutionCreateAsync");

        var model = new SolutionModel(systemName, directory);

        var projectModel = new ProjectModel("webapi", $"{systemName}.Api",model.SrcDirectory);

        model.Projects.Add(projectModel);

        return model;

    }

    public async Task<AngularWorkspaceModel> AngularWorkspaceCreateAsync(string systemName, string resourceName, string directory, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Angular Workspace Create Async");

        var solutionModel = await SolutionCreateAsync(systemName, resourceName, directory, cancellationToken);

        var model = new AngularWorkspaceModel($"{systemName}.App", string.Empty, solutionModel.SrcDirectory);

        var projectModel = new AngularProjectModel("app", "application", "app", model.Directory);

        model.Projects.Add(projectModel);

        return model;
    }

}

