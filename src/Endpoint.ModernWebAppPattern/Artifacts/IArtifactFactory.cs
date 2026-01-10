// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Artifacts;
using Endpoint.DomainDrivenDesign.Models;
using Endpoint.DotNet.Artifacts.Projects;
using Endpoint.DotNet.Artifacts.Solutions;
using Endpoint.ModernWebAppPattern.Models;
using System.Text.Json;

namespace Endpoint.ModernWebAppPattern.Artifacts;

public interface IArtifactFactory
{
    Task<SolutionModel> SolutionCreateAsync(string path, string name, string directory, CancellationToken cancellationToken);

    Task<SolutionModel> SolutionCreateAsync(JsonElement jsonElement, string name, string directory, CancellationToken cancellationToken);

    Task<IEnumerable<FileModel>> AggregateCreateAsync(Endpoint.DomainDrivenDesign.IDataContext context, AggregateModel aggregate, string directory, CancellationToken cancellationToken);

    Task<FileModel> CommandValidatorCreateAsync(Command command, string directory);

    Task<FileModel> AggregateExtenionCreateAsync(AggregateModel aggregate, string directory);

    Task<FileModel> ApiConfigureServicesCreateAsync(Microservice microservice, string directory);

    Task<FileModel> ApiProgramCreateAsync(BoundedContext boundedContext, Microservice microservice, string directory);

    Task<FileModel> ApiAppSettingsCreateAsync(BoundedContext boundedContext, string directory);

    Task<ProjectModel> ModelsProjectCreateAsync(string directory, CancellationToken cancellationToken);

    Task<FileModel> ControllerCreateAsync(Microservice microservice, AggregateModel aggregate, string directory);
}

