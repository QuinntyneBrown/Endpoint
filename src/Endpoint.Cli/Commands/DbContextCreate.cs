// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Endpoint.Core.Artifacts.Abstractions;
using Endpoint.DomainDrivenDesign.Core.Models;
using Endpoint.DotNet.Artifacts.Files;
using Endpoint.DotNet.Syntax.Classes;
using Endpoint.DotNet.Syntax.Interfaces;
using Humanizer;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Endpoint.Cli.Commands;

using DbContextModel = Endpoint.ModernWebAppPattern.Core.Syntax.DbContextModel;

[Verb("db-context-create")]
public class DbContextCreateRequest : IRequest
{
    [Option('a', "aggregate")]
    public string AggregateName { get; set; }

    [Option('p', "product-name")]
    public string ProductName { get; set; }

    [Option('d', Required = false)]
    public string Directory { get; set; } = System.Environment.CurrentDirectory;
}

public class DbContextCreateRequestHandler : IRequestHandler<DbContextCreateRequest>
{
    private readonly ILogger<DbContextCreateRequestHandler> _logger;
    private readonly IArtifactGenerator _artifactGenerator;

    public DbContextCreateRequestHandler(ILogger<DbContextCreateRequestHandler> logger, IArtifactGenerator artifactGenerator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(artifactGenerator);

        _logger = logger;
        _artifactGenerator = artifactGenerator;
    }

    public async Task Handle(DbContextCreateRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handled: {0}", nameof(DbContextCreateRequestHandler));

        var (aggregate, dataContext) = AggregateModel.Create(request.AggregateName, request.ProductName);

        var boundedContext = new BoundedContext(request.AggregateName.Pluralize())
        {
            Aggregates = [aggregate],
            ProductName = request.ProductName,
        };

        var dbContext = DbContextModel.Create(boundedContext, [aggregate]);

        foreach (var typeDeclarationModel in dbContext)
        {
            object model = typeDeclarationModel switch
            {
                ClassModel classModel => new CodeFileModel<ClassModel>(classModel, classModel.Name, request.Directory, ".cs"),
                InterfaceModel interfaceModel => new CodeFileModel<InterfaceModel>(interfaceModel, interfaceModel.Name, request.Directory, ".cs"),
                _ => throw new NotImplementedException(),
            };

            await _artifactGenerator.GenerateAsync(model);
        }
    }
}