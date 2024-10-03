// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;

namespace Endpoint.DotNet.SystemModels;

public class Class
{
    public Class(string name)
    {
        Name = name;
    }

    public string Name { get; set; }

    public string NamespaceName { get; set; }

    public Namespace Namespace { get; set; }

    public string SolutionName { get; set; }

    public string ProjectName { get; set; }

    public Solution Solution { get; set; }

    public string Project { get; set; }
}