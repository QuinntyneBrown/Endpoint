// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Core.Abstractions;
using Endpoint.Core.Syntax.Constructors;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Text;

namespace Endpoint.Core.Syntax.Methods.Strategies;

public class MethodSyntaxGenerationStrategy : SyntaxGenerationStrategyBase<MethodModel>
{
    private readonly ILogger<MethodSyntaxGenerationStrategy> _logger;
    public MethodSyntaxGenerationStrategy(
        IServiceProvider serviceProvider,
        ILogger<MethodSyntaxGenerationStrategy> logger)
        : base(serviceProvider)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override bool CanHandle(object model, dynamic context = null)
        => model is MethodModel methodModel && !methodModel.Interface;
    public override async Task<string> CreateAsync(ISyntaxGenerator syntaxGenerator, MethodModel model, dynamic context = null)
    {
        _logger.LogInformation("Generating syntax for {0}.", model);

        var builder = new StringBuilder();

        foreach (var attribute in model.Attributes)
        {
            builder.AppendLine(await syntaxGenerator.CreateAsync(attribute));
        }

        builder.Append(await syntaxGenerator.CreateAsync(model.AccessModifier));

        if (model.Override)
            builder.Append(" override");

        if (model.Async)
            builder.Append(" async");

        if (model.Params.SingleOrDefault(x => x.ExtensionMethodParam) != null || model.Static)
            builder.Append(" static");

        builder.Append($" {await syntaxGenerator.CreateAsync(model.ReturnType)}");

        builder.Append($" {model.Name}");

        builder.Append('(');

        builder.Append(string.Join(',', model.Params.Select(async x => await syntaxGenerator.CreateAsync(x))));

        builder.Append(')');

        if (string.IsNullOrEmpty(model.Body))
            builder.Append("{ }");
        else
        {
            builder.AppendLine();

            builder.AppendLine("{");

            builder.AppendLine(model.Body.Indent(1));

            builder.Append('}');
        }

        return builder.ToString();
    }
}
