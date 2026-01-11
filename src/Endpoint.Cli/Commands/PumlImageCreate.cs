// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Endpoint.DotNet.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using static System.Environment;
using static System.Environment.SpecialFolder;

namespace Endpoint.Cli.Commands;

[Verb("puml-image-create")]
public class PlantUmlImageCreateRequest : IRequest
{
    [Option('i', "input-path", Required = true)]
    public string InputPath { get; set; }

    [Option('p', "plantuml-path")]
    public string PlantUmlPath { get; set; }

    [Option('j', "java-path")]
    public string JavaPath { get; set; }

    [Option('d', Required = false)]
    public string Directory { get; set; } = System.Environment.CurrentDirectory;
}

public class ImageCreateRequestHandler : IRequestHandler<PlantUmlImageCreateRequest>
{
    private readonly ILogger<ImageCreateRequestHandler> logger;
    private readonly ICommandService commandService;
    private readonly IFileSystem fileSystem;

    public ImageCreateRequestHandler(
        ILogger<ImageCreateRequestHandler> logger, 
        ICommandService commandService,
        IFileSystem fileSystem)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
        this.fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    public async Task Handle(PlantUmlImageCreateRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating Image", nameof(ImageCreateRequestHandler));

        var plantUmlPath = request.PlantUmlPath ?? fileSystem.Path.Combine(GetFolderPath(UserProfile), ".dotnet", "tools", "plantuml.jar");
        var javaPath = request.JavaPath ?? $"\"{fileSystem.Path.Combine(GetFolderPath(ProgramFilesX86), "Java", "jre-1.8", "bin", "java.exe")}\"";

        commandService.Start($"{javaPath} -jar {plantUmlPath} {request.InputPath}", request.Directory);
    }
}