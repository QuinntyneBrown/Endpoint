// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.DomainDrivenDesign.Models;

public class Product
{

    public Product(string name)
    {
        Name = name;
    }

    public string Name { get; set; }
    public List<BoundedContext> BoundedContexts { get; set; } = [];

}
