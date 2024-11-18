// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Linq;
using System.Text;
using System.Threading;
using Endpoint.DotNet.Services;
using Microsoft.Extensions.Logging;

namespace Endpoint.DotNet.Syntax.Methods.Strategies;

public class InterfaceMethodSyntaxGenerationStrategy : ISyntaxGenerationStrategy<MethodModel>
{
    private readonly INamingConventionConverter namingConventionConverter;
    private readonly ILogger<InterfaceMethodSyntaxGenerationStrategy> logger;
    private readonly IContext context;
    private readonly ISyntaxGenerator syntaxGenerator;

    public InterfaceMethodSyntaxGenerationStrategy(
        INamingConventionConverter namingConventionConverter,
        ILogger<InterfaceMethodSyntaxGenerationStrategy> logger,
        IContext context,
        ISyntaxGenerator syntaxGenerator)
    {
        this.namingConventionConverter = namingConventionConverter ?? throw new ArgumentNullException(nameof(namingConventionConverter));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.context = context ?? throw new ArgumentNullException(nameof(context));
        this.syntaxGenerator = syntaxGenerator ?? throw new ArgumentNullException(nameof(syntaxGenerator));
    }

    public int GetPriority() => 1;

    public async Task<string> GenerateAsync(object target, CancellationToken cancellationToken)
    {
        if (context.Get<MethodModel>().Interface && target is MethodModel && ((MethodModel)target).DefaultMethod == false)
        {
            return await GenerateAsync(target, cancellationToken);
        }

        return null;
    }

    public async Task<string> GenerateAsync(MethodModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0}.", model);

        var builder = new StringBuilder();

        builder.Append($"{await syntaxGenerator.GenerateAsync(model.ReturnType)}");

        builder.Append($" {model.Name}");

        builder.Append('(');

        builder.Append(string.Join(',', await Task.WhenAll(model.Params.Select(async x => await syntaxGenerator.GenerateAsync(x)))));

        builder.Append(");");

        return builder.ToString();
    }
}
