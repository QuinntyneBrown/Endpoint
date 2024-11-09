// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DomainDrivenDesign.Core.Models;
using Endpoint.DotNet.Syntax.Types;

namespace Endpoint.ModernWebAppPattern.Core.Extensions;

public static class PropertyKindExtensions
{ 

    public static TypeModel ToType(this PropertyKind kind)
    {
        return kind switch
        {
            PropertyKind.Int => new("int"),
            PropertyKind.String => new("string"),
            PropertyKind.Guid => new("Guid")
        };
    }
}
