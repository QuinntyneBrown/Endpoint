// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.DomainDrivenDesign.UnitTests;

public class CommandTests
{
    [Fact]
    public void DefaultConstructor_CreatesInstance()
    {
        // Arrange & Act
        var command = new Command();

        // Assert
        Assert.NotNull(command);
    }

    [Fact]
    public void Name_IsEmptyStringByDefault()
    {
        // Arrange & Act
        var command = new Command();

        // Assert
        Assert.Equal(string.Empty, command.Name);
    }

    [Fact]
    public void Name_CanBeSet()
    {
        // Arrange
        var command = new Command();
        var expectedName = "CreateOrder";

        // Act
        command.Name = expectedName;

        // Assert
        Assert.Equal(expectedName, command.Name);
    }

    [Fact]
    public void Name_CanBeModified()
    {
        // Arrange
        var command = new Command { Name = "InitialName" };
        var newName = "UpdatedName";

        // Act
        command.Name = newName;

        // Assert
        Assert.Equal(newName, command.Name);
    }

    [Fact]
    public void Command_InheritsFromRequest()
    {
        // Arrange & Act
        var command = new Command();

        // Assert
        Assert.IsAssignableFrom<Request>(command);
    }

    [Fact]
    public void Kind_CanBeSet()
    {
        // Arrange
        var command = new Command();

        // Act
        command.Kind = RequestKind.Create;

        // Assert
        Assert.Equal(RequestKind.Create, command.Kind);
    }

    [Fact]
    public void Aggregate_CanBeSet()
    {
        // Arrange
        var command = new Command();
        var aggregate = new AggregateModel("TestAggregate");

        // Act
        command.Aggregate = aggregate;

        // Assert
        Assert.Same(aggregate, command.Aggregate);
    }

    [Fact]
    public void ProductName_CanBeSet()
    {
        // Arrange
        var command = new Command();
        var productName = "TestProduct";

        // Act
        command.ProductName = productName;

        // Assert
        Assert.Equal(productName, command.ProductName);
    }

    [Fact]
    public void Command_CanBeInitializedWithObjectInitializer()
    {
        // Arrange
        var aggregate = new AggregateModel("Order");
        var expectedName = "CreateOrder";
        var expectedKind = RequestKind.Create;
        var expectedProductName = "MyProduct";

        // Act
        var command = new Command
        {
            Name = expectedName,
            Kind = expectedKind,
            Aggregate = aggregate,
            ProductName = expectedProductName
        };

        // Assert
        Assert.Equal(expectedName, command.Name);
        Assert.Equal(expectedKind, command.Kind);
        Assert.Same(aggregate, command.Aggregate);
        Assert.Equal(expectedProductName, command.ProductName);
    }

    [Theory]
    [InlineData(RequestKind.Create)]
    [InlineData(RequestKind.Update)]
    [InlineData(RequestKind.Delete)]
    public void Kind_AcceptsAllCommandRelatedRequestKinds(RequestKind kind)
    {
        // Arrange
        var command = new Command();

        // Act
        command.Kind = kind;

        // Assert
        Assert.Equal(kind, command.Kind);
    }
}
