// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Endpoint.DotNet.Artifacts.Services;
using Endpoint.DotNet.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.Cli.Commands;

[Verb("lit-workspace-create")]
public class LitWorkspaceCreateRequest : IRequest
{
    [Option('n', "name")]
    public string Name { get; set; }

    [Option('d', Required = false)]
    public string Directory { get; set; } = Environment.CurrentDirectory;
}

public class LitWorkspaceCreateRequestHandler : IRequestHandler<LitWorkspaceCreateRequest>
{
    private readonly ILogger<LitWorkspaceCreateRequestHandler> logger;
    private readonly ICommandService commandService;
    private readonly ILitService litService;
    private readonly IFileSystem fileSystem;

    public LitWorkspaceCreateRequestHandler(
        ILogger<LitWorkspaceCreateRequestHandler> logger,
        ICommandService commandService,
        ILitService litService,
        IFileSystem fileSystem)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
        this.litService = litService ?? throw new ArgumentNullException(nameof(litService));
        this.fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    public async Task Handle(LitWorkspaceCreateRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handled: {0}", nameof(LitWorkspaceCreateRequestHandler));

        await litService.WorkspaceCreate(request.Name, request.Directory);

        commandService.Start("code .", fileSystem.Path.Combine(request.Directory, request.Name));
    }
}
