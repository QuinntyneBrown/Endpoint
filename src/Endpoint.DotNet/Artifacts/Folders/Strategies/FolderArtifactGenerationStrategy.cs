// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Extensions.Logging;

namespace Endpoint.DotNet.Artifacts.Folders.Strategies;

public class FolderArtifactGenerationStrategy : IArtifactGenerationStrategy<FolderModel>
{
    private readonly ILogger<FolderArtifactGenerationStrategy> logger;
    private readonly IFileSystem fileSystem;
    private readonly IArtifactGenerator artifactGenerator;

    public FolderArtifactGenerationStrategy(
        IFileSystem fileSystem,
        ILogger<FolderArtifactGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    public async Task GenerateAsync( FolderModel model)
    {
        logger.LogInformation("Generating artifact for {0}.", model);

        fileSystem.Directory.CreateDirectory(model.Directory);

        foreach (var fileModel in model.Files)
        {
            await artifactGenerator.GenerateAsync(fileModel);
        }

        foreach (var folder in model.SubFolders)
        {
            await artifactGenerator.GenerateAsync(folder);
        }
    }
}
