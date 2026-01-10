// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DomainDrivenDesign.Models;
using Endpoint.DotNet.Syntax;
using Endpoint.ModernWebAppPattern.Syntax.Expressions.RequestHandlers;
using Humanizer;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Endpoint.ModernWebAppPattern.Syntax.Expressions.Controllers;

public class GetByIdRequestHandlerExpressionGenerationStrategy : ISyntaxGenerationStrategy<GetByIdRequestHandlerExpressionModel>
{
    private readonly ILogger<ControllerGetByIdExpressionGenerationStrategy> _logger;

    public GetByIdRequestHandlerExpressionGenerationStrategy(ILogger<ControllerGetByIdExpressionGenerationStrategy> logger)
    {
        _logger = logger;
    }

    public async Task<string> GenerateAsync(GetByIdRequestHandlerExpressionModel model, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generating syntax. {type}", model.GetType());

        var stringBuilder = new StringBuilder();

        var key = model.Query.Aggregate.Properties.Single(x => x.Key);

        stringBuilder.AppendLine($$"""
            return new {{model.Query.Name}}Response()
            {
                {{model.Query.Aggregate.Name}} = _context.{{model.Query.Aggregate.Name.Pluralize()}}
                .SingleOrDefault(x => x.{{key.Name}} == request.{{key.Name}})?
                .ToDto()
            };
            """);

        return stringBuilder.ToString();
    }
}