// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DomainDrivenDesign.Core.Models;
using Endpoint.DotNet.Syntax;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Endpoint.ModernWebAppPattern.Core.Syntax.Expressions.Controllers;

public class CommandRequestValidatorConstructorExpressionGenerationStrategy : GenericSyntaxGenerationStrategy<CommandRequestValidatorConstructorExpressionModel>
{
    private readonly ILogger<ControllerCreateExpressionGenerationStrategy> _logger;

    public CommandRequestValidatorConstructorExpressionGenerationStrategy(ILogger<ControllerCreateExpressionGenerationStrategy> logger)
    {
        _logger = logger;
    }

    public override async Task<string> GenerateAsync(ISyntaxGenerator generator, CommandRequestValidatorConstructorExpressionModel model)
    {
        _logger.LogInformation("Generating syntax. {type}", model.GetType());

        var stringBuilder = new StringBuilder();

        switch(model.Command.Kind)
        {
            case RequestKind.Create:
                break;

            case RequestKind.Update:
                break;

            case RequestKind.Delete:
                break;
        }

        return stringBuilder.ToString();
    }
}