// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Endpoint.Artifacts.Abstractions;
using Endpoint.DotNet.Artifacts.Solutions.Factories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Endpoint.Cli.Commands;

[Verb("solution-create")]
public class SolutionCreateRequest : IRequest
{
    [Option('n', "name")]
    public string Name { get; set; }

    [Option('p')]
    public string ProjectName { get; set; } = "Worker.Console";

    [Option('f')]
    public string FolderName { get; set; }

    [Option("no-service-create")]
    public bool NoServiceCreate { get; set; }

    [Option('t')]
    public string ProjectType { get; set; } = "worker";

    [Option('d', Required = false)]
    public string Directory { get; set; } = System.Environment.CurrentDirectory;
}

public class SolutionCreateRequestHandler : IRequestHandler<SolutionCreateRequest>
{
    private readonly ILogger<SolutionCreateRequestHandler> _logger;
    private readonly ISolutionFactory _solutionFactory;
    private readonly IArtifactGenerator _artifactGenerator;
    private readonly ICommandService _commandService;

    public SolutionCreateRequestHandler(
        ILogger<SolutionCreateRequestHandler> logger,
        IArtifactGenerator artifactGenerator,
        ISolutionFactory solutionFactory,
        ICommandService commandService)
    {
        _logger = logger;
        _artifactGenerator = artifactGenerator;
        _solutionFactory = solutionFactory;
        _commandService = commandService;
    }

    public async Task Handle(SolutionCreateRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating Solution. {name}", request.Name);

        var model = await _solutionFactory.Create(request.Name, request.ProjectName, request.ProjectType, request.FolderName, request.Directory);

        if (request.NoServiceCreate)
        {
            model.RemoveAllServices();
        }

        await _artifactGenerator.GenerateAsync(model);

        _commandService.Start($"code .", model.SolutionDirectory);
    }
}
