// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DotNet.Artifacts.Solutions;
using Endpoint.DotNet.Artifacts.Workspaces;

namespace Endpoint.Testing.Core;

public interface IArtifactFactory
{
    Task<SolutionModel> SolutionCreateAsync(string systemName, string resourceName, string directory, CancellationToken cancellationToken);

    Task<AngularWorkspaceModel> AngularWorkspaceCreateAsync(string systemName, string resourceName, string directory, CancellationToken cancellationToken);

}

