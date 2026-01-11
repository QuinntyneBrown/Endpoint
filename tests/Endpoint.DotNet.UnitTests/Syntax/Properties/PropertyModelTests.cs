// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DotNet.Syntax.Properties;
using TypeModel = Endpoint.DotNet.Syntax.Types.TypeModel;

namespace Endpoint.DotNet.UnitTests.Syntax.Properties;

public class PropertyModelTests
{
    [Fact]
    public void PropertyModel_ShouldCreateInstance_WithEmptyConstructor()
    {
        // Arrange & Act
        var propertyModel = new PropertyModel();

        // Assert
        Assert.NotNull(propertyModel);
        Assert.NotNull(propertyModel.Accessors);
        Assert.NotNull(propertyModel.Attributes);
    }

    [Fact]
    public void PropertyModel_ShouldCreateInstance_WithTypeAndName()
    {
        // Arrange
        var type = new TypeModel("string");
        var accessor = PropertyAccessorModel.Get;

        // Act
        var propertyModel = new PropertyModel(type, "Name", accessor);

        // Assert
        Assert.NotNull(propertyModel);
        Assert.Equal("Name", propertyModel.Name);
        Assert.Equal(type, propertyModel.Type);
        Assert.Single(propertyModel.Accessors);
        Assert.True(propertyModel.Interface);
    }

    [Fact]
    public void PropertyModel_ShouldStoreDefaultValue()
    {
        // Arrange
        var propertyModel = new PropertyModel();

        // Act
        propertyModel.DefaultValue = "\"default\"";

        // Assert
        Assert.Equal("\"default\"", propertyModel.DefaultValue);
    }

    [Fact]
    public void PropertyModel_ShouldHaveInterfaceFlagSet()
    {
        // Arrange
        var propertyModel = new PropertyModel();

        // Act
        propertyModel.Interface = true;

        // Assert
        Assert.True(propertyModel.Interface);
    }
}
