// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DotNet.Artifacts.Solutions;

namespace Endpoint.ModernWebAppPattern.Core.Artifacts;

public interface IArtifactFactory
{
    Task<SolutionModel> SolutionCreateAsync(string path, string name, string directory, CancellationToken cancellationToken);

}

