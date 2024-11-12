// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DotNet.Syntax;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Endpoint.ModernWebAppPattern.Core.Syntax.Expressions.Controllers;

public class ToDtoExpressionGenerationStrategy : GenericSyntaxGenerationStrategy<ToDtoExpressionModel>
{
    private readonly ILogger<ToDtoExpressionGenerationStrategy> _logger;

    public ToDtoExpressionGenerationStrategy(ILogger<ToDtoExpressionGenerationStrategy> logger)
    {
        _logger = logger;
    }

    public override async Task<string> GenerateAsync(ISyntaxGenerator generator, ToDtoExpressionModel model)
    {
        _logger.LogInformation("Generating syntax. {type}", model.GetType());

        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine($"return new {model.Aggregate.Name}Dto()");
        
        stringBuilder.AppendLine("{");

        foreach(var property in model.Aggregate.Properties)
        {
            stringBuilder.AppendLine($"{property.Name} = {model.Aggregate.Name.ToCamelCase()}.{property.Name},".Indent(1));
        }
        
        stringBuilder.AppendLine("};");

        return stringBuilder.ToString();
    }
}