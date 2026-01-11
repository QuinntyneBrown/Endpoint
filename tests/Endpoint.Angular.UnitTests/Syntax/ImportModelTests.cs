// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Angular.Syntax;

namespace Endpoint.Angular.UnitTests.Syntax;

public class ImportModelTests
{
    [Fact]
    public void ImportModel_ShouldCreateInstance()
    {
        // Arrange & Act
        var import = new ImportModel();

        // Assert
        Assert.NotNull(import);
    }

    [Fact]
    public void ImportModel_ShouldSetName()
    {
        // Arrange
        var import = new ImportModel
        {
            Name = "Component",
        };

        // Act & Assert
        Assert.Equal("Component", import.Name);
    }

    [Fact]
    public void ImportModel_ShouldSetModule()
    {
        // Arrange
        var import = new ImportModel
        {
            Module = "@angular/core",
        };

        // Act & Assert
        Assert.Equal("@angular/core", import.Module);
    }
}
