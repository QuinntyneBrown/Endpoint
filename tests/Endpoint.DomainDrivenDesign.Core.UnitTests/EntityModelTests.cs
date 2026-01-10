// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.DomainDrivenDesign.Core.UnitTests;

public class EntityModelTests
{
    [Fact]
    public void DefaultConstructor_CreatesInstance()
    {
        // Arrange & Act
        var entity = new EntityModel();

        // Assert
        Assert.NotNull(entity);
    }

    [Fact]
    public void Name_IsNullByDefault()
    {
        // Arrange & Act
        var entity = new EntityModel();

        // Assert
        Assert.Null(entity.Name);
    }

    [Fact]
    public void Name_CanBeSet()
    {
        // Arrange
        var entity = new EntityModel();
        var expectedName = "OrderItem";

        // Act
        entity.Name = expectedName;

        // Assert
        Assert.Equal(expectedName, entity.Name);
    }

    [Fact]
    public void Name_CanBeModified()
    {
        // Arrange
        var entity = new EntityModel { Name = "InitialName" };
        var newName = "UpdatedName";

        // Act
        entity.Name = newName;

        // Assert
        Assert.Equal(newName, entity.Name);
    }

    [Fact]
    public void Properties_IsInitializedAsEmptyList()
    {
        // Arrange & Act
        var entity = new EntityModel();

        // Assert
        Assert.NotNull(entity.Properties);
        Assert.Empty(entity.Properties);
    }

    [Fact]
    public void Properties_CanAddItems()
    {
        // Arrange
        var entity = new EntityModel();
        var property = new Property("TestProperty", PropertyKind.String);

        // Act
        entity.Properties.Add(property);

        // Assert
        Assert.Single(entity.Properties);
        Assert.Same(property, entity.Properties[0]);
    }

    [Fact]
    public void Properties_CanAddMultipleItems()
    {
        // Arrange
        var entity = new EntityModel();
        var property1 = new Property("Id", PropertyKind.Guid);
        var property2 = new Property("Name", PropertyKind.String);
        var property3 = new Property("Quantity", PropertyKind.Int);

        // Act
        entity.Properties.AddRange([property1, property2, property3]);

        // Assert
        Assert.Equal(3, entity.Properties.Count);
    }

    [Fact]
    public void BoundedContext_IsNullByDefault()
    {
        // Arrange & Act
        var entity = new EntityModel();

        // Assert
        Assert.Null(entity.BoundedContext);
    }

    [Fact]
    public void BoundedContext_CanBeSet()
    {
        // Arrange
        var entity = new EntityModel();
        var boundedContext = new BoundedContext("TestContext");

        // Act
        entity.BoundedContext = boundedContext;

        // Assert
        Assert.Same(boundedContext, entity.BoundedContext);
    }

    [Fact]
    public void BoundedContext_CanBeSetToNull()
    {
        // Arrange
        var entity = new EntityModel
        {
            BoundedContext = new BoundedContext("TestContext")
        };

        // Act
        entity.BoundedContext = null;

        // Assert
        Assert.Null(entity.BoundedContext);
    }

    [Fact]
    public void Properties_CanBeInitializedWithCollectionExpression()
    {
        // Arrange
        var property = new Property("TestProperty", PropertyKind.Guid);

        // Act
        var entity = new EntityModel
        {
            Properties = [property]
        };

        // Assert
        Assert.Single(entity.Properties);
        Assert.Same(property, entity.Properties[0]);
    }

    [Fact]
    public void Entity_CanBeInitializedWithAllProperties()
    {
        // Arrange
        var boundedContext = new BoundedContext("TestContext");
        var property = new Property("Id", PropertyKind.Guid) { Key = true };
        var expectedName = "OrderItem";

        // Act
        var entity = new EntityModel
        {
            Name = expectedName,
            BoundedContext = boundedContext,
            Properties = [property]
        };

        // Assert
        Assert.Equal(expectedName, entity.Name);
        Assert.Same(boundedContext, entity.BoundedContext);
        Assert.Single(entity.Properties);
    }

    [Fact]
    public void EntityModel_WithComplexPropertyTypes()
    {
        // Arrange
        var entity = new EntityModel { Name = "ComplexEntity" };

        // Act
        entity.Properties.AddRange([
            new Property("Id", PropertyKind.Guid) { Key = true },
            new Property("Name", PropertyKind.String),
            new Property("Details", PropertyKind.Json),
            new Property("Items", PropertyKind.List) { PropertyKindReference = "SubItem" }
        ]);

        // Assert
        Assert.Equal(4, entity.Properties.Count);
        Assert.Contains(entity.Properties, p => p.Key && p.Kind == PropertyKind.Guid);
        Assert.Contains(entity.Properties, p => p.Kind == PropertyKind.Json);
        Assert.Contains(entity.Properties, p => p.PropertyKindReference == "SubItem");
    }

    [Fact]
    public void Properties_CanBeReplaced()
    {
        // Arrange
        var entity = new EntityModel();
        entity.Properties.Add(new Property("OldProperty", PropertyKind.String));
        var newProperties = new List<Property>
        {
            new Property("NewProperty", PropertyKind.Int)
        };

        // Act
        entity.Properties = newProperties;

        // Assert
        Assert.Same(newProperties, entity.Properties);
        Assert.Single(entity.Properties);
        Assert.Equal("NewProperty", entity.Properties[0].Name);
    }
}
