// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Endpoint.Core.Artifacts;
using Endpoint.Core.Artifacts.Files;
using Endpoint.Core.Syntax;
using Endpoint.Core.Syntax.Classes;
using Endpoint.Core.Syntax.Constructors;
using MediatR;
using Microsoft.Extensions.Logging;
using static Endpoint.Core.Constants.FileExtensions;

namespace Endpoint.Cli.Commands;

[Verb("enum-create")]
public class EnumCreateRequest : IRequest
{
    [Option('n',"name")]
    public string Name { get; set; }

    [Option('t', "type")]
    public string Type { get; set; } = "int";

    [Option('e', "enums")]
    public string Enums { get; set; } = string.Empty;

    [Option('d', Required = false)]
    public string Directory { get; set; } = System.Environment.CurrentDirectory;
}

public class EnumCreateRequestHandler : IRequestHandler<EnumCreateRequest>
{
    private readonly ILogger<EnumCreateRequestHandler> _logger;
    private readonly IArtifactGenerator _generator;
    private static readonly string[] supportedTypes = [ "int", "string" ];

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

        if (!supportedTypes.Contains(request.Type))
        {
            throw new NotSupportedException();
        }

        var model = ClassModel.CreateUserDefinedEnum(request.Name, request.Type, request.Enums.Split(',').Select(x =>
        {
            var parts = x.Split(":");

            var keyValuePair = new KeyValuePair<string, string>(parts[0], parts.Length > 1 ? parts[1] : string.Empty);

            return keyValuePair;
        }));

        CodeFileModel<ClassModel> classFile = new (
            model,
            model.Usings,
            model.Name,
            request.Directory,
            CSharpFile);

        await _generator.GenerateAsync(classFile);
    }
}

public class HotlineBling
{
    public static readonly HotlineBling Drake = new(6);

    private readonly int _value;

    public HotlineBling(int value)
    {
        ArgumentNullException.ThrowIfNull(value);
        _value = value;
    }

    public static implicit operator int(HotlineBling hotlineBling)
    {
        return hotlineBling._value;

    }

    public static explicit operator HotlineBling(int value)
    {
        return new HotlineBling(value);

    }

}