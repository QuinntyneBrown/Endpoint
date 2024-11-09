// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text;
using Microsoft.Extensions.Logging;

namespace Endpoint.DotNet.Syntax;

public class AccessModifierSyntaxGenerationStrategy : GenericSyntaxGenerationStrategy<AccessModifier>
{
    private readonly ILogger<AccessModifierSyntaxGenerationStrategy> logger;

    public AccessModifierSyntaxGenerationStrategy(

        ILogger<AccessModifierSyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override async Task<string> GenerateAsync(ISyntaxGenerator syntaxGenerator, AccessModifier model)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = new StringBuilder();

        builder.Append(model switch
        {
            AccessModifier.Public => "public",
            AccessModifier.Protected => "protected",
            AccessModifier.Private => "private",
            AccessModifier.Internal => "internal",
            _ => "public"
        });

        return builder.ToString();
    }
}
