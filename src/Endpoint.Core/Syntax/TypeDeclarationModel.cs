// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using Endpoint.Core.Syntax.Properties;

namespace Endpoint.Core.Syntax;

public class TypeDeclarationModel : SyntaxModel
{
    public TypeDeclarationModel()
    {
    }

    public TypeDeclarationModel(string name)
    {
        Name = name;
    }

    public string Name { get; set; }

    public List<PropertyModel> Properties { get; set; } = [];

    public List<UsingAsModel> UsingAs { get; set; } = [];
}
