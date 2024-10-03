// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Endpoint.Core.Syntax.Documents.Strategies;

public class DocumentGenerationStrategy : GenericSyntaxGenerationStrategy<DocumentModel>
{
    private readonly ILogger<DocumentGenerationStrategy> logger;

    public DocumentGenerationStrategy(ILogger<DocumentGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override async Task<string> GenerateAsync(ISyntaxGenerator generator, DocumentModel model)
    {
        logger.LogInformation("Generating syntax. {name}", model.Name);

        StringBuilder stringBuilder = new ();

        foreach (var @using in model.GetDescendants().SelectMany(x => x.Usings.Select(x => x.Name)).Distinct())
        {
            stringBuilder.AppendLine($"using {@using};");
        }

        stringBuilder.AppendLine();

        stringBuilder.AppendLine($"namespace {model.RootNamespace}.{model.Namespace};");

        stringBuilder.AppendLine();

        foreach (var item in model.Code)
        {
            string result = await generator.GenerateAsync(item);

            stringBuilder.AppendLine(result);

            stringBuilder.AppendLine();
        }

        return stringBuilder.ToString();
    }
}