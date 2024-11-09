// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Linq;
using System.Text;
using Endpoint.DotNet.Services;
using Endpoint.DotNet.Syntax.Classes;

namespace Endpoint.DotNet.Syntax.Methods.Strategies;

public class PageQueryHandlerMethodGenerationStrategy : GenericSyntaxGenerationStrategy<MethodModel>
{
    private readonly INamingConventionConverter namingConventionConverter;

    public PageQueryHandlerMethodGenerationStrategy(
        INamingConventionConverter namingConventionConverter)
    {
        this.namingConventionConverter = namingConventionConverter ?? throw new ArgumentNullException(nameof(namingConventionConverter));
    }

    public bool CanHandle(object model)
    {
        /*        if (model is MethodModel methodModel && context?.Entity is ClassModel entity)
                {
                    return methodModel.Name == "Handle" && methodModel.Params.FirstOrDefault().Type.Name.EndsWith($"PageRequest");
                }*/

        return false;
    }

    public override async Task<string> GenerateAsync(ISyntaxGenerator generator, object target)
    {
        /*        if (target is MethodModel)
                {
                    return await GenerateAsync(generator, target as MethodModel);
                */

        return null;
    }

    public override async Task<string> GenerateAsync(ISyntaxGenerator syntaxGenerator, MethodModel model)
    {
        var builder = new StringBuilder();

        var entityName = string.Empty; // context.Entity.Name;

        builder.AppendLine($"var query = from {((SyntaxToken)entityName).CamelCase()} in _context.{((SyntaxToken)entityName).PascalCasePlural()}");

        builder.AppendLine($"select {((SyntaxToken)entityName).CamelCase()};".Indent(1));

        builder.AppendLine(string.Empty);

        builder.AppendLine($"var length = await _context.{((SyntaxToken)entityName).PascalCasePlural()}.AsNoTracking().CountAsync();");

        builder.AppendLine(string.Empty);

        builder.AppendLine($"var {((SyntaxToken)entityName).CamelCasePlural()} = await query.Page(request.Index, request.PageSize).AsNoTracking()");

        builder.AppendLine(".Select(x => x.ToDto()).ToListAsync();".Indent(1));

        builder.AppendLine(string.Empty);

        builder.AppendLine("return new ()");

        builder.AppendLine("{");

        builder.AppendLine("Length = length,".Indent(1));

        builder.AppendLine($"Entities = {((SyntaxToken)entityName).CamelCasePlural()}".Indent(1));

        builder.AppendLine("};");

        model.Body = new Syntax.Expressions.ExpressionModel(builder.ToString());

        return await syntaxGenerator.GenerateAsync(model);
    }
}
