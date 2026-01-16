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

    [Option('p', "prompt", Required = false)]
    public string Prompt { get; set; }
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
        var commandBuilder = new System.Text.StringBuilder("claude --continue --print");
        
        if (!string.IsNullOrWhiteSpace(request.Prompt))
        {
            // Escape characters that could cause issues in shell commands
            // This handles both bash and cmd.exe contexts
            var escapedPrompt = request.Prompt
                .Replace("\\", "\\\\") // Escape backslashes first
                .Replace("\"", "\\\"") // Escape double quotes
                .Replace("$", "\\$") // Escape dollar signs (bash variable expansion)
                .Replace("`", "\\`"); // Escape backticks (command substitution)
            
            commandBuilder.Append($" \"{escapedPrompt}\"");
        }
        
        commandBuilder.Append(" --dangerously-skip-permissions --verbose --output-format");
        
        var command = commandBuilder.ToString();
        
        _logger.LogInformation("Executing {Command} in {Directory}", command, request.Directory);

        var exitCode = _commandService.Start(
            command,
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