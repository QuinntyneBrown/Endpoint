// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CommandLine;
using Endpoint.DotNet.Services;
using Endpoint.DotNet.Syntax;
using Endpoint.ModernWebAppPattern.Core.Artifacts;
using Endpoint.ModernWebAppPattern.Core.Syntax;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Endpoint.Cli.Commands;

[Verb("ddd-app-create")]
public class DddAppCreateRequest : IRequest
{
    [Option('n', "product-name")]
    public string ProductName { get; set; }

    [Option('b', "bounded-context")]
    public string BoundedContext { get; set; } = "ToDos";

    [Option('a', "aggregate")]
    public string Aggregate { get; set; } = "ToDo";

    [Option('p', "properties")]
    public string Properties { get; set; } = "ToDoId:Guid,Title:String,IsComplete:String";

    [Option('d', Required = false)]
    public string Directory { get; set; } = System.Environment.CurrentDirectory;
}

public class DddAppCreateRequestHandler : IRequestHandler<DddAppCreateRequest>
{
    private readonly ILogger<DddAppCreateRequestHandler> _logger;
    private readonly ISyntaxGenerator _syntaxGenerator;
    private readonly IArtifactGenerator _artifactGenerator;
    private readonly IFileSystem _fileSystem;
    private readonly IArtifactFactory _artifactFactory;
    private readonly ICommandService _commandService;

    public DddAppCreateRequestHandler(ILogger<DddAppCreateRequestHandler> logger, ISyntaxGenerator syntaxGenerator, IArtifactGenerator artifactGenerator, IFileSystem fileSystem, IArtifactFactory artifactFactory, ICommandService commandService)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(syntaxGenerator);
        ArgumentNullException.ThrowIfNull(artifactGenerator);
        ArgumentNullException.ThrowIfNull(fileSystem);
        ArgumentNullException.ThrowIfNull(artifactFactory);
        ArgumentNullException.ThrowIfNull(commandService);

        _logger = logger;
        _syntaxGenerator = syntaxGenerator;
        _artifactGenerator = artifactGenerator;
        _fileSystem = fileSystem;
        _artifactFactory = artifactFactory;
        _commandService = commandService;
    }

    public async Task Handle(DddAppCreateRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating Domain Driven Design Application", nameof(DddAppCreateRequestHandler));

        var modernWebAppDataModel = new ModernWebAppDataModel()
        {
            ProductName = request.ProductName,
            BoundedContextName = request.BoundedContext,
            Aggregates = request.Aggregate,
            Properties = request.Properties,
        };

        // Ensure Key Defined
        if (!request.Properties.Contains($"{request.Aggregate}Id"))
        {
            request.Properties = $"{request.Aggregate}Id:Guid,{request.Properties}";
        }

        var json = await _syntaxGenerator.GenerateAsync(modernWebAppDataModel);

        var path = _fileSystem.Path.Combine(_fileSystem.Path.GetTempPath(), $"{request.ProductName}.jaon");

        _fileSystem.File.WriteAllText(path, json);

        var model = await _artifactFactory.SolutionCreateAsync(path, request.ProductName, request.Directory, cancellationToken);

        await _artifactGenerator.GenerateAsync(model);

        _commandService.Start($"code {model.SolutionDirectory}");
    }
}
