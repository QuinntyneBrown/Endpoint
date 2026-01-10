// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DotNet.Syntax;
using Endpoint.ModernWebAppPattern.Syntax.Expressions.RequestHandlers;
using Humanizer;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Endpoint.ModernWebAppPattern.Syntax.Expressions.Controllers;

public class DeleteRequestHandlerExpressionGenerationStrategy : ISyntaxGenerationStrategy<DeleteRequestHandlerExpressionModel>
{
    private readonly ILogger<ControllerDeleteExpressionGenerationStrategy> _logger;

    public DeleteRequestHandlerExpressionGenerationStrategy(ILogger<ControllerDeleteExpressionGenerationStrategy> logger)
    {
        _logger = logger;
    }

    public async Task<string> GenerateAsync(DeleteRequestHandlerExpressionModel model, CancellationToken cancellationToken)
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