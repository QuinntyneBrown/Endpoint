// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Endpoint.Artifacts.Abstractions;
using Endpoint.DotNet.Artifacts;
using Endpoint.DotNet.Artifacts.Git;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Endpoint.Cli.Commands;

[Verb("git-create")]
public class GitCreateRequest : IRequest
{
    [Option('n', "name")]
    public string RepositoryName { get; set; }

    [Option('d', Required = false)]
    public string Directory { get; set; } = Environment.CurrentDirectory;
}

public class GitCreateRequestHandler : IRequestHandler<GitCreateRequest>
{
    private readonly ILogger<GitCreateRequestHandler> logger;
    private readonly IArtifactGenerator artifactGenerator;
    private readonly IFileSystem fileSystem;

    public GitCreateRequestHandler(
        ILogger<GitCreateRequestHandler> logger,
        IArtifactGenerator artifactGenerator,
        IFileSystem fileSystem)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.artifactGenerator = artifactGenerator ?? throw new ArgumentNullException(nameof(artifactGenerator));
        this.fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    public async Task Handle(GitCreateRequest request, CancellationToken cancellationToken)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Creating Git Repository");
        }

        var model = new GitModel(request.RepositoryName)
        {
            Directory = fileSystem.Path.Combine(request.Directory, request.RepositoryName),
            Username = "QuinntyneBrown",
        };

        await artifactGenerator.GenerateAsync(model);
    }
}
