// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DomainDrivenDesign.Models;
using Endpoint.ModernWebAppPattern.Syntax.Expressions.RequestHandlers;

namespace Endpoint.ModernWebAppPattern.UnitTests.Syntax.Expressions.RequestHandlers;

public class RequestHandlerExpressionModelTests
{
    [Fact]
    public void GetByIdRequestHandlerExpressionModel_ShouldCreateInstance()
    {
        // Arrange
        var query = new Query { Name = "GetUserById" };

        // Act
        var model = new GetByIdRequestHandlerExpressionModel(query);

        // Assert
        Assert.NotNull(model);
        Assert.Equal(query, model.Query);
    }

    [Fact]
    public void GetRequestHandlerExpressionModel_ShouldCreateInstance()
    {
        // Arrange
        var query = new Query { Name = "GetAllUsers" };

        // Act
        var model = new GetRequestHandlerExpressionModel(query);

        // Assert
        Assert.NotNull(model);
        Assert.Equal(query, model.Query);
    }

    [Fact]
    public void CreateRequestHandlerExpressionModel_ShouldCreateInstance()
    {
        // Arrange
        var command = new Command { Name = "CreateUser" };

        // Act
        var model = new CreateRequestHandlerExpressionModel(command);

        // Assert
        Assert.NotNull(model);
        Assert.Equal(command, model.Command);
    }

    [Fact]
    public void UpdateRequestHandlerExpressionModel_ShouldCreateInstance()
    {
        // Arrange
        var command = new Command { Name = "UpdateUser" };

        // Act
        var model = new UpdateRequestHandlerExpressionModel(command);

        // Assert
        Assert.NotNull(model);
        Assert.Equal(command, model.Command);
    }

    [Fact]
    public void DeleteRequestHandlerExpressionModel_ShouldCreateInstance()
    {
        // Arrange
        var command = new Command { Name = "DeleteUser" };

        // Act
        var model = new DeleteRequestHandlerExpressionModel(command);

        // Assert
        Assert.NotNull(model);
        Assert.Equal(command, model.Command);
    }
}
