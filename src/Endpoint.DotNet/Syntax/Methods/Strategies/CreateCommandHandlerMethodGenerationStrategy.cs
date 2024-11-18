// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Linq;
using System.Text;
using System.Threading;
using Endpoint.DotNet.Services;
using Endpoint.DotNet.Syntax.Classes;

namespace Endpoint.DotNet.Syntax.Methods.Strategies;

public class CreateCommandHandlerMethodGenerationStrategy : ISyntaxGenerationStrategy<MethodModel>
{
    private readonly INamingConventionConverter namingConventionConverter;
    private readonly ISyntaxGenerator syntaxGenerator;

    public CreateCommandHandlerMethodGenerationStrategy(INamingConventionConverter namingConventionConverter)
    {
        this.namingConventionConverter = namingConventionConverter ?? throw new ArgumentNullException(nameof(namingConventionConverter));
    }

    public async Task<string> GenerateAsync(object target, CancellationToken cancellationToken)
    {
        /*        if (target is MethodModel)
                {
                    return await GenerateAsync(generator, target as MethodModel);
                }*/

        return null;
    }

    public async Task<string> GenerateAsync(MethodModel model, CancellationToken cancellationToken)
    {
        var builder = new StringBuilder();

        var entity = new ClassModel();

        var entityName = entity.Name;

        var entityNameCamelCase = namingConventionConverter.Convert(NamingConvention.CamelCase, entityName);

        builder.AppendLine($"var {entityNameCamelCase} = new {((SyntaxToken)entityName).PascalCase()}();");

        builder.AppendLine(string.Empty);

        builder.AppendLine($"_context.{((SyntaxToken)entityName).PascalCasePlural()}.Add({entityNameCamelCase});");

        builder.AppendLine(string.Empty);

        foreach (var property in entity.Properties.Where(x => x.Name != $"{entityName}Id"))
        {
            builder.AppendLine($"{entityNameCamelCase}.{property.Name} = request.{property.Name};");
        }

        builder.AppendLine(string.Empty);

        builder.AppendLine("await _context.SaveChangesAsync(cancellationToken);");

        builder.AppendLine(string.Empty);

        builder.AppendLine("return new ()");

        builder.AppendLine("{");

        builder.AppendLine($"{((SyntaxToken)entityName).PascalCase()} = {entityNameCamelCase}.ToDto()".Indent(1));

        builder.AppendLine("};");

        model.Body = new Syntax.Expressions.ExpressionModel(builder.ToString());

        return await syntaxGenerator.GenerateAsync(model);
    }
}
