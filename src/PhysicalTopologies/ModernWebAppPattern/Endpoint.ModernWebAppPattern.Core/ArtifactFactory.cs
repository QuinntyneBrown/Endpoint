// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DomainDrivenDesign.Core;
using Endpoint.DotNet.Artifacts.Projects;
using Endpoint.DotNet.Artifacts.Solutions;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Endpoint.ModernWebAppPattern.Core;

public class ArtifactFactory : IArtifactFactory
{
    private readonly ILogger<ArtifactFactory> _logger;
    private readonly IDataContextProvider _dataContextProvider;

    public ArtifactFactory(ILogger<ArtifactFactory> logger, IDataContextProvider dataContextProvider)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(dataContextProvider);

        _logger = logger;
        _dataContextProvider = dataContextProvider;
    }

    public async Task<SolutionModel> SolutionCreateAsync(string name, string directory, CancellationToken cancellationToken)
    {
        _logger.LogInformation("SolutionCreateAsync");

        var model = new SolutionModel(name, directory);

        var context = await _dataContextProvider.GetAsync(cancellationToken);

        model.Projects.Add(await MessagingProjectCreateAsync(model.SrcDirectory, cancellationToken));

        model.Projects.Add(await ModelsProjectCreateAsync(model.SrcDirectory, cancellationToken));

        foreach(var boundContext in context.BoundedContexts)
        {
            model.Projects.Add(await ApiProjectCreateAsync(model.SrcDirectory, cancellationToken));
        }

        return model;

    }

    public async Task<ProjectModel> MessagingProjectCreateAsync(string directory, CancellationToken cancellationToken)
    {
        var context = await _dataContextProvider.GetAsync(cancellationToken);

        throw new NotImplementedException();
    }

    public async Task<ProjectModel> ModelsProjectCreateAsync(string directory, CancellationToken cancellationToken)
    {
        var context = await _dataContextProvider.GetAsync(cancellationToken);

        throw new NotImplementedException();
    }

    public async Task<ProjectModel> ApiProjectCreateAsync(string directory, CancellationToken cancellationToken)
    {
        var context = await _dataContextProvider.GetAsync(cancellationToken);

        throw new NotImplementedException();
    }
}

