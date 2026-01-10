// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Endpoint.Artifacts.Abstractions;
using Endpoint.DotNet.Artifacts;
using Endpoint.DotNet.Artifacts.Projects;
using Endpoint.DotNet.Artifacts.Projects.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Endpoint.Cli.Commands;

[Verb("ts-project-create")]
public class TypescriptStandaloneCreateRequest : IRequest
{
    [Option('n', "name")]
    public string Name { get; set; }

    [Option('d', Required = false)]
    public string Directory { get; set; } = System.Environment.CurrentDirectory;
}

public class TypescriptStandaloneCreateRequestHandler : IRequestHandler<TypescriptStandaloneCreateRequest>
{
    private readonly ILogger<TypescriptStandaloneCreateRequestHandler> logger;
    private readonly IArtifactGenerator artifactGenerator;

    public TypescriptStandaloneCreateRequestHandler(ILogger<TypescriptStandaloneCreateRequestHandler> logger, IArtifactGenerator artifactGenerator)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.artifactGenerator = artifactGenerator ?? throw new ArgumentNullException(nameof(artifactGenerator));
    }

    public async Task Handle(TypescriptStandaloneCreateRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handled: {0}", nameof(TypescriptStandaloneCreateRequestHandler));

        var model = new ProjectModel(DotNetProjectType.TypeScriptStandalone, request.Name, request.Directory);

        await artifactGenerator.GenerateAsync(model);
    }
}