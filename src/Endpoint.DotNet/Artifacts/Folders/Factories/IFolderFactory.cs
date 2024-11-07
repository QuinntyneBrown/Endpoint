// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DotNet.Syntax.Classes;

namespace Endpoint.DotNet.Artifacts.Folders.Factories;

public interface IFolderFactory
{
    Task<FolderModel> CreateAggregateCommandsAsync(ClassModel aggregate, string directory);

    Task<FolderModel> CreateAggregateQueriesAsync(ClassModel aggregate, string directory);

    Task<FolderModel> CreateAngularDomainModelAsync(string modelName, string properties);

    Task<FolderModel> CreateAggregateAsync(string aggregateName, string properties);

    Task<FolderModel> CreateAggregateAsync(string aggregateName, string properties, string directory);
}