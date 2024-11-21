// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Linq;
using System.Text;
using System.Threading;
using Endpoint.Angular.Syntax;
using Endpoint.Core;
using Endpoint.Core.Syntax;
using Microsoft.Extensions.Logging;

namespace Endpoint.Angular.Syntax;

public class ImportSyntaxGenerationStrategy : ISyntaxGenerationStrategy<ImportModel>
{
    private readonly ILogger<ImportSyntaxGenerationStrategy> logger;

    public ImportSyntaxGenerationStrategy(

        ILogger<ImportSyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> GenerateAsync(ImportModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        builder.Append("import { ");

        builder.AppendJoin(',', model.Types.Select(x => x.Name));

        builder.Append(" } from \"");

        builder.Append(model.Module);

        builder.Append("\";");

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
