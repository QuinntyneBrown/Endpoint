// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Linq;
using System.Text;
using System.Threading;
using Endpoint.DotNet.Services;
using Endpoint.DotNet.Syntax.Methods;
using Endpoint.DotNet.Syntax.Properties;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace Endpoint.DotNet.Syntax.Classes.Strategies;

public class ClassSyntaxGenerationStrategy : ISyntaxGenerationStrategy<ClassModel>
{
    private readonly ILogger<ClassSyntaxGenerationStrategy> logger;
    private readonly ISyntaxGenerator syntaxGenerator;
    private readonly IContext context;

    public ClassSyntaxGenerationStrategy(IContext context, ILogger<ClassSyntaxGenerationStrategy> logger, ISyntaxGenerator syntaxGenerator)
    {
        this.context = context;
        this.syntaxGenerator = syntaxGenerator;
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> GenerateAsync(ClassModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

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
        {
            builder.Append(" static");
        }

        builder.Append($" class {model.Name}");

        if (model.Implements.Count > 0)
        {
            builder.Append(": ");

            builder.Append(string.Join(',', await Task.WhenAll(model.Implements.Select(async x => await syntaxGenerator.GenerateAsync(x)))));
        }

        if (model.Properties.Count + model.Methods.Count + model.Constructors.Count + model.Fields.Count == 0)
        {
            builder.Append(" { }");

            return StringBuilderCache.GetStringAndRelease(builder);
        }

        builder.AppendLine($"");

        builder.AppendLine("{");

        if (model.Fields.Count > 0)
        {
            builder.AppendLine(((string)await syntaxGenerator.GenerateAsync(model.Fields)).Indent(1));
        }

        if (model.Constructors.Count > 0)
        {
            builder.AppendLine(((string)await syntaxGenerator.GenerateAsync(model.Constructors)).Indent(1));
        }

        if (model.Properties.Count > 0)
        {
            context.Set(new PropertyModel() { Parent = model });

            builder.AppendLine(((string)await syntaxGenerator.GenerateAsync(model.Properties)).Indent(1));
        }

        if (model.Methods.Count > 0)
        {
            context.Set(new MethodModel() { Interface = false });

            builder.AppendLine(((string)await syntaxGenerator.GenerateAsync(model.Methods)).Indent(1));
        }

        builder.AppendLine("}");

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}