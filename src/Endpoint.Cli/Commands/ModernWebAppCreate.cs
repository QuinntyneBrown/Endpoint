// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Endpoint.ModernWebAppPattern.Core.Artifacts;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Endpoint.Cli.Commands;

[Verb("modern-web-app-create")]
public class ModernWebAppCreateRequest : IRequest
{
    [Option('n', "name")]
    public string Name { get; set; } = Environment.GetEnvironmentVariable("MWA_NAME", EnvironmentVariableTarget.Machine) !;

    [Option('p', "path")]
    public string Path { get; set; } = Environment.GetEnvironmentVariable("MWA_PATH", EnvironmentVariableTarget.Machine) !;

    [Option('d', Required = false)]
    public string Directory { get; set; } = Environment.GetEnvironmentVariable("MWA_DIRECTORY", EnvironmentVariableTarget.Machine) !;
}

public class ModernWebAppCreateRequestHandler : IRequestHandler<ModernWebAppCreateRequest>
{
    private readonly ILogger<ModernWebAppCreateRequestHandler> _logger;
    private readonly IArtifactFactory _artifactFactory;
    private readonly IArtifactGenerator _artifactGenerator;

    public ModernWebAppCreateRequestHandler(ILogger<ModernWebAppCreateRequestHandler> logger, IArtifactGenerator artifactGenerator, IArtifactFactory artifactFactory)
    {
        _logger = logger;
        _artifactGenerator = artifactGenerator;
        _artifactFactory = artifactFactory;
    }

    public async Task Handle(ModernWebAppCreateRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handled: {0}", nameof(ModernWebAppCreateRequestHandler));

        var model = await _artifactFactory.SolutionCreateAsync(request.Path, request.Name, request.Directory, cancellationToken);

        await _artifactGenerator.GenerateAsync(model);
    }
}