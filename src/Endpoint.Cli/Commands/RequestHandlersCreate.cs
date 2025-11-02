// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CommandLine;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Endpoint.ModernWebAppPattern.Core.Syntax;
using Endpoint.DotNet.Artifacts.Files;
using Endpoint.DotNet.Syntax.Classes;
using Endpoint.Core.Artifacts.Abstractions;


namespace Endpoint.Cli.Commands;


[Verb("request-handlers-create")]
public class RequestHandlersCreateRequest : IRequest {
    [Option('a', "aggregate-name")]
    public string AggregateName { get; set; }


    [Option('p', "product-name")]
    public string ProductName { get; set; }


    [Option('b', "bounded-context-name")]
    public string BoundedContextName { get; set; }


    [Option('d', Required = false)]
    public string Directory { get; set; } = System.Environment.CurrentDirectory;
}

public class RequestHandlersCreateRequestHandler : IRequestHandler<RequestHandlersCreateRequest>
{
    private readonly ILogger<RequestHandlersCreateRequestHandler> _logger;
    private readonly IArtifactGenerator _artifactGenerator;

    public RequestHandlersCreateRequestHandler(ILogger<RequestHandlersCreateRequestHandler> logger, IArtifactGenerator artifactGenerator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(artifactGenerator);

        _logger = logger;
        _artifactGenerator = artifactGenerator;
    }

    public async Task Handle(RequestHandlersCreateRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handled: {0}", nameof(RequestHandlersCreateRequestHandler));

        var requestHandlers = RequestHandlers.Create(request.AggregateName, request.ProductName, request.BoundedContextName);

        foreach (var classModel in requestHandlers)
        {
            await _artifactGenerator.GenerateAsync(new CodeFileModel<ClassModel>(classModel, classModel.Name, request.Directory, ".cs"));
        }
    }
}