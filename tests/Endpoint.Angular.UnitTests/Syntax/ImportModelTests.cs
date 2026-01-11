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
    public void ImportModel_ShouldSetTypes()
    {
        // Arrange
        var import = new ImportModel("Component", "@angular/core");

        // Act & Assert
        Assert.Single(import.Types);
        Assert.Equal("Component", import.Types[0].Name);
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
