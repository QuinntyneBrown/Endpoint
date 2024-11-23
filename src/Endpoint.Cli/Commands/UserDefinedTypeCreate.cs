// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Endpoint.DotNet.Artifacts;
using Endpoint.DotNet.Artifacts.Files;
using Endpoint.DotNet.Syntax;
using Endpoint.DotNet.Syntax.Documents;
using Endpoint.DotNet.Syntax.Structs;
using Endpoint.DotNet.Syntax.Types;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Endpoint.Cli.Commands;

[Verb("udt-create")]
public class UserDefinedTypeCreateRequest : IRequest
{
    [Option('n', "name")]
    public string Name { get; set; }

    [Option('s', "source-type")]
    public string SourceType { get; set; }

    [Option('d', Required = false)]
    public string Directory { get; set; } = System.Environment.CurrentDirectory;
}

public class UserDefinedTypeCreateRequestHandler : IRequestHandler<UserDefinedTypeCreateRequest>
{
    private readonly ILogger<UserDefinedTypeCreateRequestHandler> logger;
    private readonly IArtifactGenerator artifactGenerator;

    public UserDefinedTypeCreateRequestHandler(ILogger<UserDefinedTypeCreateRequestHandler> logger, IArtifactGenerator artifactGenerator)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.artifactGenerator = artifactGenerator ?? throw new ArgumentNullException(nameof(artifactGenerator));
    }

    public async Task Handle(UserDefinedTypeCreateRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handled: {0}", nameof(UserDefinedTypeCreateRequestHandler));

        var codeDoc = new DocumentModel()
        {
            Code = new List<SyntaxModel>()
            {
                new UserDefinedTypeStructModel()
                {
                    Name = request.Name,
                    SourceType = new Endpoint.DotNet.Syntax.Types.TypeModel(request.SourceType),
                },
            },
        };

        await artifactGenerator.GenerateAsync(new CodeFileModel<DocumentModel>(codeDoc, request.Name, request.Directory, ".cs"));
    }
}