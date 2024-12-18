// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Endpoint.DotNet.Syntax.SpecFlow;

public class SpecFlowFeatureModel : SyntaxModel
{
    public SpecFlowFeatureModel(string name)
    {
        Name = name;
        Scenarios = new List<SpecFlowScenarioModel>();
    }

    public string Name { get; init; }

    public List<SpecFlowScenarioModel> Scenarios { get; init; }
}
