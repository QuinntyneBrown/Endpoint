// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DotNet.Syntax.Params;
using TypeModel = Endpoint.DotNet.Syntax.Types.TypeModel;

namespace Endpoint.DotNet.UnitTests.Syntax.Params;

public class ParamModelTests
{
    [Fact]
    public void ParamModel_ShouldCreateInstance_WithProperties()
    {
        // Arrange
        var type = new TypeModel("string");
        
        // Act
        var param = new ParamModel
        {
            Name = "userId",
            Type = type,
        };

        // Assert
        Assert.NotNull(param);
        Assert.Equal("userId", param.Name);
        Assert.Equal(type, param.Type);
    }

    [Fact]
    public void ParamModel_ShouldAllowSettingDefaultValue()
    {
        // Arrange
        var param = new ParamModel();

        // Act
        param.DefaultValue = "null";

        // Assert
        Assert.Equal("null", param.DefaultValue);
    }

    [Fact]
    public void ParamModel_ShouldAllowSettingExtensionMethodParam()
    {
        // Arrange
        var param = new ParamModel();

        // Act
        param.ExtensionMethodParam = true;

        // Assert
        Assert.True(param.ExtensionMethodParam);
    }

    [Fact]
    public void ParamModel_ToStringWhenNameOnly()
    {
        // Arrange
        var param = new ParamModel { Name = "test" };

        // Act
        var result = param.ToString();

        // Assert
        Assert.Equal("test", result);
    }
}
