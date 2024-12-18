// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Endpoint.DotNet.Syntax.RouteHandlers;

public class RouteHandlerDeleteSyntaxGenerationStrategy : ISyntaxGenerationStrategy<RouteHandlerModel>
{
    private readonly ILogger<RouteHandlerDeleteSyntaxGenerationStrategy> logger;

    public RouteHandlerDeleteSyntaxGenerationStrategy(
        ILogger<RouteHandlerDeleteSyntaxGenerationStrategy> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public int GetPriority() => 0;

    public bool CanHandle(object model)
        => model is RouteHandlerModel routeHandlerModel && routeHandlerModel.Type == RouteType.Delete;

    public async Task<string> GenerateAsync(RouteHandlerModel model, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating syntax for {0} and type {1}.", model, model.Type);

        var resource = (SyntaxToken)model.Entity.Name;

        var idPropertyName = $"{model.Entity.Name}Id";

        var dbContext = (SyntaxToken)model.DbContextName;

        var idPropertyType = "guid";

        var builder = StringBuilderCache.Acquire();

        builder.AppendLine($"app.MapDelete(\"/{resource.SnakeCasePlural}/" + "{" + "id" + "}" + $"\", async ({idPropertyType} id, {dbContext.PascalCase} context) =>");

        builder.AppendLine("{".Indent(1));

        builder.AppendLine($"if (await context.{resource.PascalCasePlural}.FindAsync(id) is {resource.PascalCase} {resource.CamelCase})".Indent(2));

        builder.AppendLine("{".Indent(2));

        builder.AppendLine($"context.{resource.PascalCasePlural}.Remove({resource.CamelCase});".Indent(3));

        builder.AppendLine("await context.SaveChangesAsync();".Indent(3));

        builder.AppendLine($"return Results.Ok({resource.CamelCase});".Indent(3));

        builder.AppendLine("}".Indent(2));

        builder.AppendLine(string.Empty);

        builder.AppendLine("return Results.NotFound();".Indent(2));

        builder.AppendLine("})".Indent(1));

        builder.AppendLine($".WithName(\"Delete{resource.PascalCase}\")".Indent(1));

        builder.AppendLine(".Produces(StatusCodes.Status204NoContent)".Indent(1));

        builder.AppendLine(".Produces(StatusCodes.Status404NotFound);".Indent(1));

        return StringBuilderCache.GetStringAndRelease(builder);
    }
}
