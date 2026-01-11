// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Engineering.UnitTests.DomainDrivenDesign;

public class QueryTests
{
    [Fact]
    public void DefaultConstructor_CreatesInstance()
    {
        // Arrange & Act
        var query = new Query();

        // Assert
        Assert.NotNull(query);
    }

    [Fact]
    public void Name_IsEmptyStringByDefault()
    {
        // Arrange & Act
        var query = new Query();

        // Assert
        Assert.Equal(string.Empty, query.Name);
    }

    [Fact]
    public void Name_CanBeSet()
    {
        // Arrange
        var query = new Query();
        var expectedName = "GetOrders";

        // Act
        query.Name = expectedName;

        // Assert
        Assert.Equal(expectedName, query.Name);
    }

    [Fact]
    public void Name_CanBeModified()
    {
        // Arrange
        var query = new Query { Name = "InitialName" };
        var newName = "UpdatedName";

        // Act
        query.Name = newName;

        // Assert
        Assert.Equal(newName, query.Name);
    }

    [Fact]
    public void Query_InheritsFromRequest()
    {
        // Arrange & Act
        var query = new Query();

        // Assert
        Assert.IsAssignableFrom<Request>(query);
    }

    [Fact]
    public void Kind_CanBeSet()
    {
        // Arrange
        var query = new Query();

        // Act
        query.Kind = RequestKind.Get;

        // Assert
        Assert.Equal(RequestKind.Get, query.Kind);
    }

    [Fact]
    public void Aggregate_CanBeSet()
    {
        // Arrange
        var query = new Query();
        var aggregate = new AggregateModel("TestAggregate");

        // Act
        query.Aggregate = aggregate;

        // Assert
        Assert.Same(aggregate, query.Aggregate);
    }

    [Fact]
    public void ProductName_CanBeSet()
    {
        // Arrange
        var query = new Query();
        var productName = "TestProduct";

        // Act
        query.ProductName = productName;

        // Assert
        Assert.Equal(productName, query.ProductName);
    }

    [Fact]
    public void Query_CanBeInitializedWithObjectInitializer()
    {
        // Arrange
        var aggregate = new AggregateModel("Order");
        var expectedName = "GetOrders";
        var expectedKind = RequestKind.Get;
        var expectedProductName = "MyProduct";

        // Act
        var query = new Query
        {
            Name = expectedName,
            Kind = expectedKind,
            Aggregate = aggregate,
            ProductName = expectedProductName
        };

        // Assert
        Assert.Equal(expectedName, query.Name);
        Assert.Equal(expectedKind, query.Kind);
        Assert.Same(aggregate, query.Aggregate);
        Assert.Equal(expectedProductName, query.ProductName);
    }

    [Theory]
    [InlineData(RequestKind.Get)]
    [InlineData(RequestKind.GetById)]
    public void Kind_AcceptsAllQueryRelatedRequestKinds(RequestKind kind)
    {
        // Arrange
        var query = new Query();

        // Act
        query.Kind = kind;

        // Assert
        Assert.Equal(kind, query.Kind);
    }
}
