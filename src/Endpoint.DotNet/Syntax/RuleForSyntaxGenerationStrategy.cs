// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Endpoint.DotNet.Syntax;

public class RuleForSyntaxGenerationStrategy : ISyntaxGenerationStrategy<RuleForModel>
{
    private readonly ILogger<RuleForSyntaxGenerationStrategy> logger;

    public RuleForSyntaxGenerationStrategy(

        ILogger<RuleForSyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public int GetPriority() => 0;

    public async Task<string> GenerateAsync(RuleForModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = new StringBuilder();

        return builder.ToString();
    }
}
