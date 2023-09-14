// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Extensions.Logging;
using System.Linq;
using System.Text;

namespace Endpoint.Core.Syntax.Classes.Strategies;

public class ClassSyntaxGenerationStrategy : GenericSyntaxGenerationStrategy<ClassModel>
{
    private readonly ILogger<ClassSyntaxGenerationStrategy> _logger;
    public ClassSyntaxGenerationStrategy(
        IServiceProvider serviceProvider,
        ILogger<ClassSyntaxGenerationStrategy> logger)

    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public int GetPriority { get; set; } = 0;

    public override async Task<string> GenerateAsync(ISyntaxGenerator syntaxGenerator, ClassModel model, dynamic context = null)
    {
        _logger.LogInformation("Generating syntax for {0}.", model);

        var builder = new StringBuilder();

        if (model.UsingAs.Count > 0)
        {
            foreach (var directive in model.UsingAs)
            {
                builder.AppendLine($"using {directive.Alias} = {directive.Name};");
            }

            builder.AppendLine();
        }

        foreach (var attribute in model.Attributes)
        {
            builder.AppendLine(await syntaxGenerator.GenerateAsync(attribute));
        }

        builder.Append(await syntaxGenerator.GenerateAsync(model.AccessModifier));

        if (model.Static)
            builder.Append(" static");

        builder.Append($" class {model.Name}");

        if (model.Implements.Count > 0)
        {
            builder.Append(": ");

            builder.Append(string.Join(',', await Task.WhenAll(model.Implements.Select(async x => await syntaxGenerator.GenerateAsync(x, context)))));
        }

        if (model.Properties.Count + model.Methods.Count + model.Constructors.Count + model.Fields.Count == 0)
        {
            builder.Append(" { }");

            return builder.ToString();
        }

        builder.AppendLine($"");

        builder.AppendLine("{");


        if (model.Fields.Count > 0)
            builder.AppendLine(((string)await syntaxGenerator.GenerateAsync(model.Fields, context)).Indent(1));

        if (model.Constructors.Count > 0)
            builder.AppendLine(((string)await syntaxGenerator.GenerateAsync(model.Constructors, context)).Indent(1));

        if (model.Properties.Count > 0)
            builder.AppendLine(((string)await syntaxGenerator.GenerateAsync(model.Properties, context)).Indent(1));

        if (model.Methods.Count > 0)
            builder.AppendLine(((string)await syntaxGenerator.GenerateAsync(model.Methods, context)).Indent(1));

        builder.AppendLine("}");

        return builder.ToString();
    }
}

