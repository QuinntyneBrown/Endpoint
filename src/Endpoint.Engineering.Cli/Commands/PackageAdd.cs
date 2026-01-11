// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Endpoint.DotNet.Artifacts.Projects.Services;
using Endpoint.DotNet.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.Cli.Commands;

[Verb("package-add")]
public class PackageAddRequest : IRequest
{
    [Option('n', "name")]
    public string Name { get; set; }

    [Option('d', Required = false)]
    public string Directory { get; set; } = System.Environment.CurrentDirectory;
}

public class PackageAddRequestHandler : IRequestHandler<PackageAddRequest>
{
    private readonly ILogger<PackageAddRequestHandler> logger;
    private readonly IProjectService projectService;

    public PackageAddRequestHandler(ILogger<PackageAddRequestHandler> logger, IProjectService projectService)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
    }

    public async Task Handle(PackageAddRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handled: {0}", nameof(PackageAddRequestHandler));

        projectService.PackageAdd(request.Name, request.Directory);
    }
}
