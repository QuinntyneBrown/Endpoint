// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Engineering.UnitTests.DomainDrivenDesign;

public class DataContextTests
{
    [Fact]
    public void DefaultConstructor_CreatesInstance()
    {
        // Arrange & Act
        var dataContext = new DataContext();

        // Assert
        Assert.NotNull(dataContext);
    }

    [Fact]
    public void DefaultConstructor_ImplementsIDataContext()
    {
        // Arrange & Act
        var dataContext = new DataContext();

        // Assert
        Assert.IsAssignableFrom<IDataContext>(dataContext);
    }

    [Fact]
    public void BoundedContexts_IsInitializedAsEmptyList()
    {
        // Arrange & Act
        var dataContext = new DataContext();

        // Assert
        Assert.NotNull(dataContext.BoundedContexts);
        Assert.Empty(dataContext.BoundedContexts);
    }

    [Fact]
    public void BoundedContexts_CanAddItems()
    {
        // Arrange
        var dataContext = new DataContext();
        var boundedContext = new BoundedContext("TestContext");

        // Act
        dataContext.BoundedContexts.Add(boundedContext);

        // Assert
        Assert.Single(dataContext.BoundedContexts);
        Assert.Same(boundedContext, dataContext.BoundedContexts[0]);
    }

    [Fact]
    public void BoundedContexts_CanAddMultipleItems()
    {
        // Arrange
        var dataContext = new DataContext();
        var context1 = new BoundedContext("Context1");
        var context2 = new BoundedContext("Context2");
        var context3 = new BoundedContext("Context3");

        // Act
        dataContext.BoundedContexts.AddRange([context1, context2, context3]);

        // Assert
        Assert.Equal(3, dataContext.BoundedContexts.Count);
    }

    [Fact]
    public void Messages_IsInitializedAsEmptyList()
    {
        // Arrange & Act
        var dataContext = new DataContext();

        // Assert
        Assert.NotNull(dataContext.Messages);
        Assert.Empty(dataContext.Messages);
    }

    [Fact]
    public void Messages_CanAddItems()
    {
        // Arrange
        var dataContext = new DataContext();
        var message = new Message("TestMessage");

        // Act
        dataContext.Messages.Add(message);

        // Assert
        Assert.Single(dataContext.Messages);
        Assert.Same(message, dataContext.Messages[0]);
    }

    [Fact]
    public void Messages_CanAddMultipleItems()
    {
        // Arrange
        var dataContext = new DataContext();
        var message1 = new Message("Message1");
        var message2 = new Message("Message2");

        // Act
        dataContext.Messages.AddRange([message1, message2]);

        // Assert
        Assert.Equal(2, dataContext.Messages.Count);
    }

    [Fact]
    public void ProductName_IsEmptyStringByDefault()
    {
        // Arrange & Act
        var dataContext = new DataContext();

        // Assert
        Assert.Equal(string.Empty, dataContext.ProductName);
    }

    [Fact]
    public void ProductName_CanBeSet()
    {
        // Arrange
        var dataContext = new DataContext();
        var expectedProductName = "MyProduct";

        // Act
        dataContext.ProductName = expectedProductName;

        // Assert
        Assert.Equal(expectedProductName, dataContext.ProductName);
    }

    [Fact]
    public void ProductName_CanBeModified()
    {
        // Arrange
        var dataContext = new DataContext { ProductName = "InitialProduct" };
        var newProductName = "UpdatedProduct";

        // Act
        dataContext.ProductName = newProductName;

        // Assert
        Assert.Equal(newProductName, dataContext.ProductName);
    }

    [Fact]
    public void BoundedContexts_CanBeInitializedWithCollectionExpression()
    {
        // Arrange
        var boundedContext = new BoundedContext("TestContext");

        // Act
        var dataContext = new DataContext
        {
            BoundedContexts = [boundedContext]
        };

        // Assert
        Assert.Single(dataContext.BoundedContexts);
        Assert.Same(boundedContext, dataContext.BoundedContexts[0]);
    }

    [Fact]
    public void Messages_CanBeInitializedWithCollectionExpression()
    {
        // Arrange
        var message = new Message("TestMessage");

        // Act
        var dataContext = new DataContext
        {
            Messages = [message]
        };

        // Assert
        Assert.Single(dataContext.Messages);
        Assert.Same(message, dataContext.Messages[0]);
    }

    [Fact]
    public void DataContext_CanBeInitializedWithAllProperties()
    {
        // Arrange
        var boundedContext = new BoundedContext("TestContext");
        var message = new Message("TestMessage");
        var productName = "TestProduct";

        // Act
        var dataContext = new DataContext
        {
            ProductName = productName,
            BoundedContexts = [boundedContext],
            Messages = [message]
        };

        // Assert
        Assert.Equal(productName, dataContext.ProductName);
        Assert.Single(dataContext.BoundedContexts);
        Assert.Single(dataContext.Messages);
    }
}
