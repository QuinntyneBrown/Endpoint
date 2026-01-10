// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DomainDrivenDesign.Core.Models;
using Endpoint.DotNet.Syntax;
using Endpoint.DotNet.Syntax.Attributes;
using Endpoint.DotNet.Syntax.Classes;
using Endpoint.DotNet.Syntax.Methods;
using Endpoint.DotNet.Syntax.Params;
using Endpoint.ModernWebAppPattern.Core.Syntax.Expressions.Controllers;
using Microsoft.Extensions.Logging;

namespace Endpoint.ModernWebAppPattern.Core.Syntax;

using TypeModel = Endpoint.DotNet.Syntax.Types.TypeModel;

public class SyntaxFactory : ISyntaxFactory
{
    private readonly ILogger<SyntaxFactory> _logger;

    public SyntaxFactory(ILogger<SyntaxFactory> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;
    }

    public async Task<MethodModel> ControllerCreateMethodCreateAsync(ClassModel @class, Command command)
    {
        _logger.LogInformation("ControllerCreateMethodCreateAsync. {class} {command}", @class.Name, command.Name);

        var model = new MethodModel
        {
            Name = "CreateAsync",
            ReturnType = TypeModel.TaskOf("IActionResult"),
            Async = true,
            AccessModifier = AccessModifier.Public,
            Body = new ControllerCreateExpressionModel(@class, command),
            Attributes =
            [
                new (AttributeType.Http, "HttpPost", new() { { "Name", "Create" } }),
                new (AttributeType.Consumes, "Consumes", []) { Template = "MediaTypeNames.Application.Json" },
                new (AttributeType.ProducesResponseType, "ProducesResponseType", []) { Template = "StatusCodes.Status200OK" },
                new (AttributeType.ProducesResponseType, "ProducesResponseType", []) { Template = "StatusCodes.Status400BadRequest" }
            ],
            Params =
            [
                new () { Type = new ($"{command.Name}Request"), Name = "request", Attribute = new() { Name = "FromBody" } }
            ]
        };

        return model;
    }

    public async Task<MethodModel> ControllerDeleteMethodCreateAsync(ClassModel @class, Command command)
    {
        _logger.LogInformation("ControllerDeleteMethodCreateAsync. {class} {aggregate}", @class.Name, command.Name);

        var model = new MethodModel
        {
            Name = "DeleteAsync",
            ReturnType = TypeModel.TaskOf("IActionResult"),
            Async = true,
            AccessModifier = AccessModifier.Public,
            Body = new ControllerDeleteExpressionModel(@class, command),
            Attributes =
            [
                new(AttributeType.Http, "HttpDelete", new() { { "Name", "Delete" } }) { Template = "\"{" + $"{command.Aggregate.Name.ToCamelCase()}Id" + "}\"" },
                new(AttributeType.ProducesResponseType, "ProducesResponseType", []) { Template = "StatusCodes.Status200OK" },
                new(AttributeType.ProducesResponseType, "ProducesResponseType", []) { Template = "StatusCodes.Status400BadRequest" }
            ],
            Params =
            [
                new () { Type = new ($"{command.Name}Request"), Name = "request", Attribute = new() { Name = "FromRoute" } }
            ]
        };

        return model;
    }

    public async Task<MethodModel> ControllerGetByIdMethodCreateAsync(ClassModel @class, Query query)
    {
        _logger.LogInformation("ControllerGetByIdMethodCreateAsync. {class} {query}", @class.Name, query.Name);

        var model = new MethodModel
        {
            Name = "GetByIdAsync",
            ReturnType = TypeModel.TaskOf("IActionResult"),
            Async = true,
            AccessModifier = AccessModifier.Public,
            Body = new ControllerGetByIdExpressionModel(@class, query),
            Attributes =
            [
                new(AttributeType.Http, "HttpGet", new() { { "Name", "GetById" } }) { Template = "\"{" + $"{query.Aggregate.Name.ToCamelCase()}Id" + "}\"" },
                new(AttributeType.ProducesResponseType, "ProducesResponseType", []) { Template = "StatusCodes.Status200OK" },
                new(AttributeType.ProducesResponseType, "ProducesResponseType", []) { Template = "StatusCodes.Status400BadRequest" }
            ],
            Params =
            [
                new () { Type = new ($"{query.Name}Request"), Name = "request", Attribute = new() { Name = "FromRoute" } }
            ]
        };

        return model;
    }

    public async Task<MethodModel> ControllerGetMethodCreateAsync(ClassModel @class, Query query)
    {
        _logger.LogInformation("ControllerGetMethodCreateAsync. {class} {query}", @class.Name, query.Name);

        var model = new MethodModel
        {
            Name = "GetAsync",
            ReturnType = TypeModel.TaskOf("IActionResult"),
            Async = true,
            AccessModifier = AccessModifier.Public,
            Body = new ControllerGetExpressionModel(@class, query),
            Attributes =
            [
                new(AttributeType.Http, "HttpGet", new() { { "Name", "Get" } }),
                new(AttributeType.ProducesResponseType, "ProducesResponseType", []) { Template = "StatusCodes.Status200OK" },
                new(AttributeType.ProducesResponseType, "ProducesResponseType", []) { Template = "StatusCodes.Status400BadRequest" }
            ],
            Params =
            [
                new () { Type = new ($"{query.Name}Request"), Name = "request", Attribute = new() { Name = "FromRoute" } }
            ]
        };

        return model;
    }

    public async Task<MethodModel> ControllerUpdateMethodCreateAsync(ClassModel @class, Command command)
    {
        _logger.LogInformation("ControllerUpdateMethodCreateAsync. {class} {command}", @class.Name, command.Name);

        var model = new MethodModel
        {
            Name = "UpdateAsync",
            ReturnType = TypeModel.TaskOf("IActionResult"),
            Async = true,
            AccessModifier = AccessModifier.Public,
            Body = new ControllerUpdateExpressionModel(@class, command),
            Attributes =
            [
                new(AttributeType.Http, "HttpPut", new() { { "Name", "Update" } }),
                new(AttributeType.Consumes, "Consumes", []) { Template = "MediaTypeNames.Application.Json" },
                new(AttributeType.ProducesResponseType, "ProducesResponseType", []) { Template = "StatusCodes.Status200OK" },
                new(AttributeType.ProducesResponseType, "ProducesResponseType", []) { Template = "StatusCodes.Status400BadRequest" }
            ],
            Params =
            [
                new () { Type = new ($"{command.Name}Request"), Name = "request", Attribute = new() { Name = "FromBody" } }
            ]
        };

        return model;
    }
}

