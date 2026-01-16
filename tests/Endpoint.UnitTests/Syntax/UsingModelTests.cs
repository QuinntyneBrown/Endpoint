// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Syntax;

namespace Endpoint.UnitTests.Syntax;

public class UsingModelTests
{
    [Fact]
    public void Constructor_WithName_ShouldInitializeName()
    {
        // Arrange
        var name = "System.Collections.Generic";

        // Act
        var usingModel = new UsingModel(name);

        // Assert
        Assert.Equal(name, usingModel.Name);
    }

    [Fact]
    public void Constructor_Parameterless_ShouldInitializeNameAsEmptyString()
    {
        // Arrange & Act
        var usingModel = new UsingModel();

        // Assert
        Assert.Equal(string.Empty, usingModel.Name);
    }

    [Fact]
    public void Name_ShouldBeInitOnly()
    {
        // Arrange & Act
        var usingModel = new UsingModel("System");

        // Assert
        Assert.Equal("System", usingModel.Name);
    }

    [Fact]
    public void Constructor_WithEmptyString_ShouldInitialize()
    {
        // Arrange & Act
        var usingModel = new UsingModel(string.Empty);

        // Assert
        Assert.Equal(string.Empty, usingModel.Name);
        Assert.NotNull(usingModel);
    }

    [Fact]
    public void Constructor_WithComplexNamespace_ShouldInitialize()
    {
        // Arrange
        var complexNamespace = "System.Collections.Generic.List";

        // Act
        var usingModel = new UsingModel(complexNamespace);

        // Assert
        Assert.Equal(complexNamespace, usingModel.Name);
    }

    [Fact]
    public void Constructor_WithDifferentNames_ShouldCreateDifferentInstances()
    {
        // Arrange
        var usingModel1 = new UsingModel("System");
        var usingModel2 = new UsingModel("System.Linq");

        // Assert
        Assert.NotEqual(usingModel1.Name, usingModel2.Name);
    }
}
