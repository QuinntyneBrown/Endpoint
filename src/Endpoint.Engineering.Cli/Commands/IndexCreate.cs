// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Endpoint.Artifacts.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.Cli.Commands;

using IFileFactory = Endpoint.Angular.Artifacts.IFileFactory;

[Verb(".")]
public class IndexCreateRequest : IRequest
{
    [Option('d', Required = false)]
    public string Directory { get; set; } = System.Environment.CurrentDirectory;
}

public class IndexCreateRequestHandler : IRequestHandler<IndexCreateRequest>
{
    private readonly ILogger<IndexCreateRequestHandler> _logger;
    private readonly IFileFactory _fileFactory;
    private readonly IArtifactGenerator _artifactGenerator;

    public IndexCreateRequestHandler(
        ILogger<IndexCreateRequestHandler> logger,
        IFileFactory fileFactory,
        IArtifactGenerator artifactGenerator)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _fileFactory = fileFactory ?? throw new ArgumentNullException(nameof(fileFactory));
        _artifactGenerator = artifactGenerator;
    }

    public async Task Handle(IndexCreateRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handled: {0}", nameof(IndexCreateRequestHandler));

        foreach (var file in await _fileFactory.IndexCreate(request.Directory))
        {
            await _artifactGenerator.GenerateAsync(file);
        }
    }
}