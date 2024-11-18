// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using EnvDTE;
using Microsoft.Extensions.Logging;

namespace Endpoint.DotNet.Syntax.Fields;

public class FieldsSyntaxGenerationStrategy : ISyntaxGenerationStrategy<List<FieldModel>>
{
    private readonly ILogger<FieldsSyntaxGenerationStrategy> logger;
    private readonly ISyntaxGenerator syntaxGenerator;

    public FieldsSyntaxGenerationStrategy(
        ISyntaxGenerator syntaxGenerator,
        ILogger<FieldsSyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.syntaxGenerator = syntaxGenerator;
    }

    public async Task<string> GenerateAsync(List<FieldModel> model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = new StringBuilder();

        foreach (var field in model)
        {
            builder.AppendLine(await CreateAsync(syntaxGenerator, field));

            if (field != model.Last())
            {
                builder.AppendLine();
            }
        }

        return builder.ToString();
    }

    private async Task<string> CreateAsync(ISyntaxGenerator syntaxGenerator, FieldModel model)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = new StringBuilder();

        builder.Append(await syntaxGenerator.GenerateAsync(model.AccessModifier));

        if(model.Static)
        {
            builder.Append(" static");
        }

        if (model.ReadOnly)
        {
            builder.Append(" readonly");
        }

        if (!string.IsNullOrEmpty(model.DefaultValue))
        {
            builder.Append($" {await syntaxGenerator.GenerateAsync(model.Type)} {model.Name} = {model.DefaultValue};");
        }
        else
        {
            builder.Append($" {await syntaxGenerator.GenerateAsync(model.Type)} {model.Name};");
        }

        return builder.ToString();
    }
}
