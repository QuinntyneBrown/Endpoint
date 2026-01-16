// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Artifacts;

namespace Endpoint.UnitTests.Artifacts;

public class ArtifactModelTests
{
    [Fact]
    public void Constructor_ShouldCreateInstance()
    {
        // Arrange & Act
        var artifactModel = new ArtifactModel();

        // Assert
        Assert.NotNull(artifactModel);
    }

    [Fact]
    public void Parent_ShouldBeSettableAndGettable()
    {
        // Arrange
        var parent = new ArtifactModel();
        var child = new ArtifactModel();

        // Act
        child.Parent = parent;

        // Assert
        Assert.Equal(parent, child.Parent);
    }

    [Fact]
    public void Parent_DefaultsToNull()
    {
        // Arrange & Act
        var artifactModel = new ArtifactModel();

        // Assert
        Assert.Null(artifactModel.Parent);
    }

    [Fact]
    public void GetChildren_ShouldReturnEmptySequence()
    {
        // Arrange
        var artifactModel = new ArtifactModel();

        // Act
        var children = artifactModel.GetChildren();

        // Assert
        Assert.NotNull(children);
        Assert.Empty(children);
    }

    [Fact]
    public void GetDescendants_WithNoRoot_ShouldReturnSelfInList()
    {
        // Arrange
        var artifactModel = new ArtifactModel();

        // Act
        var descendants = artifactModel.GetDescendants();

        // Assert
        Assert.NotNull(descendants);
        Assert.Single(descendants);
        Assert.Contains(artifactModel, descendants);
    }

    [Fact]
    public void GetDescendants_WithRoot_ShouldReturnRootInList()
    {
        // Arrange
        var root = new ArtifactModel();
        var artifactModel = new ArtifactModel();

        // Act
        var descendants = artifactModel.GetDescendants(root);

        // Assert
        Assert.NotNull(descendants);
        Assert.Contains(root, descendants);
    }

    [Fact]
    public void GetDescendants_WithProvidedChildren_ShouldAddToExistingList()
    {
        // Arrange
        var artifactModel = new ArtifactModel();
        var existingChildren = new List<ArtifactModel> { new ArtifactModel() };

        // Act
        var descendants = artifactModel.GetDescendants(children: existingChildren);

        // Assert
        Assert.NotNull(descendants);
        Assert.Contains(artifactModel, descendants);
    }

    [Fact]
    public void GetDescendants_WithNullRoot_ShouldUseThisAsRoot()
    {
        // Arrange
        var artifactModel = new ArtifactModel();

        // Act
        var descendants = artifactModel.GetDescendants(null);

        // Assert
        Assert.NotNull(descendants);
        Assert.Single(descendants);
        Assert.Contains(artifactModel, descendants);
    }

    [Fact]
    public void GetDescendants_WithNullChildren_ShouldCreateNewList()
    {
        // Arrange
        var artifactModel = new ArtifactModel();

        // Act
        var descendants = artifactModel.GetDescendants(children: null);

        // Assert
        Assert.NotNull(descendants);
        Assert.IsType<List<ArtifactModel>>(descendants);
    }
}
