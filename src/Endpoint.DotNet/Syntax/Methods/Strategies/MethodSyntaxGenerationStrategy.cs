// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Linq;
using System.Text;
using Endpoint.DotNet.Extensions;
using Microsoft.Extensions.Logging;

namespace Endpoint.DotNet.Syntax.Methods.Strategies;

public class MethodSyntaxGenerationStrategy : GenericSyntaxGenerationStrategy<MethodModel>
{
    private readonly ILogger<MethodSyntaxGenerationStrategy> logger;

    public MethodSyntaxGenerationStrategy(
        ILogger<MethodSyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override async Task<string> GenerateAsync(ISyntaxGenerator syntaxGenerator, MethodModel model)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = new StringBuilder();

        foreach (var attribute in model.Attributes)
        {
            builder.AppendLine(await syntaxGenerator.GenerateAsync(attribute));
        }

        builder.Append(await syntaxGenerator.GenerateAsync(model.AccessModifier));

        if (model.Override)
        {
            builder.Append(" override");
        }

        if (model.Async)
        {
            builder.Append(" async");
        }

        if (model.Params.SingleOrDefault(x => x.ExtensionMethodParam) != null || model.Static)
        {
            builder.Append(" static");
        }

        if (model.ImplicitOperator)
        {
            builder.Append(" implicit operator");
        }
        else if (model.ExplicitOperator)
        {
            builder.Append(" explicit operator");
        }
        else
        {
            builder.Append($" {await syntaxGenerator.GenerateAsync(model.ReturnType)}");
        }

        builder.Append($" {model.Name}");

        builder.Append('(');

        builder.Append(string.Join(',', await Task.WhenAll(model.Params.Select(async x => await syntaxGenerator.GenerateAsync(x)))));

        builder.Append(')');

        if (model.Body == null)
        {
            // builder.Append("{ }");
            builder.AppendLine();
            builder.AppendLine("{");
            builder.AppendLine("}");

            /*            builder.AppendLine();
                        builder.AppendLine("{");
                        builder.AppendLine("}");*/
        }
        else
        {
            builder.AppendLine();

            builder.AppendLine("{");

            string body = await syntaxGenerator.GenerateAsync(model.Body);

            builder.AppendLine(body.Indent(1));

            builder.Append('}');
        }

        return builder.ToString();
    }
}
