// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Core.Artifacts;
using Endpoint.DomainDrivenDesign.Core.Models;
using Endpoint.DotNet.Artifacts.Projects;
using Endpoint.DotNet.Artifacts.Solutions;
using Endpoint.ModernWebAppPattern.Core.Models;
using System.Text.Json;

namespace Endpoint.ModernWebAppPattern.Core.Artifacts;

public interface IArtifactFactory
{
    Task<SolutionModel> SolutionCreateAsync(string path, string name, string directory, CancellationToken cancellationToken);

    Task<SolutionModel> SolutionCreateAsync(JsonElement jsonElement, string name, string directory, CancellationToken cancellationToken);

    Task<IEnumerable<FileModel>> AggregateCreateAsync(Endpoint.DomainDrivenDesign.Core.IDataContext context, Aggregate aggregate, string directory, CancellationToken cancellationToken);

    Task<FileModel> CommandValidatorCreateAsync(Command command, string directory);

    Task<FileModel> AggregateExtenionCreateAsync(Aggregate aggregate, string directory);

    Task<FileModel> ApiConfigureServicesCreateAsync(Microservice microservice, string directory);

    Task<FileModel> ApiProgramCreateAsync(BoundedContext boundedContext, Microservice microservice, string directory);

    Task<FileModel> ApiAppSettingsCreateAsync(BoundedContext boundedContext, string directory);

    Task<ProjectModel> ModelsProjectCreateAsync(string directory, CancellationToken cancellationToken);

    Task<FileModel> ControllerCreateAsync(Microservice microservice, Aggregate aggregate, string directory);
}

