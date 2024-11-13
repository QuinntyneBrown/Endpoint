// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DotNet.Syntax;
using Endpoint.ModernWebAppPattern.Core.Syntax.Expressions.RequestHandlers;
using Humanizer;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Endpoint.ModernWebAppPattern.Core.Syntax.Expressions.Controllers;

public class DeleteRequestHandlerExpressionGenerationStrategy : GenericSyntaxGenerationStrategy<DeleteRequestHandlerExpressionModel>
{
    private readonly ILogger<ControllerDeleteExpressionGenerationStrategy> _logger;

    public DeleteRequestHandlerExpressionGenerationStrategy(ILogger<ControllerDeleteExpressionGenerationStrategy> logger)
    {
        _logger = logger;
    }

    public override async Task<string> GenerateAsync(ISyntaxGenerator generator, DeleteRequestHandlerExpressionModel model)
    {
        _logger.LogInformation("Generating syntax. {type}", model.GetType());

        var key = model.Command.Aggregate.Properties.Single(x => x.Key);

        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine($$"""
            var {{model.Command.Aggregate.Name.ToCamelCase()}} = await _context.{{model.Command.Aggregate.Name.Pluralize()}}.FindAsync(request.{{key.Name}});

            _context.{{model.Command.Aggregate.Name.Pluralize()}}.Remove({{model.Command.Aggregate.Name.ToCamelCase()}});

            await _context.SaveChangesAsync(cancellationToken);

            return new();
            """);

        return stringBuilder.ToString();
    }
}