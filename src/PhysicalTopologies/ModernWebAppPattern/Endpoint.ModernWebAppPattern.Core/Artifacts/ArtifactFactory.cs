// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DotNet.Artifacts.Projects;
using Endpoint.DotNet.Artifacts.Solutions;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;

namespace Endpoint.ModernWebAppPattern.Core.Artifacts;

using Microservice = Endpoint.ModernWebAppPattern.Core.Models.Microservice;

public class ArtifactFactory : IArtifactFactory
{
    private readonly ILogger<ArtifactFactory> _logger;
    private readonly IDataContextProvider _dataContextProvider;
    private readonly IFileSystem _fileSystem;

    public ArtifactFactory(ILogger<ArtifactFactory> logger, IDataContextProvider dataContextProvider, IFileSystem fileSystem)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(dataContextProvider);
        ArgumentNullException.ThrowIfNull(fileSystem);

        _logger = logger;
        _dataContextProvider = dataContextProvider;
        _fileSystem = fileSystem;
    }

    public async Task<SolutionModel> SolutionCreateAsync(string path, string name, string directory, CancellationToken cancellationToken)
    {
        _logger.LogInformation("SolutionCreateAsync");

        var model = new SolutionModel(name, directory);

        var context = await _dataContextProvider.GetAsync(path, cancellationToken);

        model.Projects.Add(await MessagingProjectCreateAsync(model.SrcDirectory, cancellationToken));

        model.Projects.Add(await ModelsProjectCreateAsync(model.SrcDirectory, cancellationToken));

        foreach (var microservice in context.Microservices)
        {
            model.Projects.Add(await ApiProjectCreateAsync(microservice, model.SrcDirectory, cancellationToken));
        }

        return model;

    }

    public async Task<ProjectModel> MessagingProjectCreateAsync(string directory, CancellationToken cancellationToken)
    {
        var context = await _dataContextProvider.GetAsync(cancellationToken: cancellationToken);

        var model = new ProjectModel($"{context.ProductName}.Messaging", directory);

        var servicesDirectory = _fileSystem.Path.Combine(model.Directory, "Services");

        var messagesDirectory = _fileSystem.Path.Combine(model.Directory, "Messages");

        model.DotNetProjectType = DotNet.Artifacts.Projects.Enums.DotNetProjectType.ClassLib;

        return model;
    }

    public async Task<ProjectModel> ModelsProjectCreateAsync(string directory, CancellationToken cancellationToken)
    {
        var context = await _dataContextProvider.GetAsync(cancellationToken: cancellationToken);

        var model = new ProjectModel($"{context.ProductName}.Models", directory);

        model.DotNetProjectType = DotNet.Artifacts.Projects.Enums.DotNetProjectType.ClassLib;

        return model;
    }

    public async Task<ProjectModel> ApiProjectCreateAsync(Microservice microservice, string directory, CancellationToken cancellationToken)
    {
        var context = await _dataContextProvider.GetAsync(cancellationToken: cancellationToken);

        var model = new ProjectModel(microservice.Name, directory);

        model.DotNetProjectType = DotNet.Artifacts.Projects.Enums.DotNetProjectType.WebApi;

        var boundedContext = context.BoundedContexts.Single(x => x.Name == microservice.BoundedContextName);

        return model;
    }
}

