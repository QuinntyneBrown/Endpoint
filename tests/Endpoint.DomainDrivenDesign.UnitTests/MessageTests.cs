// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.DomainDrivenDesign.UnitTests;

public class MessageTests
{
    [Fact]
    public void Constructor_WithName_SetsNameProperty()
    {
        // Arrange
        var expectedName = "OrderCreated";

        // Act
        var message = new Message(expectedName);

        // Assert
        Assert.Equal(expectedName, message.Name);
    }

    [Fact]
    public void Constructor_CreatesInstance()
    {
        // Arrange & Act
        var message = new Message("TestMessage");

        // Assert
        Assert.NotNull(message);
    }

    [Fact]
    public void Properties_IsInitializedAsEmptyList()
    {
        // Arrange & Act
        var message = new Message("TestMessage");

        // Assert
        Assert.NotNull(message.Properties);
        Assert.Empty(message.Properties);
    }

    [Fact]
    public void Properties_CanAddItems()
    {
        // Arrange
        var message = new Message("TestMessage");
        var property = new Property("TestProperty", PropertyKind.String);

        // Act
        message.Properties.Add(property);

        // Assert
        Assert.Single(message.Properties);
        Assert.Same(property, message.Properties[0]);
    }

    [Fact]
    public void Properties_CanAddMultipleItems()
    {
        // Arrange
        var message = new Message("TestMessage");
        var property1 = new Property("Property1", PropertyKind.String);
        var property2 = new Property("Property2", PropertyKind.Int);
        var property3 = new Property("Property3", PropertyKind.Bool);

        // Act
        message.Properties.AddRange([property1, property2, property3]);

        // Assert
        Assert.Equal(3, message.Properties.Count);
    }

    [Fact]
    public void Name_CanBeModified()
    {
        // Arrange
        var message = new Message("InitialName");
        var newName = "UpdatedName";

        // Act
        message.Name = newName;

        // Assert
        Assert.Equal(newName, message.Name);
    }

    [Fact]
    public void Properties_CanBeInitializedWithCollectionExpression()
    {
        // Arrange
        var property = new Property("TestProperty", PropertyKind.Guid);

        // Act
        var message = new Message("TestMessage")
        {
            Properties = [property]
        };

        // Assert
        Assert.Single(message.Properties);
        Assert.Same(property, message.Properties[0]);
    }

    [Fact]
    public void Properties_CanBeReplaced()
    {
        // Arrange
        var message = new Message("TestMessage");
        var property = new Property("TestProperty", PropertyKind.String);
        var newProperties = new List<Property> { property };

        // Act
        message.Properties = newProperties;

        // Assert
        Assert.Same(newProperties, message.Properties);
    }

    [Fact]
    public void Message_WithMultiplePropertyTypes()
    {
        // Arrange
        var message = new Message("ComplexMessage");

        // Act
        message.Properties.AddRange([
            new Property("Id", PropertyKind.Guid) { Key = true },
            new Property("Name", PropertyKind.String),
            new Property("Count", PropertyKind.Int),
            new Property("IsActive", PropertyKind.Bool),
            new Property("Metadata", PropertyKind.Json),
            new Property("CreatedAt", PropertyKind.DateTime),
            new Property("Tags", PropertyKind.Array),
            new Property("Items", PropertyKind.List) { PropertyKindReference = "Item" }
        ]);

        // Assert
        Assert.Equal(8, message.Properties.Count);
        Assert.Contains(message.Properties, p => p.Key);
        Assert.Contains(message.Properties, p => p.PropertyKindReference == "Item");
    }
}
