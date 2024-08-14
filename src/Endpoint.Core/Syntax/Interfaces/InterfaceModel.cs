// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using Endpoint.Core.Syntax.Methods;
using Endpoint.Core.Syntax.Types;

namespace Endpoint.Core.Syntax.Interfaces;

public class InterfaceModel : TypeDeclarationModel
{
    public InterfaceModel()
    {
    }

    public InterfaceModel(string name)
        : base(name)
    {

    }

    public List<TypeModel> Implements { get; set; } = [];

    public List<MethodModel> Methods { get; set; } = [];

    public virtual void AddMethod(MethodModel method)
    {
        method.Interface = true;
        Methods.Add(method);
    }
}
