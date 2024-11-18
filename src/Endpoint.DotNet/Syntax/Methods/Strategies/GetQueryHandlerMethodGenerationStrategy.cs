// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Linq;
using System.Text;
using System.Threading;
using Endpoint.DotNet.Services;
using Endpoint.DotNet.Syntax.Classes;
using Microsoft.Extensions.Logging;

namespace Endpoint.DotNet.Syntax.Methods.Strategies;

public class GetQueryHandlerMethodGenerationStrategy : ISyntaxGenerationStrategy<MethodModel>
{
    private readonly INamingConventionConverter namingConventionConverter;
    private readonly ISyntaxGenerator syntaxGenerator;
    public GetQueryHandlerMethodGenerationStrategy(
        INamingConventionConverter namingConventionConverter)
    {
        this.namingConventionConverter = namingConventionConverter ?? throw new ArgumentNullException(nameof(namingConventionConverter));
    }

    public int Priority => int.MaxValue;

    public async Task<string> GenerateAsync(object target)
    {
        /*        if (target is MethodModel)
                {
                    return await GenerateAsync(generator, target as MethodModel);
                }*/

        return null;
    }

    public bool CanHandle(object model)
    {
        /*        if (model is MethodModel methodModel && context?.Entity is ClassModel entity)
                {
                    var entityNamePascalCasePlural = _namingConventionConverter.Convert(NamingConvention.PascalCase, entity.Name, pluralize: true);

                    return methodModel.Params.First().Type.Name == $"Get{entityNamePascalCasePlural}Request";
                }*/

        return false;
    }

    public async Task<string> GenerateAsync(MethodModel model, CancellationToken cancellationToken)
    {
        var builder = new StringBuilder();

        var entityName = string.Empty; // ""; // context.Entity.Name;

        var entityNamePascalCasePlural = namingConventionConverter.Convert(NamingConvention.PascalCase, entityName, pluralize: true);

        builder.AppendLine("return new () {");

        builder.AppendLine($"{entityNamePascalCasePlural} = await _context.{entityNamePascalCasePlural}.AsNoTracking().ToDtosAsync(cancellationToken)".Indent(1));

        builder.AppendLine("};");

        model.Body = new Syntax.Expressions.ExpressionModel(builder.ToString());

        return await syntaxGenerator.GenerateAsync(model);
    }
}
