// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Engineering.DomainDrivenDesign.Models;

public class EntityModel
{
    public EntityModel()
    {

    }

    public string Name { get; set; }

    public List<Property> Properties { get; set; } = [];

    public BoundedContext? BoundedContext { get; set; }
}
