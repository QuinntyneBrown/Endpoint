// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DotNet.Syntax;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Endpoint.ModernWebAppPattern.Core.Syntax.Expressions.Controllers;

public class ControllerUpdateExpressionGenerationStrategy : GenericSyntaxGenerationStrategy<ControllerUpdateExpressionModel>
{
    private readonly ILogger<ControllerUpdateExpressionGenerationStrategy> _logger;

    public ControllerUpdateExpressionGenerationStrategy(ILogger<ControllerUpdateExpressionGenerationStrategy> logger)
    {
        _logger = logger;
    }

    public override async Task<string> GenerateAsync(ISyntaxGenerator generator, ControllerUpdateExpressionModel model)
    {
        _logger.LogInformation("Generating syntax. {type}", model.GetType());

        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine("throw new NotImplementedException();");

        return stringBuilder.ToString();
    }
}