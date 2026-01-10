// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Artifacts.Abstractions;
using Endpoint.DotNet.Services;
using Endpoint.DotNet.Syntax.Interfaces;
using Microsoft.Extensions.Logging;

namespace Endpoint.DotNet.Artifacts.Files.Strategies;

public class InterfaceCodeFileArtifactGenerationStrategy : CodeFileIArtifactGenerationStrategy<InterfaceModel>
{
    public InterfaceCodeFileArtifactGenerationStrategy(IArtifactGenerator artifactGenerator, ISyntaxGenerator syntaxGenerator, IFileSystem fileSystem, INamespaceProvider namespaceProvider, ILogger<CodeFileIArtifactGenerationStrategy<InterfaceModel>> logger)
        : base(syntaxGenerator, fileSystem, namespaceProvider, artifactGenerator, logger)
    {
    }
}
