// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Endpoint.DotNet.Syntax.Methods.Strategies;

public class MethodsSyntaxGenerationStrategy : ISyntaxGenerationStrategy<List<MethodModel>>
{
    private readonly ILogger<MethodsSyntaxGenerationStrategy> logger;
    private readonly ISyntaxGenerator syntaxGenerator;

    public MethodsSyntaxGenerationStrategy(
        ILogger<MethodsSyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> GenerateAsync(List<MethodModel> model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        foreach (var method in model)
        {
            builder.AppendLine(await syntaxGenerator.GenerateAsync(method));

            if (method != model.Last())
            {
                builder.AppendLine();
            }
        }

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
