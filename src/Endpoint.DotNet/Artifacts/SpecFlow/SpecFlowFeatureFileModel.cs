// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DotNet.Artifacts.Files;
using Endpoint.DotNet.Syntax.SpecFlow;

namespace Endpoint.DotNet.Artifacts.SpecFlow;

public class SpecFlowFeatureFileModel : CodeFileModel<SpecFlowFeatureModel>
{
    public SpecFlowFeatureFileModel(SpecFlowFeatureModel model, string directory)
        : base(model, model.Name, directory, "feature")
    {
    }
}
