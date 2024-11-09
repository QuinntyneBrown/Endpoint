// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using Endpoint.DotNet.Syntax.Properties;

namespace Endpoint.DotNet.Syntax.TypeScript;

public class TypeScriptTypeModel : SyntaxModel
{
    public TypeScriptTypeModel(string name)
    {
        Name = name;
        Properties = new List<PropertyModel>();
    }

    public string Name { get; set; }

    public List<PropertyModel> Properties { get; set; }
}
