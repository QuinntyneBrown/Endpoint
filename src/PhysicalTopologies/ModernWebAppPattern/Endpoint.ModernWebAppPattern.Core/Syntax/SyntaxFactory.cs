// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using DotLiquid;
using Endpoint.DomainDrivenDesign.Core.Models;
using Endpoint.DotNet.Syntax;
using Endpoint.DotNet.Syntax.Attributes;
using Endpoint.DotNet.Syntax.Classes;
using Endpoint.DotNet.Syntax.Methods;
using Endpoint.DotNet.Syntax.Params;
using Endpoint.DotNet.Syntax.Types;
using Endpoint.ModernWebAppPattern.Core.Syntax.Expressions.Controllers;
using Humanizer;
using Microsoft.Extensions.Logging;

namespace Endpoint.ModernWebAppPattern.Core.Syntax;

public class SyntaxFactory : ISyntaxFactory
{
    private readonly ILogger<SyntaxFactory> _logger;

    public SyntaxFactory(ILogger<SyntaxFactory> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;
    }

    public async Task<MethodModel> ControllerCreateMethodCreateAsync(ClassModel @class, Aggregate aggregate)
    {
        _logger.LogInformation("ControllerCreateMethodCreateAsync. {class} {aggregate}", @class.Name, aggregate.Name);

        var model = new MethodModel
        {
            Name = "CreateAsync",
            ReturnType = TypeModel.TaskOf("IActionResult"),
            Async = true,
            AccessModifier = AccessModifier.Public,
            Body = new ControllerCreateExpressionModel(@class, aggregate),
            Attributes =
            [
                new (AttributeType.Http, "HttpPost", new() { { "Name", "Create" } }),
                new (AttributeType.Consumes, "Consumes", []) { Template = "MediaTypeNames.Application.Json" },
                new (AttributeType.ProducesResponseType, "ProducesResponseType", []) { Template = "StatusCodes.Status200OK" },
                new (AttributeType.ProducesResponseType, "ProducesResponseType", []) { Template = "StatusCodes.Status400BadRequest" }
            ],
            Params =
            [
                new ParamModel() { Type = new TypeModel($"Create{aggregate.Name}Request"), Name = "request", Attribute = new() { Name = "FromBody" } }
            ]
        };

        return model;
    }

    public async Task<MethodModel> ControllerDeleteMethodCreateAsync(ClassModel @class, Aggregate aggregate)
    {
        _logger.LogInformation("ControllerDeleteMethodCreateAsync. {class} {aggregate}", @class.Name, aggregate.Name);

        var model = new MethodModel
        {
            Name = "DeleteAsync",
            ReturnType = TypeModel.TaskOf("IActionResult"),
            Async = true,
            AccessModifier = AccessModifier.Public,
            Body = new ControllerDeleteExpressionModel(@class, aggregate),
            Attributes =
            [
                new(AttributeType.Http, "HttpDelete", new() { { "Name", "Delete" } }) { Template = "\"{" + $"{aggregate.Name.ToCamelCase()}Id" + "}\"" },
                new(AttributeType.ProducesResponseType, "ProducesResponseType", []) { Template = "StatusCodes.Status200OK" },
                new(AttributeType.ProducesResponseType, "ProducesResponseType", []) { Template = "StatusCodes.Status400BadRequest" }
            ],
            Params =
            [
                new ParamModel() { Type = new TypeModel($"Delete{aggregate.Name}Request"), Name = "request", Attribute = new() { Name = "FromRoute" } }
            ]
        };

        return model;
    }

    public async Task<MethodModel> ControllerGetByIdMethodCreateAsync(ClassModel @class, Aggregate aggregate)
    {
        _logger.LogInformation("ControllerGetByIdMethodCreateAsync. {class} {aggregate}", @class.Name, aggregate.Name);

        var model = new MethodModel
        {
            Name = "GetByIdAsync",
            ReturnType = TypeModel.TaskOf("IActionResult"),
            Async = true,
            AccessModifier = AccessModifier.Public,
            Body = new ControllerGetByIdExpressionModel(@class, aggregate),
            Attributes =
            [
                new(AttributeType.Http, "HttpGet", new() { { "Name", "GetById" } }) { Template = "\"{" + $"{aggregate.Name.ToCamelCase()}Id" + "}\"" },
                new(AttributeType.ProducesResponseType, "ProducesResponseType", []) { Template = "StatusCodes.Status200OK" },
                new(AttributeType.ProducesResponseType, "ProducesResponseType", []) { Template = "StatusCodes.Status400BadRequest" }
            ],
            Params =
            [
                new ParamModel() { Type = new TypeModel($"Get{aggregate.Name}ByIdRequest"), Name = "request", Attribute = new() { Name = "FromRoute" } }
            ]
        };

        return model;
    }

    public async Task<MethodModel> ControllerGetMethodCreateAsync(ClassModel @class, Aggregate aggregate)
    {
        _logger.LogInformation("ControllerGetMethodCreateAsync. {class} {aggregate}", @class.Name, aggregate.Name);

        var model = new MethodModel
        {
            Name = "GetAsync",
            ReturnType = TypeModel.TaskOf("IActionResult"),
            Async = true,
            AccessModifier = AccessModifier.Public,
            Body = new ControllerGetExpressionModel(@class, aggregate),
            Attributes =
            [
                new(AttributeType.Http, "HttpGet", new() { { "Name", "Get" } }),
                new(AttributeType.ProducesResponseType, "ProducesResponseType", []) { Template = "StatusCodes.Status200OK" },
                new(AttributeType.ProducesResponseType, "ProducesResponseType", []) { Template = "StatusCodes.Status400BadRequest" }
            ],
            Params =
            [
                new ParamModel() { Type = new TypeModel($"Get{aggregate.Name.Pluralize()}Request"), Name = "request", Attribute = new() { Name = "FromRoute" } }
            ]
        };

        return model;
    }

    public async Task<MethodModel> ControllerUpdateMethodCreateAsync(ClassModel @class, Aggregate aggregate)
    {
        _logger.LogInformation("ControllerUpdateMethodCreateAsync. {class} {aggregate}", @class.Name, aggregate.Name);

        var model = new MethodModel
        {
            Name = "UpdateAsync",
            ReturnType = TypeModel.TaskOf("IActionResult"),
            Async = true,
            AccessModifier = AccessModifier.Public,
            Body = new ControllerUpdateExpressionModel(@class, aggregate),
            Attributes =
            [
                new(AttributeType.Http, "HttpPut", new() { { "Name", "Update" } }),
                new(AttributeType.Consumes, "Consumes", []) { Template = "MediaTypeNames.Application.Json" },
                new(AttributeType.ProducesResponseType, "ProducesResponseType", []) { Template = "StatusCodes.Status200OK" },
                new(AttributeType.ProducesResponseType, "ProducesResponseType", []) { Template = "StatusCodes.Status400BadRequest" }
            ],
            Params =
            [
                new ParamModel() { Type = new TypeModel($"Update{aggregate.Name}Request"), Name = "request", Attribute = new() { Name = "FromBody" } }
            ]
        };

        return model;
    }
}

