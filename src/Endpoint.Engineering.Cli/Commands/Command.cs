// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Endpoint.Artifacts.Abstractions;
using Endpoint.DotNet.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.Cli.Commands;

using IFileFactory = Endpoint.DotNet.Artifacts.Files.Factories.IFileFactory;

[Verb("command")]
public class CommandRequest : IRequest
{
    [Option('n', "name", HelpText = "The name of the command to generate")]
    public string Name { get; set; }

    [Option('d', Required = false, HelpText = "The target directory")]
    public string Directory { get; set; } = System.Environment.CurrentDirectory;
}

public class CommandRequestHandler : IRequestHandler<CommandRequest>
{
    private readonly IArtifactGenerator _artifactGenerator;
    private readonly ILogger<CommandRequestHandler> _logger;
    private readonly IFileFactory _fileFactory;
    private readonly INamespaceProvider _namespaceProvider;

    public CommandRequestHandler(
        ILogger<CommandRequestHandler> logger,
        IArtifactGenerator artifactGenerator,
        IFileFactory fileFactory,
        INamespaceProvider namespaceProvider)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(artifactGenerator);
        ArgumentNullException.ThrowIfNull(fileFactory);
        ArgumentNullException.ThrowIfNull(namespaceProvider);

        _artifactGenerator = artifactGenerator;
        _logger = logger;
        _fileFactory = fileFactory;
        _namespaceProvider = namespaceProvider;
    }

    public async Task Handle(CommandRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generating System.CommandLine command: {Name}", request.Name);

        var @namespace = _namespaceProvider.Get(request.Directory);

        var tokens = new TokensBuilder()
            .With("Name", (SyntaxToken)request.Name)
            .With("Namespace", (SyntaxToken)@namespace)
            .Build();

        var model = _fileFactory.CreateTemplate("Command", $"{request.Name}Command", request.Directory, tokens: tokens);

        await _artifactGenerator.GenerateAsync(model);

        _logger.LogInformation("Command generated: {Name}Command.cs", request.Name);
    }
}
