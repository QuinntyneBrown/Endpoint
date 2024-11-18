// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Endpoint.DotNet.Syntax.Namespaces.Strategies;

public class NamespaceGenerationStrategy : ISyntaxGenerationStrategy<NamespaceModel>
{
    private readonly ILogger<NamespaceGenerationStrategy> logger;

    public NamespaceGenerationStrategy(ILogger<NamespaceGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> GenerateAsync(NamespaceModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax. {name}", model.Name);

        StringBuilder sb = new StringBuilder();

        sb.AppendLine($"namespace {model.Name};");

        return sb.ToString();
    }
}