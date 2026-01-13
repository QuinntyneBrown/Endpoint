// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Endpoint.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.Cli.Commands;

[Verb("yolo")]
public class YoloRequest : IRequest
{
    [Option('d', Required = false)]
    public string Directory { get; set; } = System.Environment.CurrentDirectory;
}

public class YoloRequestHandler : IRequestHandler<YoloRequest>
{
    private readonly ILogger<YoloRequestHandler> _logger;
    private readonly ICommandService _commandService;

    public YoloRequestHandler(
        ILogger<YoloRequestHandler> logger,
        ICommandService commandService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
    }

    public async Task Handle(YoloRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Executing claude --dangerously-skip-permissions in {Directory}", request.Directory);

        var exitCode = _commandService.Start(
            "claude --dangerously-skip-permissions",
            request.Directory,
            waitForExit: true);

        if (exitCode == 0)
        {
            _logger.LogInformation("Claude command completed successfully");
        }
        else
        {
            _logger.LogWarning("Claude command exited with code {ExitCode}", exitCode);
        }

        await Task.CompletedTask;
    }
}