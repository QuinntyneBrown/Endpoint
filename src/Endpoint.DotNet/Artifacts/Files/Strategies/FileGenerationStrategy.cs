// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO.Abstractions;
using System.Linq;
using System.Text;
using Endpoint.Artifacts.Abstractions;
using Endpoint.DotNet.Extensions;
using Endpoint.DotNet.Services;
using Microsoft.Extensions.Logging;
using static Endpoint.DotNet.Constants;

namespace Endpoint.DotNet.Artifacts.Files.Strategies;

public class FileGenerationStrategy : IArtifactGenerationStrategy<FileModel>
{
    protected readonly IFileSystem fileSystem;
    protected readonly ITemplateLocator templateLocator;
    private readonly ILogger<FileGenerationStrategy> logger;

    public FileGenerationStrategy(
        ILogger<FileGenerationStrategy> logger,
        IFileSystem fileSystem,
        ITemplateLocator templateLocator)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        this.templateLocator = templateLocator ?? throw new ArgumentNullException(nameof(templateLocator));
    }

    public async Task GenerateAsync(FileModel model)
    {
        logger.LogInformation("Generating file artifact. {name}", model.Name);

        var copyright = templateLocator.GetCopyright();

        var parts = fileSystem.Path.GetDirectoryName(model.Path).Split(fileSystem.Path.DirectorySeparatorChar);

        for (var i = 1; i <= parts.Length; i++)
        {
            var dir = string.Join(fileSystem.Path.DirectorySeparatorChar, parts.Take(i));

            if (!fileSystem.Directory.Exists(dir))
            {
                fileSystem.Directory.CreateDirectory(dir);
            }
        }

        var extension = fileSystem.Path.GetExtension(model.Path);

        var validExtension = extension == ".cs" || extension == ".ts" || extension == ".js";

        var raw = validExtension && !ExcludePatterns.Any(model.Path.Contains)
            ? new StringBuilder().AppendJoin(Environment.NewLine, copyright, string.Empty, model.Body).ToString()
            : model.Body;

        fileSystem.File.WriteAllText(model.Path, model.Path.EndsWith(".cs") ? raw : raw);
    }
}
