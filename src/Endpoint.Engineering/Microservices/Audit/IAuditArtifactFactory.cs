// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DotNet.Artifacts.Projects;

namespace Endpoint.Engineering.Microservices.Audit;

/// <summary>
/// Factory interface for creating Audit microservice artifacts.
/// </summary>
public interface IAuditArtifactFactory
{
    void AddCoreFiles(ProjectModel project);

    void AddInfrastructureFiles(ProjectModel project, string microserviceName);

    void AddApiFiles(ProjectModel project, string microserviceName);
}
