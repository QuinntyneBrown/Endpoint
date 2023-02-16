// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Endpoint.Core.Models.Syntax.Types;

public class TypeModel
{
    public TypeModel(string name = null)
    {
        Name = name;
        GenericTypeParameters = new List<TypeModel>();
    }

    public string Name { get; set; }
    public List<TypeModel> GenericTypeParameters { get; set; }
    public bool Nullable { get; set; }
    public bool Interface { get; set; }

    public static TypeModel Task = new TypeModel("Task");
    public static TypeModel TaskOf(string typeName)
    {
        return new TypeModel("Task")
        {
            GenericTypeParameters = new List<TypeModel>()
            {
                new TypeModel(typeName)
            }
        };
    }

    public static TypeModel LoggerOf(string typeName)
    {
        return new TypeModel("ILogger")
        {
            GenericTypeParameters = new List<TypeModel>()
            {
                new TypeModel(typeName)
            }
        };
    }

    public static TypeModel ListOf(string typeName)
    {
        return new TypeModel("List")
        {
            GenericTypeParameters = new List<TypeModel>()
            {
                new TypeModel(typeName)
            }
        };
    }

    public static TypeModel CreateTaskOfActionResultOf(string typeName)
    {
        return new TypeModel("Task")
        {
            GenericTypeParameters = new List<TypeModel>
                {
                    new TypeModel("ActionResult")
                    {
                        GenericTypeParameters = new List<TypeModel>()
                        {
                            new TypeModel(typeName)
                        }
                    }
                }
        };
    }

}
