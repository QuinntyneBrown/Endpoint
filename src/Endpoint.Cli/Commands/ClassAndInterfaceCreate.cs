// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Endpoint.Core.Artifacts.Abstractions;
using Endpoint.DotNet.Artifacts;
using Endpoint.DotNet.Artifacts.Files;
using Endpoint.DotNet.Syntax.Classes;
using Endpoint.DotNet.Syntax.Classes.Factories;
using Endpoint.DotNet.Syntax.Interfaces;
using Endpoint.DotNet.Syntax.Types;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Endpoint.Cli.Commands;

using TypeModel = Endpoint.DotNet.Syntax.Types.TypeModel;

[Verb("class-and-interface-create")]
public class ClassAndInterfaceCreateRequest : IRequest
{
    [Option('n', "name")]
    public string Name { get; set; }

    [Option('d', Required = false)]
    public string Directory { get; set; } = System.Environment.CurrentDirectory;
}

public class ClassAndInterfaceCreateRequestHandler : IRequestHandler<ClassAndInterfaceCreateRequest>
{
    private readonly ILogger<ClassAndInterfaceCreateRequestHandler> logger;
    private readonly IClassFactory classFactory;
    private readonly IArtifactGenerator artifactGenerator;

    public ClassAndInterfaceCreateRequestHandler(
        ILogger<ClassAndInterfaceCreateRequestHandler> logger,
        IArtifactGenerator artifactGenerator,
        IClassFactory classFactory)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.classFactory = classFactory ?? throw new ArgumentNullException(nameof(classFactory));
        this.artifactGenerator = artifactGenerator ?? throw new ArgumentNullException(nameof(artifactGenerator));
    }

    public async Task Handle(ClassAndInterfaceCreateRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handled: {0}", nameof(ClassAndInterfaceCreateRequestHandler));

        foreach (var nameAndMaybeInterfaces in request.Name.Split(','))
        {
            var parts = nameAndMaybeInterfaces.Split(":");
            var name = parts[0];
            var interfaces = new List<TypeModel>();

            if (parts.Length > 1)
            {
                for (var i = 1; i < parts.Length; i++)
                {
                    interfaces.Add(new Endpoint.DotNet.Syntax.Types.TypeModel(parts[i]));
                }
            }

            var (classModel, interfaceModel) = classFactory.CreateClassAndInterface(name);

            foreach (var @interface in interfaces)
            {
                interfaceModel.Implements.Add(@interface);
            }

            var classFileModel = new CodeFileModel<ClassModel>(classModel, classModel.Usings, classModel.Name, request.Directory, ".cs");

            var interfaceFileModel = new CodeFileModel<InterfaceModel>(interfaceModel, interfaceModel.Usings, interfaceModel.Name, request.Directory, ".cs");

            await artifactGenerator.GenerateAsync(classFileModel);

            await artifactGenerator.GenerateAsync(interfaceFileModel);
        }
    }
}
