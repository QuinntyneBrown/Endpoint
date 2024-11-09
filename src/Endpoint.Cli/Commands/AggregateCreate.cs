// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Endpoint.DotNet.Artifacts;
using Endpoint.DotNet.Artifacts.Folders.Factories;
using Endpoint.DotNet.DataModel;
using Endpoint.DotNet.Services;
using Endpoint.DotNet.Syntax.Documents;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Endpoint.Cli.Commands;

[Verb("aggregate-create")]
public class AggregateCreateRequest : IRequest
{
    [Option('n', "name")]
    public string Name { get; set; }

    [Option('p', "properties")]
    public string Properties { get; set; }

    [Option('d', Required = false)]
    public string Directory { get; set; } = System.Environment.CurrentDirectory;
}

public class AggregateCreateRequestHandler : IRequestHandler<AggregateCreateRequest>
{
    private readonly ILogger<AggregateCreateRequestHandler> logger;
    private readonly IFolderFactory folderFactory;
    private readonly IArtifactGenerator artifactGenerator;
    private readonly IContext _context;

    public AggregateCreateRequestHandler(
        ILogger<AggregateCreateRequestHandler> logger,
        IFolderFactory folderFactory,
        IArtifactGenerator artifactGenerator,
        IContext context)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.folderFactory = folderFactory ?? throw new ArgumentNullException(nameof(folderFactory));
        this.artifactGenerator = artifactGenerator ?? throw new ArgumentNullException(nameof(artifactGenerator));
        _context = context;
    }

    public async Task Handle(AggregateCreateRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating Aggregate. {name}", request.Name);

        var dataContextProvider = new DataModelContextProvider<DataModelContext>() { };

        dataContextProvider.Configure(x =>
        {
            x.ServiceModels.Add(new ServiceModel()
            {
                Namespace = "HairPop.Core",
                Aggregates = [
                    new AggregateModel()
                    {
                        Properties = [

                        ],
                    },
                ],
            });
        });

        _context.Set<IDataModelContextProvider<DataModelContext>>(dataContextProvider);

        var model = await folderFactory.CreateAggregateAsync(request.Name, request.Properties, request.Directory);

        await artifactGenerator.GenerateAsync(model);
    }
}
