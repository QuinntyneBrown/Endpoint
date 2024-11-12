// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DotNet.Syntax;
using Endpoint.ModernWebAppPattern.Core.Syntax.Expressions.RequestHandlers;
using Humanizer;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Endpoint.ModernWebAppPattern.Core.Syntax.Expressions.Controllers;

public class CreateRequestHandlerExpressionGenerationStrategy : GenericSyntaxGenerationStrategy<CreateRequestHandlerExpressionModel>
{
    private readonly ILogger<ControllerCreateExpressionGenerationStrategy> _logger;

    public CreateRequestHandlerExpressionGenerationStrategy(ILogger<ControllerCreateExpressionGenerationStrategy> logger)
    {
        _logger = logger;
    }

    public override async Task<string> GenerateAsync(ISyntaxGenerator generator, CreateRequestHandlerExpressionModel model)
    {
        _logger.LogInformation("Generating syntax. {type}", model.GetType());

        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine($$"""
            var {{model.Command.Aggregate.Name.ToCamelCase()}} = new {{model.Command.Aggregate.Name.ToPascalCase()}}()
            {
            {{string.Join(',', model.Command.Aggregate.Properties.Where(x => !x.Key).Select(x => $"{x.Name} = request.{x.Name}".Indent(1)))}}
            };

            _context.{{model.Command.Aggregate.Name.ToPascalCase().Pluralize()}}.Add({{model.Command.Aggregate.Name.ToCamelCase()}});

            await _context.SaveChangesAsync(cancellationToken);

            return new() { 

                {{model.Command.Aggregate.Name.ToPascalCase()}} = {{model.Command.Aggregate.Name.ToCamelCase()}}.ToDto(),
            };
            """);

        return stringBuilder.ToString();
    }
}