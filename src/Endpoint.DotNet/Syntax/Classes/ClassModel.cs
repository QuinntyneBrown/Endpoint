// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using Endpoint.DotNet.Syntax.Attributes;
using Endpoint.DotNet.Syntax.Constructors;
using Endpoint.DotNet.Syntax.Fields;
using Endpoint.DotNet.Syntax.Interfaces;
using Endpoint.DotNet.Syntax.Methods;

namespace Endpoint.DotNet.Syntax.Classes;

public class ClassModel : InterfaceModel
{
    public ClassModel()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ClassModel"/> class.
    /// </summary>
    /// <param name="name">The name.</param>
    public ClassModel(string name)
        : base(name)
    {
        Fields = new List<FieldModel>();
        Constructors = new List<ConstructorModel>();
        Attributes = new List<AttributeModel>();
        AccessModifier = AccessModifier.Public;
    }

    public AccessModifier AccessModifier { get; set; }

    public List<FieldModel> Fields { get; set; }

    public List<ConstructorModel> Constructors { get; set; }

    public List<AttributeModel> Attributes { get; set; }

    public bool Static { get; set; }

    public override void AddMethod(MethodModel method)
    {
        method.Interface = false;
        Methods.Add(method);
    }

    public ClassModel CreateDto()
        => new ClassModel($"{Name}Dto")
        {
            Properties = Properties,
        };

    public override IEnumerable<SyntaxModel> GetChildren()
    {
        foreach (var method in Methods)
        {
            yield return method;
        }

        foreach (var method in Fields)
        {
            yield return method;
        }

        foreach (var method in Constructors)
        {
            yield return method;
        }

        foreach (var method in Attributes)
        {
            yield return method;
        }

        foreach (var implements in Implements)
        {
            yield return implements;
        }
    }
}
