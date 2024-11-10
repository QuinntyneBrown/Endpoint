// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.DotNet.Syntax;

public class UsingModel
{
    public UsingModel(string name)
    {
        Name = name;
    }

    public UsingModel()
    {
    }

    public static UsingModel MediatR => new ("MediatR");

    public string Name { get; init; }
}
