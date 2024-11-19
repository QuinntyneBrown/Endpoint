﻿// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Linq;
using System.Text;
using Endpoint.DotNet.Services;
using Endpoint.DotNet.Syntax.Documents;
using Microsoft.Extensions.Logging;

namespace Endpoint.DotNet.Artifacts.Files.Strategies;

public abstract class CodeFileIArtifactGenerationStrategy<T> : IArtifactGenerationStrategy<CodeFileModel<T>>
    where T : SyntaxModel
{
    protected readonly ILogger<CodeFileIArtifactGenerationStrategy<T>> logger;
    protected readonly ISyntaxGenerator syntaxGenerator;
    protected readonly IFileSystem fileSystem;
    protected readonly INamespaceProvider namespaceProvider;


    public CodeFileIArtifactGenerationStrategy(
        ISyntaxGenerator syntaxGenerator,
        IFileSystem fileSystem,
        INamespaceProvider namespaceProvider,
        ILogger<CodeFileIArtifactGenerationStrategy<T>> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.syntaxGenerator = syntaxGenerator ?? throw new ArgumentNullException(nameof(syntaxGenerator));
        this.fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        this.namespaceProvider = namespaceProvider ?? throw new ArgumentNullException(nameof(namespaceProvider));
    }

    public async Task GenerateAsync(CodeFileModel<T> model)
    {
        logger.LogInformation("Generating Code File. {name}", model.Name);

        var stringBuilder = new StringBuilder();

        if (typeof(T) != typeof(DocumentModel))
        {
            foreach (var @using in model.Usings)
            {
                stringBuilder.AppendLine($"using {@using.Name};");

                if (@using == model.Usings.Last())
                {
                    stringBuilder.AppendLine();
                }
            }

            var fileNamespace = string.IsNullOrEmpty(model.Namespace) ? namespaceProvider.Get(model.Directory) : model.Namespace;

            if (!string.IsNullOrEmpty(fileNamespace) && fileNamespace != "NamespaceNotFound" && !fileNamespace.Contains(".lib."))
            {
                stringBuilder.AppendLine($"namespace {fileNamespace};");

                stringBuilder.AppendLine();
            }
        }

        stringBuilder.AppendLine(await syntaxGenerator.GenerateAsync(model.Object));

        model.Body = stringBuilder.ToString();

    }
}
