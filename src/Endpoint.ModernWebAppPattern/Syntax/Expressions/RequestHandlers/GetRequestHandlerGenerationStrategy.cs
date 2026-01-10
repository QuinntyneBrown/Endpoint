// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DotNet.Syntax;
using Endpoint.ModernWebAppPattern.Syntax.Expressions.RequestHandlers;
using Humanizer;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Endpoint.ModernWebAppPattern.Syntax.Expressions.Controllers;

public class GetRequestHandlerExpressionGenerationStrategy : ISyntaxGenerationStrategy<GetRequestHandlerExpressionModel>
{
    private readonly ILogger<ControllerGetExpressionGenerationStrategy> _logger;

    public GetRequestHandlerExpressionGenerationStrategy(ILogger<ControllerGetExpressionGenerationStrategy> logger)
    {
        _logger = logger;
    }

    public async Task<string> GenerateAsync(GetRequestHandlerExpressionModel model, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generating syntax. {type}", model.GetType());

        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine($$"""
            return new {{model.Query.Name}}Response()
            {
                {{model.Query.Aggregate.Name.Pluralize()}} = _context.{{model.Query.Aggregate.Name.Pluralize()}}
                .Select(x => x.ToDto())
                .ToList()
            };
            """);

        return stringBuilder.ToString();
    }
}