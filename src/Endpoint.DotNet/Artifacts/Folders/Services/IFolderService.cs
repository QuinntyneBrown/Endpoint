// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Endpoint.DotNet.Syntax.Classes;

namespace Endpoint.DotNet.Artifacts.Folders.Services;

public interface IFolderService
{
    Task<FolderModel> AggregateQueries(ClassModel aggregate, string directory);

    Task<FolderModel> AggregateCommands(ClassModel aggregate, string directory);
}