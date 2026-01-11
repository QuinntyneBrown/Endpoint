// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DotNet.Syntax;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Endpoint.Engineering.ModernWebAppPattern.Syntax.Expressions.Controllers;

public class ControllerGetExpressionGenerationStrategy : ISyntaxGenerationStrategy<ControllerGetExpressionModel>
{
    private readonly ILogger<ControllerGetExpressionGenerationStrategy> _logger;

    public ControllerGetExpressionGenerationStrategy(ILogger<ControllerGetExpressionGenerationStrategy> logger)
    {
        _logger = logger;
    }

    public async Task<string> GenerateAsync(ControllerGetExpressionModel model, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generating syntax. {type}", model.GetType());

        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine($$"""
            var response = await _mediator.Send(request);

            return Ok(response);
            """);

        return stringBuilder.ToString();
    }
}