// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DotNet.Artifacts.Projects;

namespace Endpoint.Engineering.Microservices.ConfigurationManagement;

/// <summary>
/// Factory interface for creating Configuration Management microservice artifacts.
/// </summary>
public interface IConfigurationManagementArtifactFactory
{
    void AddCoreFiles(ProjectModel project);

    void AddInfrastructureFiles(ProjectModel project, string microserviceName);

    void AddInfrastructureSeederFiles(ProjectModel project);

    void AddApiFiles(ProjectModel project, string microserviceName);
}
