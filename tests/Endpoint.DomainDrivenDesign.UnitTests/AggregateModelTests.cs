// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.DomainDrivenDesign.UnitTests;

public class AggregateModelTests
{
    [Fact]
    public void Constructor_WithName_SetsNameProperty()
    {
        // Arrange
        var expectedName = "Customer";

        // Act
        var aggregate = new AggregateModel(expectedName);

        // Assert
        Assert.Equal(expectedName, aggregate.Name);
    }

    [Fact]
    public void DefaultConstructor_CreatesInstance()
    {
        // Arrange & Act
        var aggregate = new AggregateModel();

        // Assert
        Assert.NotNull(aggregate);
    }

    [Fact]
    public void DefaultConstructor_PropertiesListIsInitialized()
    {
        // Arrange & Act
        var aggregate = new AggregateModel();

        // Assert
        Assert.NotNull(aggregate.Properties);
        Assert.Empty(aggregate.Properties);
    }

    [Fact]
    public void DefaultConstructor_CommandsListIsInitialized()
    {
        // Arrange & Act
        var aggregate = new AggregateModel();

        // Assert
        Assert.NotNull(aggregate.Commands);
        Assert.Empty(aggregate.Commands);
    }

    [Fact]
    public void DefaultConstructor_QueriesListIsInitialized()
    {
        // Arrange & Act
        var aggregate = new AggregateModel();

        // Assert
        Assert.NotNull(aggregate.Queries);
        Assert.Empty(aggregate.Queries);
    }

    [Fact]
    public void DefaultConstructor_EntitiesListIsInitialized()
    {
        // Arrange & Act
        var aggregate = new AggregateModel();

        // Assert
        Assert.NotNull(aggregate.Entities);
        Assert.Empty(aggregate.Entities);
    }

    [Fact]
    public void Create_ReturnsCorrectAggregateModel()
    {
        // Arrange
        var name = "Order";
        var productName = "MyProduct";

        // Act
        var (aggregate, dataContext) = AggregateModel.Create(name, productName);

        // Assert
        Assert.NotNull(aggregate);
        Assert.Equal(name, aggregate.Name);
    }

    [Fact]
    public void Create_ReturnsCorrectIDataContext()
    {
        // Arrange
        var name = "Order";
        var productName = "MyProduct";

        // Act
        var (aggregate, dataContext) = AggregateModel.Create(name, productName);

        // Assert
        Assert.NotNull(dataContext);
        Assert.IsAssignableFrom<IDataContext>(dataContext);
    }

    [Fact]
    public void Create_SetsProductNameOnDataContext()
    {
        // Arrange
        var name = "Order";
        var productName = "MyProduct";

        // Act
        var (aggregate, dataContext) = AggregateModel.Create(name, productName);

        // Assert
        Assert.Equal(productName, dataContext.ProductName);
    }

    [Fact]
    public void Create_CreatesBoundedContextWithPlurializedName()
    {
        // Arrange
        var name = "Order";
        var productName = "MyProduct";

        // Act
        var (aggregate, dataContext) = AggregateModel.Create(name, productName);

        // Assert
        Assert.Single(dataContext.BoundedContexts);
        Assert.Equal("Orders", dataContext.BoundedContexts[0].Name);
    }

    [Fact]
    public void Create_CreatesKeyPropertyWithCorrectName()
    {
        // Arrange
        var name = "Order";
        var productName = "MyProduct";

        // Act
        var (aggregate, dataContext) = AggregateModel.Create(name, productName);

        // Assert
        Assert.Single(aggregate.Properties);
        Assert.Equal("OrderId", aggregate.Properties[0].Name);
        Assert.True(aggregate.Properties[0].Key);
        Assert.Equal(PropertyKind.Guid, aggregate.Properties[0].Kind);
    }

    [Fact]
    public void Create_CreatesDefaultQueries()
    {
        // Arrange
        var name = "Order";
        var productName = "MyProduct";

        // Act
        var (aggregate, dataContext) = AggregateModel.Create(name, productName);

        // Assert
        Assert.Equal(2, aggregate.Queries.Count);
        Assert.Contains(aggregate.Queries, q => q.Name == "GetOrders" && q.Kind == RequestKind.Get);
        Assert.Contains(aggregate.Queries, q => q.Name == "GetOrderById" && q.Kind == RequestKind.GetById);
    }

