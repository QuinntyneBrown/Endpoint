// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Endpoint.Core.Artifacts.Abstractions;
using Endpoint.DomainDrivenDesign.Core;
using Endpoint.DomainDrivenDesign.Core.Models;
using Endpoint.ModernWebAppPattern.Core;
using Endpoint.ModernWebAppPattern.Core.Artifacts;
using Endpoint.ModernWebAppPattern.Core.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Endpoint.Cli.Commands;

[Verb("mwa-create")]
public class ModernWebAppCreateRequest : IRequest
{
    [Option('n', "name")]
    public string Name { get; set; }

    [Option('p', "path")]
    public string Path { get; set; }

    [Option('d', Required = false)]
    public string Directory { get; set; } = System.Environment.CurrentDirectory;
}

public class ModernWebAppCreateRequestHandler : IRequestHandler<ModernWebAppCreateRequest>
{
    private readonly ILogger<ModernWebAppCreateRequestHandler> _logger;
    private readonly IArtifactFactory _artifactFactory;
    private readonly IArtifactGenerator _artifactGenerator;
    private readonly IUserInputService _userInputService;
    private readonly IFileSystem _fileSystem;
    private readonly ICommandService _commandService;

    public ModernWebAppCreateRequestHandler(ILogger<ModernWebAppCreateRequestHandler> logger, IArtifactGenerator artifactGenerator, IArtifactFactory artifactFactory, IUserInputService userInputService, IFileSystem fileSystem, ICommandService commandService)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(artifactGenerator);
        ArgumentNullException.ThrowIfNull(artifactFactory);
        ArgumentNullException.ThrowIfNull(userInputService);
        ArgumentNullException.ThrowIfNull(fileSystem);
        ArgumentNullException.ThrowIfNull(commandService);

        _logger = logger;
        _artifactGenerator = artifactGenerator;
        _artifactFactory = artifactFactory;
        _userInputService = userInputService;
        _fileSystem = fileSystem;
        _commandService = commandService;
    }

    public async Task Handle(ModernWebAppCreateRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handled: {0}", nameof(ModernWebAppCreateRequestHandler));

        JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
        };

        string defaultTemplate = string.Empty;

        if (string.IsNullOrEmpty(request.Path))
        {
            var (_, dddDataContext) = AggregateModel.Create("ToDo", request.Name);

            var dataContext = new ModernWebAppPattern.Core.DataContext
            {
                ProductName = dddDataContext.ProductName,

                BoundedContexts = dddDataContext.BoundedContexts,

                Messages = dddDataContext.Messages,

                Microservices =
                [
                    new ($"{dddDataContext.ProductName}.{dddDataContext.BoundedContexts.Single().Name}.Api", dddDataContext.BoundedContexts.First().Name, MicroseviceKind.Api)
                    {
                        ProductName = dddDataContext.ProductName,
                    },
                ],
            };

            defaultTemplate = JsonSerializer.Serialize(dataContext, options);
        }
        else
        {
            defaultTemplate = _fileSystem.File.ReadAllText(request.Path);
        }

        var jsonElement = await _userInputService.ReadJsonAsync(defaultTemplate);

        var model = await _artifactFactory.SolutionCreateAsync(jsonElement, request.Name, request.Directory, cancellationToken);

        await _artifactGenerator.GenerateAsync(model);

        await _artifactGenerator.GenerateAsync(new FileModel("endpoint", model.SolutionDirectory, ".json")
        {
            Body = JsonSerializer.Serialize(jsonElement, options),
        });

        _commandService.Start("code .", model.SolutionDirectory);
    }
}