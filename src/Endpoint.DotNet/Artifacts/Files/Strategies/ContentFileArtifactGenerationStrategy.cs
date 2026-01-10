// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Core.Artifacts.Abstractions;

namespace Endpoint.DotNet.Artifacts.Files.Strategies;

public class ContentFileArtifactGenerationStrategy : IArtifactGenerationStrategy<ContentFileModel>
{
    private readonly IFileSystem fileSystem;

    public ContentFileArtifactGenerationStrategy(

        IFileSystem fileSystem)
    {
        this.fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    public async Task GenerateAsync(ContentFileModel model)
    {
        fileSystem.File.WriteAllText(model.Path, model.Content);
    }
}
