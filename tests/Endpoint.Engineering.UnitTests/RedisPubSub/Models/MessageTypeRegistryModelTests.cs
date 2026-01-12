// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.RedisPubSub.Models;

namespace Endpoint.Engineering.UnitTests.RedisPubSub.Models;

public class MessageTypeRegistryModelTests
{
    [Fact]
    public void Name_ShouldDefaultToMessageTypeRegistry()
    {
        // Arrange & Act
        var model = new MessageTypeRegistryModel();

        // Assert
        Assert.Equal("MessageTypeRegistry", model.Name);
    }

    [Fact]
    public void Namespace_ShouldDefaultToEmptyString()
    {
        // Arrange & Act
        var model = new MessageTypeRegistryModel();

        // Assert
        Assert.Equal(string.Empty, model.Namespace);
    }

    [Fact]
    public void AllProperties_ShouldBeSettable()
    {
        // Arrange
        var model = new MessageTypeRegistryModel
        {
            Name = "CustomRegistry",
            Namespace = "MyApp.Services"
        };

        // Assert
        Assert.Equal("CustomRegistry", model.Name);
        Assert.Equal("MyApp.Services", model.Namespace);
    }
}
