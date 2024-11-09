// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using Endpoint.DotNet.Syntax.Types;

namespace Endpoint.DotNet.Syntax.Structs;

public class UserDefinedTypeStructModel : SyntaxModel
{
    public UserDefinedTypeStructModel()
    {
    }

    public string Name { get; set; }

    public TypeModel SourceType { get; set; }
}
