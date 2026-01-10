// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DomainDrivenDesign.Core.Models;
using Endpoint.DotNet.Syntax;
using Endpoint.ModernWebAppPattern.Core.Syntax.Expressions.RequestHandlers;
using Humanizer;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Endpoint.ModernWebAppPattern.Core.Syntax.Expressions.Controllers;

public class UpdateRequestHandlerExpressionGenerationStrategy : ISyntaxGenerationStrategy<UpdateRequestHandlerExpressionModel>
{
    private readonly ILogger<ControllerUpdateExpressionGenerationStrategy> _logger;

    public UpdateRequestHandlerExpressionGenerationStrategy(ILogger<ControllerUpdateExpressionGenerationStrategy> logger)
    {
        _logger = logger;
    }

    public async Task<string> GenerateAsync(UpdateRequestHandlerExpressionModel model, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generating syntax. {type}", model.GetType());

        var stringBuilder = new StringBuilder();

        var key = model.Command.Aggregate.Properties.Single(x => x.Key);

        stringBuilder.AppendLine($$"""
            var {{model.Command.Aggregate.Name.ToCamelCase()}} = _context.{{model.Command.Aggregate.Name.Pluralize()}}
            .Single(x => x.{{key.Name}} == request.{{key.Name}});
            """);

        foreach (var property in model.Command.Aggregate.Properties.Where(x => !x.Key))
        {
            stringBuilder.AppendLine($"{model.Command.Aggregate.Name.ToCamelCase()}.{property.Name} = request.{property.Name};");
        }

        stringBuilder.AppendLine($$"""
            await _context.SaveChangesAsync(cancellationToken);

            return new()
            {
                {{model.Command.Aggregate.Name}} = {{model.Command.Aggregate.Name.ToCamelCase()}}.ToDto()
            };
            """);

        return stringBuilder.ToString();
    }
}