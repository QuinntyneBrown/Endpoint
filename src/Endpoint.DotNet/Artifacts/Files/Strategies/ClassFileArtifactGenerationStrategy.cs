// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DotNet.Services;
using Endpoint.DotNet.Syntax.Classes;
using Microsoft.Extensions.Logging;

namespace Endpoint.DotNet.Artifacts.Files.Strategies;

public class ClassFileArtifactGenerationStrategy : CodeFileIArtifactGenerationStrategy<ClassModel>
{
    public ClassFileArtifactGenerationStrategy(ISyntaxGenerator syntaxGenerator, IFileSystem fileSystem, INamespaceProvider namespaceProvider, IGenericArtifactGenerationStrategy<FileModel> fileArtifactGenerationStrategy, ILogger<CodeFileIArtifactGenerationStrategy<ClassModel>> logger)
        : base(syntaxGenerator, fileSystem, namespaceProvider, fileArtifactGenerationStrategy, logger)
    {
    }
}
