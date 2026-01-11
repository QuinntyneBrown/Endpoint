// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Angular.Syntax;
using Endpoint.Syntax;

namespace Endpoint.Angular.UnitTests.Syntax;

public class PropertyModelTests
{
    [Fact]
    public void PropertyModel_ShouldCreateInstance()
    {
        // Arrange & Act
        var property = new PropertyModel();

        // Assert
        Assert.NotNull(property);
    }

    [Fact]
    public void PropertyModel_ShouldSetNameAndType()
    {
        // Arrange
        var property = new PropertyModel
        {
            Name = "userId",
            Type = new TypeModel("string"),
        };

        // Act & Assert
        Assert.Equal("userId", property.Name);
        Assert.NotNull(property.Type);
        Assert.Equal("string", property.Type.Name);
    }
}
