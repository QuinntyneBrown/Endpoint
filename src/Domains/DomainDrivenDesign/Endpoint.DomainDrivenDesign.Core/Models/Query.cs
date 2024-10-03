// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.DomainDrivenDesign.Core.Models;

public class Query
{
    public Query(string name)
    {
        Name = name;
    }

    public string Name { get; set; }
}