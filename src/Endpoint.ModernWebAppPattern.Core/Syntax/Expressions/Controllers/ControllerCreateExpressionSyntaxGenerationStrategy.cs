// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DotNet.Syntax;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Endpoint.ModernWebAppPattern.Core.Syntax.Expressions.Controllers;

public class ControllerCreateExpressionGenerationStrategy : ISyntaxGenerationStrategy<ControllerCreateExpressionModel>
{
    private readonly ILogger<ControllerCreateExpressionGenerationStrategy> _logger;

    public ControllerCreateExpressionGenerationStrategy(ILogger<ControllerCreateExpressionGenerationStrategy> logger)
    {
        _logger = logger;
    }

    public async Task<string> GenerateAsync(ControllerCreateExpressionModel model, CancellationToken cancellationToken)
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