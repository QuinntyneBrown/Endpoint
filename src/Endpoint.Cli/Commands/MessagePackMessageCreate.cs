// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Endpoint.DotNet.Artifacts;
using Endpoint.DotNet.Artifacts.Files;
using Endpoint.DotNet.Syntax.Classes;
using Endpoint.DotNet.Syntax.Classes.Factories;
using MediatR;
using Microsoft.Extensions.Logging;
using static Endpoint.DotNet.Constants.FileExtensions;

namespace Endpoint.Cli.Commands;


[Verb("message-pack-message-create")]
public class MessagePackMessageCreateRequest : IRequest
{
    [Option('n', "name")]
    public string Name { get; set; }

    [Option('p', "properties")]
    public string Properties { get; set; }

    [Option('i', "implements")]
    public string Implements { get; set; }

    [Option('d', Required = false)]
    public string Directory { get; set; } = System.Environment.CurrentDirectory;
}

public class MessagePackMessageCreateRequestHandler : IRequestHandler<MessagePackMessageCreateRequest>
{
    private readonly ILogger<MessagePackMessageCreateRequestHandler> _logger;
    private readonly IClassFactory _classFactory;
    private readonly IArtifactGenerator _artifactGenerator;

    public MessagePackMessageCreateRequestHandler(
        ILogger<MessagePackMessageCreateRequestHandler> logger,
        IClassFactory classFactory,
        IArtifactGenerator artifactGenerator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(classFactory);
        ArgumentNullException.ThrowIfNull(artifactGenerator);

        _classFactory = classFactory;
        _logger = logger;
        _artifactGenerator = artifactGenerator;
    }

    public async Task Handle(MessagePackMessageCreateRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handled: {0}", nameof(MessagePackMessageCreateRequestHandler));

        var @class = await _classFactory.CreateMessagePackMessageAsync(request.Name, request.Properties.ToKeyValuePairList(), request.Implements.FromCsv());

        var model = new CodeFileModel<ClassModel>(
            @class,
            @class.Usings,
            @class.Name,
            request.Directory,
            CSharpFile);

        await _artifactGenerator.GenerateAsync(model);
    }
}