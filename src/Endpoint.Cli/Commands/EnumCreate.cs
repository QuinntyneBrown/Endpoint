// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CommandLine;
using Endpoint.Core.Artifacts;
using Endpoint.Core.Artifacts.Files;
using Endpoint.Core.Syntax.Classes;
using Endpoint.Core.Syntax.Constructors;
using Endpoint.Core.Syntax.Fields;
using Endpoint.Core.Syntax.Params;
using Endpoint.Core.Syntax.Types;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Endpoint.Cli.Commands;

[Verb("enum-create")]
public class EnumCreateRequest : IRequest
{
    [Option('n',"name")]
    public string Name { get; set; }

    [Option('e', "enums")]
    public string Enums { get; set; }

    [Option('d', Required = false)]
    public string Directory { get; set; } = System.Environment.CurrentDirectory;
}

public class EnumCreateRequestHandler : IRequestHandler<EnumCreateRequest>
{
    private readonly ILogger<EnumCreateRequestHandler> _logger;
    private readonly IArtifactGenerator _generator;

    public EnumCreateRequestHandler(ILogger<EnumCreateRequestHandler> logger, IArtifactGenerator generator)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(generator);

        _logger = logger;
        _generator = generator;
    }

    public async Task Handle(EnumCreateRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handled: {0}", nameof(EnumCreateRequestHandler));

        var model = new ClassModel(request.Name);

        model.Usings.Add(new ("System"));

        int numberOfEnums = 0;
        foreach (var @enum in request.Enums.Split(','))
        {
            var parts = @enum.Split(':');

            if (parts.Length != 2)
            {
                model.Fields.Add(new FieldModel()
                {
                    Name = parts[0],
                    Static = true,
                    AccessModifier = Core.Syntax.AccessModifier.Public,
                    DefaultValue = $"new (1 << {numberOfEnums})",
                    Type = new TypeModel(request.Name),
                });
            }

            numberOfEnums++;
        }

        var constructor = new ConstructorModel(model, model.Name)
        {
            Params = new List<ParamModel>()
            {
                new ParamModel()
                {
                    Name = "value",
                    Type = new TypeModel("int"),
                },
            },
        };

        model.Fields.Add(new FieldModel() {
            Name = "_value",
            AccessModifier = Core.Syntax.AccessModifier.Private,
            ReadOnly = true,
            Type = new TypeModel("int"),
        });

        model.Methods.Add(new ()
        {
            ImplicitOperator = true,
            Name = "int",
            Static = true,
            Params = new List<ParamModel>()
            {
                new ParamModel()
                {
                    Name = request.Name.ToCamelCase(),
                    Type = new TypeModel(request.Name),
                },
            },
            Body = new ($"return {request.Name.ToCamelCase()}._value;"),
        });

        model.Methods.Add(new ()
        {
            ExplicitOperator = true,
            Name = request.Name,
            Static = true,
            Params = new List<ParamModel>()
            {
                new ParamModel()
                {
                    Name = "value",
                    Type = new TypeModel("int"),
                },
            },
            Body = new ($"return new {request.Name}(value);"),
        });

        model.Constructors.Add(constructor);

        var classFile = new CodeFileModel<ClassModel>(
            model,
            model.Usings,
            model.Name,
            request.Directory,
            ".cs");

        await _generator.GenerateAsync(classFile);
    }
}