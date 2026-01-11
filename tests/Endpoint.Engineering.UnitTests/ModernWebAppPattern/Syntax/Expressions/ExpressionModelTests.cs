// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.DomainDrivenDesign.Models;
using Endpoint.Engineering.ModernWebAppPattern.Syntax.Expressions;

namespace Endpoint.Engineering.UnitTests.ModernWebAppPattern.Syntax.Expressions;

public class ExpressionModelTests
{
    [Fact]
    public void ToDtoExpressionModel_ShouldCreateInstance()
    {
        // Arrange
        var aggregate = new AggregateModel { Name = "User" };

        // Act
        var model = new ToDtoExpressionModel(aggregate);

        // Assert
        Assert.NotNull(model);
        Assert.Equal(aggregate, model.Aggregate);
    }

    [Fact]
    public void CommandRequestValidatorConstructorExpressionModel_ShouldCreateInstance()
    {
        // Arrange
        var command = new Command { Name = "CreateUser" };

        // Act
        var model = new CommandRequestValidatorConstructorExpressionModel(command);

        // Assert
        Assert.NotNull(model);
        Assert.Equal(command, model.Command);
    }
}
