// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text;
using Microsoft.Extensions.Logging;

namespace Endpoint.DotNet.Syntax.Classes.Strategies;

public class DbContextSyntaxGenerationStrategy : GenericSyntaxGenerationStrategy<DbContextModel>
{
    private readonly ILogger<DbContextSyntaxGenerationStrategy> logger;

    public DbContextSyntaxGenerationStrategy(

        ILogger<DbContextSyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override async Task<string> GenerateAsync(ISyntaxGenerator syntaxGenerator, DbContextModel model)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = new StringBuilder();

        return builder.ToString();
    }
}
