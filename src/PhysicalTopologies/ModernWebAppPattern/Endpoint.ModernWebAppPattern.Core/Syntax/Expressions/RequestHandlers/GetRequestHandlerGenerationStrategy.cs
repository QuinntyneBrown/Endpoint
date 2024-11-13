// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DotNet.Syntax;
using Endpoint.ModernWebAppPattern.Core.Syntax.Expressions.RequestHandlers;
using Humanizer;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Endpoint.ModernWebAppPattern.Core.Syntax.Expressions.Controllers;

public class GetRequestHandlerExpressionGenerationStrategy : GenericSyntaxGenerationStrategy<GetRequestHandlerExpressionModel>
{
    private readonly ILogger<ControllerGetExpressionGenerationStrategy> _logger;

    public GetRequestHandlerExpressionGenerationStrategy(ILogger<ControllerGetExpressionGenerationStrategy> logger)
    {
        _logger = logger;
    }

    public override async Task<string> GenerateAsync(ISyntaxGenerator generator, GetRequestHandlerExpressionModel model)
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