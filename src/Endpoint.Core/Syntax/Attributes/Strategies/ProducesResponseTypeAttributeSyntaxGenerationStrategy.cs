// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Extensions.Logging;
using System.Text;

namespace Endpoint.Core.Syntax.Attributes.Strategies;

public class ProducesResponseTypeAttributeSyntaxGenerationStrategy : GenericSyntaxGenerationStrategy<ProducesResponseTypeAttributeModel>
{
    private readonly ILogger<ProducesResponseTypeAttributeSyntaxGenerationStrategy> _logger;
    public ProducesResponseTypeAttributeSyntaxGenerationStrategy(
        IServiceProvider serviceProvider,
        ILogger<ProducesResponseTypeAttributeSyntaxGenerationStrategy> logger)

    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public int GetPriority { get; set; } = 0;

    public override async Task<string> GenerateAsync(ISyntaxGenerator syntaxGenerator, ProducesResponseTypeAttributeModel model, dynamic context = null)
    {
        _logger.LogInformation("Generating syntax for {0}.", model);

        var builder = new StringBuilder();

        builder.Append('[');

        builder.Append("ProducesResponseType(");

        if (!string.IsNullOrEmpty(model.TypeName))
        {
            builder.Append($"typeof({model.TypeName}), ");
        }

        builder.Append($"(int)HttpStatusCode.{model.HttpStatusCodeName}");

        builder.Append(")]");

        return builder.ToString();
    }
}
