// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Endpoint.Cli.Commands;


[Verb("file-input")]
public class FileInputRequest : IRequest
{
    [Option('d', Required = false)]
    public string Directory { get; set; } = System.Environment.CurrentDirectory;
}

public class FileInputRequestHandler : IRequestHandler<FileInputRequest>
{
    private readonly ILogger<FileInputRequestHandler> _logger;
    private readonly ICommandService _commandService;
    private readonly IFileSystem _fileSystem;
    private readonly PeriodicTimer _periodicTimer = new PeriodicTimer(TimeSpan.FromSeconds(1));

    public FileInputRequestHandler(ILogger<FileInputRequestHandler> logger, ICommandService commandService, IFileSystem fileSystem)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(commandService);
        ArgumentNullException.ThrowIfNull(fileSystem);

        _logger = logger;
        _commandService = commandService;
        _fileSystem = fileSystem;
    }

    public async Task Handle(FileInputRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handled: {0}", nameof(FileInputRequestHandler));

        var temporaryFilePath = _fileSystem.Path.GetTempPath() + Guid.NewGuid().ToString() + ".json";

        _fileSystem.File.WriteAllText(temporaryFilePath, string.Empty);

        _commandService.Start($"code -r {temporaryFilePath}");

        await Task.Delay(5000);
        var open = true;

        while(open && await _periodicTimer.WaitForNextTickAsync(cancellationToken))
        {
            try
            {
                var fileStream = _fileSystem.File.Open(temporaryFilePath, FileMode.Open, FileAccess.Read, FileShare.None);

                open = false;

                fileStream.Dispose();
            }
            catch (IOException)
            {
                Console.WriteLine("Open");
            }

        }

        Console.WriteLine(temporaryFilePath);
    }
}