// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.DomainDrivenDesign.Core.Models;

public class Aggregate
{
    public Aggregate(string name)
    {
        Name = name;        
    }

    public string Name { get; set; }

    public List<Property> Properties { get; set; } = [];

    public List<Command> Commands { get; set; } = [];

    public List<Query> Queries { get; set; } = [];

    public List<Entity> Entities { get; set; } = [];

    public BoundedContext BoundedContext { get; set; }


}
