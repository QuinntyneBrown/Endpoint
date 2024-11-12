// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DomainDrivenDesign.Core.Models;
using Endpoint.DotNet.Syntax;
using Endpoint.ModernWebAppPattern.Core.Syntax.Expressions.RequestHandlers;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Endpoint.ModernWebAppPattern.Core.Syntax.Expressions.Controllers;

public class UpdateRequestHandlerExpressionGenerationStrategy : GenericSyntaxGenerationStrategy<UpdateRequestHandlerExpressionModel>
{
    private readonly ILogger<ControllerUpdateExpressionGenerationStrategy> _logger;

    public UpdateRequestHandlerExpressionGenerationStrategy(ILogger<ControllerUpdateExpressionGenerationStrategy> logger)
    {
        _logger = logger;
    }

    public override async Task<string> GenerateAsync(ISyntaxGenerator generator, UpdateRequestHandlerExpressionModel model)
    {
        _logger.LogInformation("Generating syntax. {type}", model.GetType());

        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine($$"""
            throw new NotImplementedException();
            """);

        return stringBuilder.ToString();
    }
}