// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Endpoint.Artifacts.Abstractions;
using Endpoint.DotNet.Artifacts;
using Endpoint.DotNet.Artifacts.Files.Factories;
using Endpoint.DotNet.Artifacts.Solutions.Factories;
using Endpoint.DotNet.Artifacts.Solutions.Services;
using Endpoint.DotNet.Services;
using Endpoint.DotNet.Syntax.Classes.Factories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.Cli.Commands;

using IFileFactory = Endpoint.DotNet.Artifacts.Files.Factories.IFileFactory;

[Verb("ws-app-create")]
public class WebSocketAppCreateRequest : IRequest
{
    [Option('n', "name")]
    public string Name { get; set; }

    [Option('d', Required = false)]
    public string Directory { get; set; } = System.Environment.CurrentDirectory;
}

public class WebSocketAppCreateRequestHandler : IRequestHandler<WebSocketAppCreateRequest>
{
    private readonly ILogger<WebSocketAppCreateRequestHandler> logger;
    private readonly ISolutionService solutionService;
    private readonly ISolutionFactory solutionFactory;
    private readonly IArtifactGenerator generator;
    private readonly INamingConventionConverter namingConventionConverter;
    private readonly ICommandService commandService;
    private readonly IClassFactory classFactory;
    private readonly IFileProvider fileProvider;
    private readonly IFileSystem fileSystem;
    private readonly IFileFactory fileFactory;

    public WebSocketAppCreateRequestHandler(
        ILogger<WebSocketAppCreateRequestHandler> logger,
        ISolutionService solutionService,
        ISolutionFactory solutionFactory,
        IArtifactGenerator artifactGenerator,
        INamingConventionConverter namingConventionConverter,
        ICommandService commandService,
        IClassFactory classFactory,
        IFileProvider fileProvider,
        IFileSystem fileSystem,
        IFileFactory fileFactory)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.solutionService = solutionService ?? throw new ArgumentNullException(nameof(solutionService));
        this.solutionFactory = solutionFactory;
        generator = artifactGenerator ?? throw new ArgumentNullException(nameof(artifactGenerator));
        this.namingConventionConverter = namingConventionConverter ?? throw new ArgumentNullException(nameof(namingConventionConverter));
        this.commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
        this.classFactory = classFactory ?? throw new ArgumentNullException(nameof(classFactory));
        this.fileProvider = fileProvider ?? throw new ArgumentNullException(nameof(fileProvider));
        this.fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        this.fileFactory = fileFactory ?? throw new ArgumentNullException(nameof(fileFactory));
    }

    public async Task Handle(WebSocketAppCreateRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handled: {0}", nameof(WebSocketAppCreateRequestHandler));

        var model = await solutionFactory.Create(request.Name, request.Name, "web", string.Empty, request.Directory);

        if (fileSystem.Directory.Exists(model.SolutionDirectory))
        {
            fileSystem.Directory.Delete(model.SolutionDirectory);
        }

        await generator.GenerateAsync(model);

        commandService.Start($"start {model.SolultionFileName}", model.SolutionDirectory);
    }
}
