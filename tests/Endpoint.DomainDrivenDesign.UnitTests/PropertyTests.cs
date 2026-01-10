// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.DomainDrivenDesign.UnitTests;

public class PropertyTests
{
    [Fact]
    public void Constructor_WithNameAndKind_SetsNameProperty()
    {
        // Arrange
        var expectedName = "CustomerName";
        var expectedKind = PropertyKind.String;

        // Act
        var property = new Property(expectedName, expectedKind);

        // Assert
        Assert.Equal(expectedName, property.Name);
    }

    [Fact]
    public void Constructor_WithNameAndKind_SetsKindProperty()
    {
        // Arrange
        var expectedName = "CustomerName";
        var expectedKind = PropertyKind.String;

        // Act
        var property = new Property(expectedName, expectedKind);

        // Assert
        Assert.Equal(expectedKind, property.Kind);
    }

    [Fact]
    public void DefaultConstructor_CreatesInstance()
    {
        // Arrange & Act
        var property = new Property();

        // Assert
        Assert.NotNull(property);
    }

    [Fact]
    public void DefaultConstructor_KeyIsFalseByDefault()
    {
        // Arrange & Act
        var property = new Property();

        // Assert
        Assert.False(property.Key);
    }

    [Fact]
    public void Key_CanBeSetToTrue()
    {
        // Arrange
        var property = new Property("Id", PropertyKind.Guid);

        // Act
        property.Key = true;

        // Assert
        Assert.True(property.Key);
    }

    [Fact]
    public void Key_CanBeSetToFalse()
    {
        // Arrange
        var property = new Property("Id", PropertyKind.Guid) { Key = true };

        // Act
        property.Key = false;

        // Assert
        Assert.False(property.Key);
    }

    [Fact]
    public void PropertyKindReference_CanBeSet()
    {
        // Arrange
        var property = new Property("Items", PropertyKind.List);
        var expectedReference = "OrderItem";

        // Act
        property.PropertyKindReference = expectedReference;

        // Assert
        Assert.Equal(expectedReference, property.PropertyKindReference);
    }

    [Fact]
    public void PropertyKindReference_IsNullByDefault()
    {
        // Arrange & Act
        var property = new Property();

        // Assert
        Assert.Null(property.PropertyKindReference);
    }

    [Fact]
    public void Name_CanBeSet()
    {
        // Arrange
        var property = new Property();
        var expectedName = "TestProperty";

        // Act
        property.Name = expectedName;

        // Assert
        Assert.Equal(expectedName, property.Name);
    }

    [Fact]
    public void Kind_CanBeSet()
    {
        // Arrange
        var property = new Property();

        // Act
        property.Kind = PropertyKind.Int;

        // Assert
        Assert.Equal(PropertyKind.Int, property.Kind);
    }

    [Theory]
    [InlineData(PropertyKind.Guid)]
    [InlineData(PropertyKind.String)]
    [InlineData(PropertyKind.Int)]
    [InlineData(PropertyKind.Bool)]
    [InlineData(PropertyKind.Json)]
    [InlineData(PropertyKind.DateTime)]
    [InlineData(PropertyKind.Array)]
    [InlineData(PropertyKind.List)]
    public void Constructor_WithNameAndKind_AcceptsAllPropertyKinds(PropertyKind kind)
    {
        // Arrange
        var name = "TestProperty";

        // Act
        var property = new Property(name, kind);

        // Assert
        Assert.Equal(kind, property.Kind);
    }

    [Fact]
    public void Property_WithInitializer_SetsAllProperties()
    {
        // Arrange & Act
        var property = new Property("TestId", PropertyKind.Guid)
        {
            Key = true,
            PropertyKindReference = "ReferenceType"
        };

        // Assert
        Assert.Equal("TestId", property.Name);
        Assert.Equal(PropertyKind.Guid, property.Kind);
        Assert.True(property.Key);
        Assert.Equal("ReferenceType", property.PropertyKindReference);
    }
}
