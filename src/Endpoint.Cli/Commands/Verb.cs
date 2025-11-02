// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Endpoint.Core.Artifacts.Abstractions;
using Endpoint.DotNet.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Endpoint.Cli.Commands;

using IFileFactory = Endpoint.DotNet.Artifacts.Files.Factories.IFileFactory;

[Verb("verb")]
public class VerbRequest : IRequest
{
    [Option('n', "name")]
    public string Name { get; set; }

    [Option('d', Required = false)]
    public string Directory { get; set; } = System.Environment.CurrentDirectory;
}

public class VerbRequestHandler : IRequestHandler<VerbRequest>
{
    private readonly IArtifactGenerator _artifactGenerator;
    private readonly ILogger<VerbRequestHandler> _logger;
    private readonly IFileFactory _fileFactory;
    private readonly INamespaceProvider _namespaceProvider;

    public VerbRequestHandler(
        ILogger<VerbRequestHandler> logger,
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

    public async Task Handle(VerbRequest request, CancellationToken cancellationToken)
    {
        var @namespace = _namespaceProvider.Get(request.Directory);

        var tokens = new TokensBuilder()
            .With("Name", (SyntaxToken)request.Name)
            .With("Namespace", (SyntaxToken)@namespace)
            .Build();

        var model = _fileFactory.CreateTemplate("Verb", request.Name, request.Directory, tokens: tokens);

        await _artifactGenerator.GenerateAsync(model);
    }
}
