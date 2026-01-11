// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Angular.Syntax;

namespace Endpoint.Angular.UnitTests.Syntax;

public class TypeScriptTypeModelTests
{
    [Fact]
    public void TypeScriptTypeModel_ShouldCreateInstance()
    {
        // Arrange & Act
        var type = new TypeScriptTypeModel("TestType");

        // Assert
        Assert.NotNull(type);
    }

    [Fact]
    public void TypeScriptTypeModel_ShouldSetName()
    {
        // Arrange
        var type = new TypeScriptTypeModel("Observable");

        // Act & Assert
        Assert.Equal("Observable", type.Name);
    }
}
