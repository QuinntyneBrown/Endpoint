// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DotNet.Syntax.Classes;

namespace Endpoint.DotNet.Artifacts.Folders.Factories;

public class AggregateFolderModel : FolderModel
{
    public AggregateFolderModel(ClassModel aggregate, string directory)
        : base($"{aggregate.Name}Aggregate", directory)
    {
        Aggregate = aggregate;
    }

    public ClassModel Aggregate { get; set; }
}
