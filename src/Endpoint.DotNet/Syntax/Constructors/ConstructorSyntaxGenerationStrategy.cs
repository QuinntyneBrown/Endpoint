// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Linq;
using System.Text;
using Endpoint.DotNet.Services;
using Endpoint.DotNet.Syntax.Classes;
using Microsoft.Extensions.Logging;

namespace Endpoint.DotNet.Syntax.Constructors;

public class ConstructorSyntaxGenerationStrategy : GenericSyntaxGenerationStrategy<ConstructorModel>
{
    private readonly ILogger<ConstructorSyntaxGenerationStrategy> logger;
    private readonly INamingConventionConverter namingConventionConverter;

    public ConstructorSyntaxGenerationStrategy(

        INamingConventionConverter namingConventionConverter,
        ILogger<ConstructorSyntaxGenerationStrategy> logger)
    {
        this.namingConventionConverter = namingConventionConverter ?? throw new ArgumentNullException(nameof(namingConventionConverter));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override async Task<string> GenerateAsync(ISyntaxGenerator syntaxGenerator, ConstructorModel model)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = new StringBuilder();

        builder.Append(await syntaxGenerator.GenerateAsync(model.AccessModifier));

        builder.Append($" {model.Name}");

        builder.Append('(');

        builder.Append(string.Join(',', await Task.WhenAll(model.Params.Select(async x => await syntaxGenerator.GenerateAsync(x)))));

        builder.Append(')');

        if (model.BaseParams.Count > 0)
        {
            builder.AppendLine($": base({string.Join(',', model.BaseParams)})".Indent(1));
        }

        builder.AppendLine("{");

        foreach (var param in model.Params)
        {
            var field = ((ClassModel)model.Parent).Fields.SingleOrDefault(x => x.Type.Name == param.Type.Name);

            if (field != null)
            {
                var fieldName = namingConventionConverter.Convert(NamingConvention.CamelCase, field.Name);

                var paramName = namingConventionConverter.Convert(NamingConvention.CamelCase, param.Name);

                builder.AppendLine($"ArgumentNullException.ThrowIfNull({paramName});".Indent(1));

                builder.AppendLine($"{fieldName} = {paramName};".Indent(1));
            }
        }

        if (model.Body != null)
        {
            string result = await syntaxGenerator.GenerateAsync(model.Body);

            builder.AppendLine(result.Indent(1));
        }

        builder.Append("}");

        return builder.ToString();
    }
}
