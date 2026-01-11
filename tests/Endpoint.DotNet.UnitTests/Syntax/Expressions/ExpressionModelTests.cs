// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DotNet.Syntax.Classes;
using Endpoint.DotNet.Syntax.Constructors;
using Endpoint.DotNet.Syntax.Expressions;

namespace Endpoint.DotNet.UnitTests.Syntax.Expressions;

public class ExpressionModelTests
{
    [Fact]
    public void ExpressionModel_ShouldCreateInstance_WithExpression()
    {
        // Arrange & Act
        var expressionModel = new ExpressionModel("x => x.Id");

        // Assert
        Assert.NotNull(expressionModel);
        Assert.Equal("x => x.Id", expressionModel.Body);
    }

    [Fact]
    public void ConstructorExpressionModel_ShouldCreateInstance()
    {
        // Arrange
        var classModel = new ClassModel("TestClass");
        var constructorModel = new ConstructorModel(classModel, classModel.Name);

        // Act
        var model = new ConstructorExpressionModel(classModel, constructorModel);

        // Assert
        Assert.NotNull(model);
        Assert.Equal(classModel, model.Class);
        Assert.Equal(constructorModel, model.Constructor);
    }
}
