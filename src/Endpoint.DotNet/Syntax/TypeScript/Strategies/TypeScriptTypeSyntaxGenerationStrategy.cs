// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text;
using System.Threading;
using Endpoint.DotNet.Services;
using Microsoft.Extensions.Logging;

namespace Endpoint.DotNet.Syntax.TypeScript.Strategies;

public class TypeScriptTypeSyntaxGenerationStrategy : ISyntaxGenerationStrategy<TypeScriptTypeModel>
{
    private readonly ILogger<TypeScriptTypeSyntaxGenerationStrategy> logger;
    private readonly INamingConventionConverter namingConventionConverter;

    public TypeScriptTypeSyntaxGenerationStrategy(

        ILogger<TypeScriptTypeSyntaxGenerationStrategy> logger,
        INamingConventionConverter namingConventionConverter)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.namingConventionConverter = namingConventionConverter ?? throw new ArgumentNullException(nameof(namingConventionConverter));
    }

    public async Task<string> GenerateAsync(TypeScriptTypeModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = StringBuilderCache.Acquire();

        builder.AppendLine($"export type {namingConventionConverter.Convert(NamingConvention.PascalCase, model.Name)}" + " = {");

        foreach (var property in model.Properties)
        {
            builder.AppendLine($"{namingConventionConverter.Convert(NamingConvention.CamelCase, property.Name)}?: {namingConventionConverter.Convert(NamingConvention.CamelCase, property.Type.Name)};".Indent(1, 2));
        }

        builder.AppendLine("};");

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
