// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DotNet.Syntax.Expressions;
using TypeModel = Endpoint.DotNet.Syntax.Types.TypeModel;

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
        Assert.Equal("x => x.Id", expressionModel.Expression);
    }

    [Fact]
    public void ConstructorExpressionModel_ShouldCreateInstance()
    {
        // Arrange & Act
        var model = new ConstructorExpressionModel("testValue");

        // Assert
        Assert.NotNull(model);
    }
}
