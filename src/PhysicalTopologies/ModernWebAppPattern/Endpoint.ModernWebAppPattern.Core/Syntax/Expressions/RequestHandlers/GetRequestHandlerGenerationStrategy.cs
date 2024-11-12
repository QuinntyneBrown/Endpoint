// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DomainDrivenDesign.Core.Models;
using Endpoint.DotNet.Syntax;
using Endpoint.ModernWebAppPattern.Core.Syntax.Expressions.RequestHandlers;
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
            throw new NotImplementedException();
            """);

        return stringBuilder.ToString();
    }
}