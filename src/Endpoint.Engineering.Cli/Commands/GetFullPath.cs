// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.Cli.Commands;

[Verb("get-full-path")]
public class GetFullPathRequest : IRequest
{
    [Option('r', "relative")]
    public string RelativePath { get; set; }

    [Option('d', Required = false)]
    public string Directory { get; set; } = System.Environment.CurrentDirectory;
}

public class GetFullPathRequestHandler : IRequestHandler<GetFullPathRequest>
{
    private readonly ILogger<GetFullPathRequestHandler> logger;
    private readonly IFileSystem fileSystem;

    public GetFullPathRequestHandler(
        ILogger<GetFullPathRequestHandler> logger,
        IFileSystem fileSystem)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    public async Task Handle(GetFullPathRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Resolving Relative Path. {relativePath}", request.RelativePath);

        Console.WriteLine(fileSystem.Path.GetFullPath(request.RelativePath, request.Directory));
    }
}