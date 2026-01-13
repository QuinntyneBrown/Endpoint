// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DotNet.Artifacts.Projects;

namespace Endpoint.Engineering.Microservices.Export;

public interface IExportArtifactFactory
{
    void AddCoreFiles(ProjectModel project);
    void AddInfrastructureFiles(ProjectModel project, string microserviceName);
    void AddApiFiles(ProjectModel project, string microserviceName);
}
