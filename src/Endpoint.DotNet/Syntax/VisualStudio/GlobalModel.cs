﻿// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.DotNet.Syntax.VisualStudio;

public class GlobalModel
{
    public GlobalSectionModel[] GlobalSections { get; set; }

    public SolutionModel Solution { get; set; }
}
