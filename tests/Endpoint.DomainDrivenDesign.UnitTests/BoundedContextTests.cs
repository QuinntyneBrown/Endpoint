// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.DomainDrivenDesign.UnitTests;

public class BoundedContextTests
{
    [Fact]
    public void Constructor_WithName_SetsNameProperty()
    {
        // Arrange
        var expectedName = "Orders";

        // Act
        var boundedContext = new BoundedContext(expectedName);

        // Assert
        Assert.Equal(expectedName, boundedContext.Name);
    }

    [Fact]
    public void Constructor_CreatesInstance()
    {
        // Arrange & Act
        var boundedContext = new BoundedContext("TestContext");

        // Assert
        Assert.NotNull(boundedContext);
    }

    [Fact]
    public void Aggregates_IsInitializedAsEmptyList()
    {
        // Arrange & Act
        var boundedContext = new BoundedContext("TestContext");

        // Assert
        Assert.NotNull(boundedContext.Aggregates);
        Assert.Empty(boundedContext.Aggregates);
    }

    [Fact]
    public void Aggregates_CanAddItems()
    {
        // Arrange
        var boundedContext = new BoundedContext("TestContext");
        var aggregate = new AggregateModel("TestAggregate");

        // Act
        boundedContext.Aggregates.Add(aggregate);

        // Assert
        Assert.Single(boundedContext.Aggregates);
        Assert.Same(aggregate, boundedContext.Aggregates[0]);
    }

    [Fact]
    public void Aggregates_CanAddMultipleItems()
    {
        // Arrange
        var boundedContext = new BoundedContext("TestContext");
        var aggregate1 = new AggregateModel("Aggregate1");
        var aggregate2 = new AggregateModel("Aggregate2");
        var aggregate3 = new AggregateModel("Aggregate3");

        // Act
        boundedContext.Aggregates.AddRange([aggregate1, aggregate2, aggregate3]);

        // Assert
        Assert.Equal(3, boundedContext.Aggregates.Count);
    }

    [Fact]
    public void Handles_IsInitializedAsEmptyList()
    {
        // Arrange & Act
        var boundedContext = new BoundedContext("TestContext");

        // Assert
        Assert.NotNull(boundedContext.Handles);
        Assert.Empty(boundedContext.Handles);
    }

    [Fact]
    public void Handles_CanAddItems()
    {
        // Arrange
        var boundedContext = new BoundedContext("TestContext");
        var message = new Message("TestMessage");

        // Act
        boundedContext.Handles.Add(message);

        // Assert
        Assert.Single(boundedContext.Handles);
        Assert.Same(message, boundedContext.Handles[0]);
    }

    [Fact]
    public void Handles_CanAddMultipleItems()
    {
        // Arrange
        var boundedContext = new BoundedContext("TestContext");
        var message1 = new Message("Message1");
        var message2 = new Message("Message2");

        // Act
        boundedContext.Handles.AddRange([message1, message2]);

        // Assert
        Assert.Equal(2, boundedContext.Handles.Count);
    }

    [Fact]
    public void Name_CanBeModified()
    {
        // Arrange
        var boundedContext = new BoundedContext("InitialName");
        var newName = "UpdatedName";

        // Act
        boundedContext.Name = newName;

        // Assert
        Assert.Equal(newName, boundedContext.Name);
    }

    [Fact]
    public void ProductName_CanBeSet()
    {
        // Arrange
        var boundedContext = new BoundedContext("TestContext");
        var expectedProductName = "MyProduct";

        // Act
        boundedContext.ProductName = expectedProductName;

        // Assert
        Assert.Equal(expectedProductName, boundedContext.ProductName);
    }

    [Fact]
    public void ProductName_IsNullByDefault()
    {
        // Arrange & Act
        var boundedContext = new BoundedContext("TestContext");

        // Assert
        Assert.Null(boundedContext.ProductName);
    }

    [Fact]
    public void Aggregates_CanBeInitializedWithCollectionExpression()
    {
        // Arrange
        var aggregate = new AggregateModel("TestAggregate");

        // Act
        var boundedContext = new BoundedContext("TestContext")
        {
            Aggregates = [aggregate]
        };

        // Assert
        Assert.Single(boundedContext.Aggregates);
        Assert.Same(aggregate, boundedContext.Aggregates[0]);
    }

    [Fact]
    public void Handles_CanBeInitializedWithCollectionExpression()
    {
        // Arrange
        var message = new Message("TestMessage");

        // Act
        var boundedContext = new BoundedContext("TestContext")
        {
            Handles = [message]
        };

        // Assert
        Assert.Single(boundedContext.Handles);
        Assert.Same(message, boundedContext.Handles[0]);
    }
}
