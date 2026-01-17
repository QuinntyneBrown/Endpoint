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

[Verb("command-issue-create")]
public class CommandIssueCreateRequest : IRequest
{
    [Option('p', "prompt", Required = true)]
    public string Prompt { get; set; }

    [Option('t', "type", Required = true)]
    public string Type { get; set; }
}

public class CommandIssueCreateRequestHandler : IRequestHandler<CommandIssueCreateRequest>
{
    private readonly ILogger<CommandIssueCreateRequestHandler> logger;
    private readonly IClipboardService clipboardService;

    public CommandIssueCreateRequestHandler(
        ILogger<CommandIssueCreateRequestHandler> logger,
        IClipboardService clipboardService)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.clipboardService = clipboardService ?? throw new ArgumentNullException(nameof(clipboardService));
    }

    public async Task Handle(CommandIssueCreateRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handled: {0}", nameof(CommandIssueCreateRequestHandler));

        var output = $@"In Endpoint.Engineering.Cli

Create a new command called ""{request.Type}""

Create a solution in the playground folder demonstrating the functionality of {request.Type}

* Features
{request.Prompt}

* Tests
Add Unit Tests so that there is 80% Test coverage
";

        clipboardService.SetText(output);

        logger.LogInformation("Issue template copied to clipboard");
    }
}
