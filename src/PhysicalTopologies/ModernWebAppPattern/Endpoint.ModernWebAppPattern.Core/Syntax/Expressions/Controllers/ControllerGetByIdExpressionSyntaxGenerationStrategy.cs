// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DotNet.Syntax;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Endpoint.ModernWebAppPattern.Core.Syntax.Expressions.Controllers;

public class ControllerGetByIdExpressionGenerationStrategy : ISyntaxGenerationStrategy<ControllerGetByIdExpressionModel>
{
    private readonly ILogger<ControllerGetByIdExpressionGenerationStrategy> _logger;

    public ControllerGetByIdExpressionGenerationStrategy(ILogger<ControllerGetByIdExpressionGenerationStrategy> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;
    }

    public async Task<string> GenerateAsync(ControllerGetByIdExpressionModel model, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generating syntax. {type}", model.GetType());

        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine($$"""
            var response = await _mediator.Send(request);

            if(response.{{model.Query.Aggregate.Name}} == null)
            {
                return NotFound(request.{{model.Query.Aggregate.Name}}Id);
            }

            return Ok(response);
            """);

        return stringBuilder.ToString();
    }
}