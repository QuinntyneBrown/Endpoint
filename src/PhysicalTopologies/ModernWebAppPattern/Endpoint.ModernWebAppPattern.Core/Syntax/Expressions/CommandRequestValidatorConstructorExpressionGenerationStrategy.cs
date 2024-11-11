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

        var properties = new List<Property>();

        switch(model.Command.Kind)
        {
            case RequestKind.Create:
                properties.AddRange(model.Command.Aggregate.Properties.Where(x => !x.Key));
                break;

            case RequestKind.Update:
                properties.AddRange(model.Command.Aggregate.Properties);
                break;

            case RequestKind.Delete:
                properties.AddRange(model.Command.Aggregate.Properties.Where(x => x.Key));
                break;
        }

        foreach ( var property in properties)
        {
            if(property.Kind == PropertyKind.String)
            {
                stringBuilder.AppendLine($$"""
                RuleFor(x => x.{{property.Name}}).NotNull().NotEmpty();
                """);
            }

            if (property.Kind == PropertyKind.Guid || property.Kind == PropertyKind.Int)
            {
                stringBuilder.AppendLine($$"""
                RuleFor(x => x.{{property.Name}}).NotNull();
                """);
            }
        }

        return stringBuilder.ToString();
    }
}