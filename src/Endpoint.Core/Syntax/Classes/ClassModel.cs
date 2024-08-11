// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using Endpoint.Core.Syntax.Attributes;
using Endpoint.Core.Syntax.Constructors;
using Endpoint.Core.Syntax.Fields;
using Endpoint.Core.Syntax.Interfaces;
using Endpoint.Core.Syntax.Methods;

namespace Endpoint.Core.Syntax.Classes;

public class ClassModel : InterfaceModel
{
    public static ClassModel CreateUserDefinedEnum(string name, string type, IEnumerable<KeyValuePair<string,string>> keyValuePairs)
    {
        var model = new ClassModel(name);

        model.Usings.Add(new ("System"));

        int numberOfEnums = 0;

        foreach (var keyValuePair in keyValuePairs)
        {
            model.Fields.Add(new ()
            {
                Name = keyValuePair.Key,
                Static = true,
                AccessModifier = AccessModifier.Public,
                Type = new(name),
                DefaultValue = string.IsNullOrEmpty(keyValuePair.Value)
                ? type == "int" ? $"new (1 << {numberOfEnums})" : $"new (nameof({keyValuePair.Key}))"
                : type == "int" ? $"new ({keyValuePair.Value})" : $"new (\"{keyValuePair.Value}\")",
            });

            numberOfEnums++;
        }

        ConstructorModel constructor = new(model, model.Name)
        {
            Params =
            [
                new()
                {
                    Name = "value",
                    Type = new(type),
                },
            ],
        };

        model.Fields.Add(new()
        {
            Name = "_value",
            AccessModifier = AccessModifier.Private,
            ReadOnly = true,
            Type = new(type),
        });

        model.Methods.Add(new()
        {
            ImplicitOperator = true,
            Name = type,
            Static = true,
            Params =
            [
                new()
                {
                    Name = name.ToCamelCase(),
                    Type = new(name),
                },
            ],
            Body = new($"return {name.ToCamelCase()}._value;"),
        });

        model.Methods.Add(new()
        {
            ExplicitOperator = true,
            Name = name,
            Static = true,
            Params =
            [
                new()
                {
                    Name = "value",
                    Type = new(type),
                },
            ],
            Body = new($"return new {name}(value);"),
        });

        model.Constructors.Add(constructor);

        return model;
    }

    public ClassModel()
    {
    }

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
        method.IsInterface = false;
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
