// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.DomainDrivenDesign.Core.Models;

public class Property
{
    public Property()
    {

    }

    public Property(string name, PropertyKind kind)
    {
        Name = name;
        Kind = kind;
    }

    public string Name { get; set; }
    public PropertyKind Kind { get; set; }
    public string PropertyKindReference { get; set; }
    public bool Key { get; set; }
}