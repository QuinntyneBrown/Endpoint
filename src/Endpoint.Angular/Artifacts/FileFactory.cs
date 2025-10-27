// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Core.Artifacts;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;

namespace Endpoint.Angular.Artifacts;

public class FileFactory: IFileFactory
{
    private readonly ILogger<FileFactory> _logger;
    private readonly IFileSystem _fileSystem;

    public FileFactory(ILogger<FileFactory> logger, IFileSystem fileSystem)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(fileSystem);

        _logger = logger;
        _fileSystem = fileSystem;
    }

    public async Task<List<FileModel>> IndexCreate(string directory, bool scss = false)
    {
        _logger.LogInformation("Index Create");

        List<FileModel> result = [];

        foreach(var path in _fileSystem.Directory.GetDirectories(directory))
        {
            List<string> lines = [];

            var files = _fileSystem.Directory.GetFiles(path);

            var fileNames = _fileSystem.Directory.GetFiles(path).Select(Path.GetFileNameWithoutExtension);

            var containsIndex = _fileSystem.Directory.GetFiles(path)
                .Select(Path.GetFileNameWithoutExtension)
            .Contains("index");

            if (!scss && _fileSystem.Directory.GetFiles(path)
                .Select(Path.GetFileNameWithoutExtension)
                .Contains("index"))
            {
                lines.Add($"export * from './{_fileSystem.Path.GetFileNameWithoutExtension(path)}';");
            }

            if (scss)
            {
                foreach (var file in Directory.GetFiles(directory, "*.scss"))
                {
                    if (!file.EndsWith("index.scss"))
                    {
                        lines.Add($"@use './{Path.GetFileNameWithoutExtension(file)}';");
                    }
                }

                result.Add(new()
                {
                    Name = "index",
                    Extension = ".scss",
                    Directory = directory,
                    Body = string.Join(System.Environment.NewLine, lines)
                });
            }
            else
            {
                foreach (var file in Directory.GetFiles(directory, "*.ts"))
                {
                    if (!file.Contains(".spec.") && !file.EndsWith("index.ts"))
                    {
                        lines.Add($"export * from './{Path.GetFileNameWithoutExtension(file)}';");
                    }
                }

                result.Add(new()
                {
                    Name = "index",
                    Extension = ".ts",
                    Directory = directory,
                    Body = string.Join(System.Environment.NewLine, lines)
                });
            }
        }

        return result;
    }
}

