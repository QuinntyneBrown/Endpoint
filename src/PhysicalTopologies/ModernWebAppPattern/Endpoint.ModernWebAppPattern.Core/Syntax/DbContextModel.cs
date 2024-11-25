// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DomainDrivenDesign.Core.Models;
using Endpoint.DotNet.Syntax;
using Endpoint.DotNet.Syntax.Classes;
using Endpoint.DotNet.Syntax.Interfaces;
using Endpoint.DotNet.Syntax.Params;
using Endpoint.DotNet.Syntax.Properties;
using Humanizer;

namespace Endpoint.ModernWebAppPattern.Core.Syntax;

using TypeModel = Endpoint.DotNet.Syntax.Types.TypeModel;

/// <summary>
/// DbContextModel.
/// </summary>
public class DbContextModel: List<TypeDeclarationModel>
{
    /// <summary>
    /// Create.
    /// </summary>
    /// <param name="boundedContext">The bounded context.</param>
    /// <param name="aggregates">The aggregates.</param>
    /// <returns></returns>
    public static DbContextModel Create(BoundedContext boundedContext, List<Aggregate> aggregates)
    {
        var dbContextClassModel = new ClassModel($"{boundedContext.Name}DbContext");

        dbContextClassModel.Implements.Add(new("DbContext"));

        dbContextClassModel.Implements.Add(new($"I{boundedContext.Name}DbContext"));

        dbContextClassModel.Usings.Add(new("Microsoft.EntityFrameworkCore"));

        dbContextClassModel.Constructors.Add(new(dbContextClassModel, dbContextClassModel.Name)
        {
            BaseParams =
            [
                "options",
            ],
            Params =
            [
                new() { Name = "options", Type = new($"DbContextOptions<{boundedContext.Name}DbContext>") }
            ]
        });

        foreach (var aggregate in boundedContext.Aggregates)
        {
            var aggregateNamespace = $"{boundedContext.ProductName}.Models.{aggregate.Name}";

            dbContextClassModel.Usings.Add(new(aggregateNamespace));

            dbContextClassModel.Properties.Add(new(dbContextClassModel, AccessModifier.Public, TypeModel.DbSetOf(aggregate.Name), aggregate.Name.Pluralize(), PropertyAccessorModel.GetPrivateSet));
        }

        var dbContextInterfaceModel = new InterfaceModel($"I{boundedContext.Name}DbContext");

        dbContextInterfaceModel.Usings.Add(new("Microsoft.EntityFrameworkCore"));

        foreach (var aggregate in boundedContext.Aggregates)
        {
            var aggregateNamespace = $"{boundedContext.ProductName}.Models.{aggregate.Name}";

            dbContextInterfaceModel.Usings.Add(new(aggregateNamespace));

            dbContextInterfaceModel.Properties.Add(new(dbContextInterfaceModel, AccessModifier.Public, TypeModel.DbSetOf(aggregate.Name), aggregate.Name.Pluralize(), PropertyAccessorModel.GetPrivateSet));
        }

        dbContextInterfaceModel.Methods.Add(new()
        {
            Name = "SaveChangesAsync",
            Interface = true,
            Params =
            [
                ParamModel.CancellationToken
            ],
            ReturnType = TypeModel.TaskOf("int")
        });

        var dbContextModel = new DbContextModel
        {
            dbContextClassModel,
            dbContextInterfaceModel
        };

        return dbContextModel;
    }
}
