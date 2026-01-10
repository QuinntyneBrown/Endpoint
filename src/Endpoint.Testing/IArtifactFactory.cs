// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Angular.Artifacts;
using Endpoint.DotNet.Artifacts.Solutions;


namespace Endpoint.Testing;

public interface IArtifactFactory
{
    Task<SolutionModel> SolutionCreateAsync(string systemName, string resourceName, string directory, CancellationToken cancellationToken);

    Task<WorkspaceModel> AngularWorkspaceCreateAsync(string systemName, string resourceName, string directory, CancellationToken cancellationToken);

}

