// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Syntax;

namespace Endpoint.UnitTests.Syntax;

public class SyntaxModelTests
{
    [Fact]
    public void Constructor_ShouldInitializeUsingsAsEmptyList()
    {
        // Arrange & Act
        var syntaxModel = new SyntaxModel();

        // Assert
        Assert.NotNull(syntaxModel.Usings);
        Assert.Empty(syntaxModel.Usings);
    }

    [Fact]
    public void Parent_DefaultsToNull()
    {
        // Arrange & Act
        var syntaxModel = new SyntaxModel();

        // Assert
        Assert.Null(syntaxModel.Parent);
    }

    [Fact]
    public void Parent_ShouldBeSettableAndGettable()
    {
        // Arrange
        var parent = new SyntaxModel();
        var child = new SyntaxModel();

        // Act
        child.Parent = parent;

        // Assert
        Assert.Equal(parent, child.Parent);
    }

    [Fact]
    public void Usings_ShouldBeSettableAndGettable()
    {
        // Arrange
        var syntaxModel = new SyntaxModel();
        var usings = new List<UsingModel>
        {
            new UsingModel("System"),
            new UsingModel("System.Linq")
        };

        // Act
        syntaxModel.Usings = usings;

        // Assert
        Assert.Equal(usings, syntaxModel.Usings);
        Assert.Equal(2, syntaxModel.Usings.Count);
    }

    [Fact]
    public void GetChildren_ShouldReturnEmptySequence()
    {
        // Arrange
        var syntaxModel = new SyntaxModel();

        // Act
        var children = syntaxModel.GetChildren();

        // Assert
        Assert.NotNull(children);
        Assert.Empty(children);
    }

    [Fact]
    public void GetDescendants_WithNoParameters_ShouldReturnSelfInList()
    {
        // Arrange
        var syntaxModel = new SyntaxModel();

        // Act
        var descendants = syntaxModel.GetDescendants();

        // Assert
        Assert.NotNull(descendants);
        Assert.Single(descendants);
        Assert.Contains(syntaxModel, descendants);
    }

    [Fact]
    public void GetDescendants_WithSyntaxParameter_ShouldReturnParameterInList()
    {
        // Arrange
        var syntaxModel = new SyntaxModel();
        var otherSyntax = new SyntaxModel();

        // Act
        var descendants = syntaxModel.GetDescendants(otherSyntax);

        // Assert
        Assert.NotNull(descendants);
        Assert.Contains(otherSyntax, descendants);
    }

    [Fact]
    public void GetDescendants_WithNullSyntax_ShouldUseThisAsSyntax()
    {
        // Arrange
        var syntaxModel = new SyntaxModel();

        // Act
        var descendants = syntaxModel.GetDescendants(null);

        // Assert
        Assert.NotNull(descendants);
        Assert.Single(descendants);
        Assert.Contains(syntaxModel, descendants);
    }

    [Fact]
    public void GetDescendants_WithNullChildren_ShouldCreateNewList()
    {
        // Arrange
        var syntaxModel = new SyntaxModel();

        // Act
        var descendants = syntaxModel.GetDescendants(children: null);

        // Assert
        Assert.NotNull(descendants);
        Assert.IsType<List<SyntaxModel>>(descendants);
    }

    [Fact]
    public void GetDescendants_WithProvidedChildren_ShouldAddToExistingList()
    {
        // Arrange
        var syntaxModel = new SyntaxModel();
        var existingChildren = new List<SyntaxModel> { new SyntaxModel() };
        var originalCount = existingChildren.Count;

        // Act
        var descendants = syntaxModel.GetDescendants(children: existingChildren);

        // Assert
        Assert.NotNull(descendants);
        Assert.True(descendants.Count > originalCount);
        Assert.Contains(syntaxModel, descendants);
    }

    [Fact]
    public void Usings_CanAddUsingModels()
    {
        // Arrange
        var syntaxModel = new SyntaxModel();
        var usingModel = new UsingModel("System");

        // Act
        syntaxModel.Usings.Add(usingModel);

        // Assert
        Assert.Single(syntaxModel.Usings);
        Assert.Contains(usingModel, syntaxModel.Usings);
    }
}
