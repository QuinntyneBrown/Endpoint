// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Endpoint.DotNet.Artifacts.PlantUml.Models;

public class PlantUmlEnumModel
{
    public PlantUmlEnumModel()
    {
        Values = [];
    }

    public string Name { get; set; }

    public string Namespace { get; set; }

    public List<string> Values { get; set; }
}