    [Fact]
    public void Create_CreatesDefaultCommands()
    {
        // Arrange
        var name = "Order";
        var productName = "MyProduct";

        // Act
        var (aggregate, dataContext) = AggregateModel.Create(name, productName);

        // Assert
        Assert.Equal(3, aggregate.Commands.Count);
        Assert.Contains(aggregate.Commands, c => c.Name == "CreateOrder" && c.Kind == RequestKind.Create);
        Assert.Contains(aggregate.Commands, c => c.Name == "UpdateOrder" && c.Kind == RequestKind.Update);
        Assert.Contains(aggregate.Commands, c => c.Name == "DeleteOrder" && c.Kind == RequestKind.Delete);
    }

    [Fact]
    public void Create_SetsAggregateReferenceOnQueries()
    {
        // Arrange
        var name = "Order";
        var productName = "MyProduct";

        // Act
        var (aggregate, dataContext) = AggregateModel.Create(name, productName);

        // Assert
        Assert.All(aggregate.Queries, q => Assert.Same(aggregate, q.Aggregate));
    }

    [Fact]
    public void Create_SetsAggregateReferenceOnCommands()
    {
        // Arrange
        var name = "Order";
        var productName = "MyProduct";

        // Act
        var (aggregate, dataContext) = AggregateModel.Create(name, productName);

        // Assert
        Assert.All(aggregate.Commands, c => Assert.Same(aggregate, c.Aggregate));
    }

    [Fact]
    public void Create_SetsBoundedContextOnAggregate()
    {
        // Arrange
        var name = "Order";
        var productName = "MyProduct";

        // Act
        var (aggregate, dataContext) = AggregateModel.Create(name, productName);

        // Assert
        Assert.NotNull(aggregate.BoundedContext);
        Assert.Equal("Orders", aggregate.BoundedContext.Name);
    }

    [Fact]
    public void Create_WithNullProductName_UsesNameAsProductName()
    {
        // Arrange
        var name = "Order";
        string? productName = null;

        // Act
        var (aggregate, dataContext) = AggregateModel.Create(name, productName!);

        // Assert
        Assert.Equal(name, dataContext.ProductName);
    }

    [Fact]
    public void Properties_CanAddItems()
    {
        // Arrange
        var aggregate = new AggregateModel("Test");
        var property = new Property("TestProperty", PropertyKind.String);

        // Act
        aggregate.Properties.Add(property);

        // Assert
        Assert.Single(aggregate.Properties);
        Assert.Same(property, aggregate.Properties[0]);
    }

    [Fact]
    public void Commands_CanAddItems()
    {
        // Arrange
        var aggregate = new AggregateModel("Test");
        var command = new Command { Name = "TestCommand" };

        // Act
        aggregate.Commands.Add(command);

        // Assert
        Assert.Single(aggregate.Commands);
        Assert.Same(command, aggregate.Commands[0]);
    }

    [Fact]
    public void Queries_CanAddItems()
    {
        // Arrange
        var aggregate = new AggregateModel("Test");
        var query = new Query { Name = "TestQuery" };

        // Act
        aggregate.Queries.Add(query);

        // Assert
        Assert.Single(aggregate.Queries);
        Assert.Same(query, aggregate.Queries[0]);
    }

    [Fact]
    public void BoundedContext_CanBeSet()
    {
        // Arrange
        var aggregate = new AggregateModel("Test");
        var boundedContext = new BoundedContext("TestContext");

        // Act
        aggregate.BoundedContext = boundedContext;

        // Assert
        Assert.Same(boundedContext, aggregate.BoundedContext);
    }

    [Fact]
    public void BoundedContext_IsNullByDefault()
    {
        // Arrange & Act
        var aggregate = new AggregateModel("Test");

        // Assert
        Assert.Null(aggregate.BoundedContext);
    }
}
