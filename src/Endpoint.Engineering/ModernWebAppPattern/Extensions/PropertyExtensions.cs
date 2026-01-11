// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.DomainDrivenDesign.Models;

namespace Endpoint.Engineering.ModernWebAppPattern.Extensions;

using TypeModel = Endpoint.DotNet.Syntax.Types.TypeModel;

public static class PropertyExtensions
{

    public static TypeModel ToType(this Property property)
    {
        return property.Kind switch
        {
            PropertyKind.Array => new TypeModel($"{property.PropertyKindReference}[]"),
            PropertyKind.List => new TypeModel($"List<{property.PropertyKindReference}>"),

            _ => property switch
            {
                { Kind: PropertyKind.Int } => new("int"),
                { Kind: PropertyKind.String } => new("string"),
                { Kind: PropertyKind.Guid } => new("Guid"),
                { Kind: PropertyKind.Bool } => new("bool"),
                { Kind: PropertyKind.DateTime } => new("DateTime"),
                { Kind: PropertyKind.Json } => new("JsonElement") { Usings = [new UsingModel("System.Text.Json")] },
                _ => throw new NotSupportedException()
            }
        };
    }
}
