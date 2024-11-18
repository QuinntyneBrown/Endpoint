// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Endpoint.DotNet.Syntax.Params;

public class ParamSyntaxGenerationStrategy : ISyntaxGenerationStrategy<ParamModel>
{
    private readonly ILogger<ParamSyntaxGenerationStrategy> logger;
    private readonly ISyntaxGenerator syntaxGenerator;

    public ParamSyntaxGenerationStrategy(

        ILogger<ParamSyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public int GetPriority() => 0;

    public async Task<string> GenerateAsync(ParamModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = new StringBuilder();

        if (model.ExtensionMethodParam)
        {
            builder.Append("this ");
        }

        if (model.Attribute != null)
        {
            builder.Append(await syntaxGenerator.GenerateAsync(model.Attribute));
        }

        builder.Append($"{await syntaxGenerator.GenerateAsync(model.Type)} {model.Name}");

        return builder.ToString();
    }
}
