// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;

namespace Endpoint.DotNet.Artifacts.Projects;

public class ProjectReferenceModel : ArtifactModel
{
    public string ReferenceDirectory { get; set; }

    public string Name { get; set; }
}
