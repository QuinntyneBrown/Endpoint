// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.DotNet.Artifacts.Projects;

public class PackageModel : ArtifactModel
{
    public PackageModel(string name, string verison)
    {
        Name = name;
        Version = verison;
    }

    public PackageModel()
    {
    }

    public string Name { get; init; }

    public string Version { get; init; }

    public bool IsPreRelease { get; init; }
}
