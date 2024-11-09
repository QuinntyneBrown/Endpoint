// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using System;

namespace Endpoint.DotNet.UnitTests.FolderFactory;

using FolderFactory = Endpoint.DotNet.Artifacts.Folders.Factories.FolderFactory;
public class CreateAggregateAsyncShould {

    [Fact]
    public async Task ReturnExpectedFolderStructure()
    {
        // ARRANGE
        var services = new ServiceCollection();

        // ACT

        // ASSERT
    }
}
