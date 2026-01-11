// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DomainDrivenDesign.Models;
using Endpoint.DotNet.Syntax.Classes;
using Endpoint.ModernWebAppPattern.Syntax.Expressions.Controllers;

namespace Endpoint.ModernWebAppPattern.UnitTests.Syntax.Expressions.Controllers;

public class ControllerExpressionModelTests
{
    [Fact]
    public void ControllerGetByIdExpressionModel_ShouldCreateInstance()
    {
        // Arrange
        var classModel = new ClassModel("TestController");
        var query = new Query { Name = "GetById" };

        // Act
        var model = new ControllerGetByIdExpressionModel(classModel, query);

        // Assert
        Assert.NotNull(model);
        Assert.Equal(classModel, model.Class);
        Assert.Equal(query, model.Query);
    }

    [Fact]
    public void ControllerUpdateExpressionModel_ShouldCreateInstance()
    {
        // Arrange
        var classModel = new ClassModel("TestController");
        var command = new Command { Name = "Update" };

        // Act
        var model = new ControllerUpdateExpressionModel(classModel, command);

        // Assert
        Assert.NotNull(model);
        Assert.Equal(classModel, model.Class);
        Assert.Equal(command, model.Command);
    }

    [Fact]
    public void ControllerDeleteExpressionModel_ShouldCreateInstance()
    {
        // Arrange
        var classModel = new ClassModel("TestController");
        var command = new Command { Name = "Delete" };

        // Act
        var model = new ControllerDeleteExpressionModel(classModel, command);

        // Assert
        Assert.NotNull(model);
        Assert.Equal(classModel, model.Class);
        Assert.Equal(command, model.Command);
    }

    [Fact]
    public void ControllerCreateExpressionModel_ShouldCreateInstance()
    {
        // Arrange
        var classModel = new ClassModel("TestController");
        var command = new Command { Name = "Create" };

        // Act
        var model = new ControllerCreateExpressionModel(classModel, command);

        // Assert
        Assert.NotNull(model);
        Assert.Equal(classModel, model.Class);
        Assert.Equal(command, model.Command);
    }

    [Fact]
    public void ControllerGetExpressionModel_ShouldCreateInstance()
    {
        // Arrange
        var classModel = new ClassModel("TestController");
        var query = new Query { Name = "GetAll" };

        // Act
        var model = new ControllerGetExpressionModel(classModel, query);

        // Assert
        Assert.NotNull(model);
        Assert.Equal(classModel, model.Class);
        Assert.Equal(query, model.Query);
    }
}
