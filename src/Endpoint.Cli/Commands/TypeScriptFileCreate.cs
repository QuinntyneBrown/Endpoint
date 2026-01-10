// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Endpoint.Core.Artifacts.Abstractions;
using Humanizer;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Endpoint.Cli.Commands;

[Verb("ts-file-create")]
public class TypeScriptFileCreateRequest : IRequest
{
    [Option('n', "name")]
    public string Name { get; set; }

    [Option('d', Required = false)]
    public string Directory { get; set; } = System.Environment.CurrentDirectory;
}

public class TypeScriptFileCreateRequestHandler : IRequestHandler<TypeScriptFileCreateRequest>
{
    private readonly ILogger<TypeScriptFileCreateRequestHandler> _logger;
    private readonly IArtifactGenerator _artifactGenerator;
    private readonly ICommandService _commandService;

    public TypeScriptFileCreateRequestHandler(ILogger<TypeScriptFileCreateRequestHandler> logger, IArtifactGenerator artifactGenerator, ICommandService commandService)
    {
        ArgumentNullException.ThrowIfNull(artifactGenerator);

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _artifactGenerator = artifactGenerator;
        _commandService = commandService;
    }

    public async Task Handle(TypeScriptFileCreateRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handled: {0}", nameof(TypeScriptFileCreateRequestHandler));

        await _artifactGenerator.GenerateAsync(new FileModel(
            request.Name.Kebaberize(),
            request.Directory,
            ".ts"));

        _commandService.Start("endpoint .", request.Directory);
    }
}