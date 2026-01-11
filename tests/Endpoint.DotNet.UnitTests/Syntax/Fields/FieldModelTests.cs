// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DotNet.Syntax;
using Endpoint.DotNet.Syntax.Fields;
using TypeModel = Endpoint.DotNet.Syntax.Types.TypeModel;

namespace Endpoint.DotNet.UnitTests.Syntax.Fields;

public class FieldModelTests
{
    [Fact]
    public void FieldModel_ShouldCreateInstance()
    {
        // Arrange & Act
        var field = new FieldModel();

        // Assert
        Assert.NotNull(field);
    }

    [Fact]
    public void FieldModel_ShouldSetNameAndType()
    {
        // Arrange
        var type = new TypeModel("ILogger");
        var field = new FieldModel
        {
            Name = "_logger",
            Type = type,
        };

        // Act & Assert
        Assert.Equal("_logger", field.Name);
        Assert.Equal(type, field.Type);
    }

    [Fact]
    public void FieldModel_ShouldSetReadOnly()
    {
        // Arrange
        var field = new FieldModel
        {
            ReadOnly = true,
        };

        // Act & Assert
        Assert.True(field.ReadOnly);
    }

    [Fact]
    public void FieldModel_ShouldSetAccessModifier()
    {
        // Arrange
        var field = new FieldModel
        {
            AccessModifier = AccessModifier.Private,
        };

        // Act & Assert
        Assert.Equal(AccessModifier.Private, field.AccessModifier);
    }
}
