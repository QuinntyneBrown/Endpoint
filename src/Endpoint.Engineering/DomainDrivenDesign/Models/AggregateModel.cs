// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.
using Humanizer;

namespace Endpoint.Engineering.DomainDrivenDesign.Models;

public class AggregateModel
{
    public AggregateModel()
    {

    }

    public AggregateModel(string name)
    {
        Name = name;
    }

    public string Name { get; set; }

    public List<Property> Properties { get; set; } = [];

    public List<Command> Commands { get; set; } = [];

    public List<Query> Queries { get; set; } = [];

    public List<EntityModel> Entities { get; set; } = [];

    public BoundedContext? BoundedContext { get; set; }

    public static (AggregateModel, IDataContext) Create(string name, string productName)
    {
        var aggregate = new AggregateModel(name)
        {
            Properties =
            [
                new($"{name}Id", PropertyKind.Guid) { Key = true },
            ],
        };

        var boundedContext = new BoundedContext(name.Pluralize())
        {
            Aggregates =
            [
                aggregate
            ],
            ProductName = productName ?? name,
        };

        DataContext dataContext = new()
        {
            ProductName = productName ?? name,
            BoundedContexts =
            [
                boundedContext,
            ],
        };

        aggregate.BoundedContext = boundedContext;

        aggregate.Queries.AddRange([
            new()
            {
                Name = $"Get{name.Pluralize()}",
                Kind = RequestKind.Get,
                Aggregate = aggregate,
            },
            new()
            {
                Name = $"Get{name}ById",
                Kind = RequestKind.GetById,
                Aggregate = aggregate,
            },
        ]);

        aggregate.Commands.AddRange([
            new()
            {
                Name = $"Create{name}",
                Kind = RequestKind.Create,
                ProductName = productName ?? name,
                Aggregate = aggregate,
            },
            new()
            {
                Name = $"Update{name}",
                Kind = RequestKind.Update,
                ProductName = productName ?? name,
                Aggregate = aggregate,
            },
            new()
            {
                Name = $"Delete{name}",
                Kind = RequestKind.Delete,
                ProductName = productName ?? name,
                Aggregate = aggregate,
            },
        ]);

        return (aggregate, dataContext);
    }


}
