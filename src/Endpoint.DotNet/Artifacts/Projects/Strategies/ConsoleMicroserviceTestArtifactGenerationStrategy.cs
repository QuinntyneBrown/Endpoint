// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DotNet.Services;

namespace Endpoint.DotNet.Artifacts.Projects.Strategies;

public class ConsoleMicroserviceTestArtifactGenerationStrategy : GenericArtifactGenerationStrategy<ConsoleMicroserviceProjectModel>
{
    private readonly IFileSystem fileSytem;

    public ConsoleMicroserviceTestArtifactGenerationStrategy(IServiceProvider serviceProvider, IFileSystem fileSystem)
    {
        fileSytem = fileSystem;
    }

    public override async Task GenerateAsync(IArtifactGenerator artifactGenerator, ConsoleMicroserviceProjectModel model)
    {
        throw new NotImplementedException();
    }
}
