// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Core.Artifacts.Abstractions;
using Endpoint.DotNet.Services;
using Endpoint.DotNet.Syntax.Classes;
using Microsoft.Extensions.Logging;

namespace Endpoint.DotNet.Artifacts.Files.Strategies;

public class ClassCodeFileArtifactGenerationStrategy : CodeFileIArtifactGenerationStrategy<ClassModel>
{
    public ClassCodeFileArtifactGenerationStrategy( IArtifactGenerator artifactGenerator, ISyntaxGenerator syntaxGenerator, IFileSystem fileSystem, INamespaceProvider namespaceProvider, ILogger<CodeFileIArtifactGenerationStrategy<ClassModel>> logger) 
        : base(syntaxGenerator, fileSystem, namespaceProvider, artifactGenerator, logger)
    {
    }
}
