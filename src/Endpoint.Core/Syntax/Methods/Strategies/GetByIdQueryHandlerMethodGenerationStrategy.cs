// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Linq;
using System.Text;
using Endpoint.Core.Services;
using Endpoint.Core.Syntax.Classes;
using Microsoft.Extensions.Logging;

namespace Endpoint.Core.Syntax.Methods.Strategies;

public class GetByIdQueryHandlerMethodGenerationStrategy : GenericSyntaxGenerationStrategy<MethodModel>
{
    private readonly INamingConventionConverter namingConventionConverter;

    public GetByIdQueryHandlerMethodGenerationStrategy(
        INamingConventionConverter namingConventionConverter)
    {
        this.namingConventionConverter = namingConventionConverter ?? throw new ArgumentNullException(nameof(namingConventionConverter));
    }

    public int Priority => int.MaxValue;

    public override async Task<string> GenerateAsync(ISyntaxGenerator generator, object target)
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
                    return methodModel.Name == "Handle" && methodModel.Params.FirstOrDefault().Type.Name.StartsWith($"Get{entity.Name}ByIdRequest");
                }*/

        return false;
    }

    public override async Task<string> GenerateAsync(ISyntaxGenerator syntaxGenerator, MethodModel model)
    {
        var builder = new StringBuilder();

        var entityName = string.Empty; // ""; // context.Entity.Name;

        var entityNamePascalCasePlural = namingConventionConverter.Convert(NamingConvention.PascalCase, entityName, pluralize: true);

        builder.AppendLine("return new () {");

        builder.AppendLine($"{entityName} = (await _context.{entityNamePascalCasePlural}.AsNoTracking().SingleOrDefaultAsync(x => x.{entityName}Id == request.{entityName}Id)).ToDto()".Indent(1));

        builder.AppendLine("};");

        model.Body = new Syntax.Expressions.ExpressionModel(builder.ToString());

        return await syntaxGenerator.GenerateAsync(model);
    }
}
