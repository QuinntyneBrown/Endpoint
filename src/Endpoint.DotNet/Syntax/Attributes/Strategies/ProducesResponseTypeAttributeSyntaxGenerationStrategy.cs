// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Endpoint.DotNet.Syntax.Attributes.Strategies;

public class ProducesResponseTypeAttributeSyntaxGenerationStrategy : ISyntaxGenerationStrategy<ProducesResponseTypeAttributeModel>
{
    private readonly ILogger<ProducesResponseTypeAttributeSyntaxGenerationStrategy> logger;

    public ProducesResponseTypeAttributeSyntaxGenerationStrategy(

        ILogger<ProducesResponseTypeAttributeSyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> GenerateAsync(ProducesResponseTypeAttributeModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        builder.Append('[');

        builder.Append("ProducesResponseType(");

        if (!string.IsNullOrEmpty(model.TypeName))
        {
            builder.Append($"typeof({model.TypeName}), ");
        }

        builder.Append($"(int)HttpStatusCode.{model.HttpStatusCodeName}");

        builder.Append(")]");

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
