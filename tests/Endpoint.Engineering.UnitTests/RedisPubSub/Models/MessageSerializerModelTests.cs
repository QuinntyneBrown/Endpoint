// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.RedisPubSub.Models;

namespace Endpoint.Engineering.UnitTests.RedisPubSub.Models;

public class MessageSerializerModelTests
{
    [Fact]
    public void Name_ShouldDefaultToMessagePackMessageSerializer()
    {
        // Arrange & Act
        var model = new MessageSerializerModel();

        // Assert
        Assert.Equal("MessagePackMessageSerializer", model.Name);
    }

    [Fact]
    public void Namespace_ShouldDefaultToEmptyString()
    {
        // Arrange & Act
        var model = new MessageSerializerModel();

        // Assert
        Assert.Equal(string.Empty, model.Namespace);
    }

    [Fact]
    public void UseLz4Compression_ShouldDefaultToTrue()
    {
        // Arrange & Act
        var model = new MessageSerializerModel();

        // Assert
        Assert.True(model.UseLz4Compression);
    }

    [Fact]
    public void AllProperties_ShouldBeSettable()
    {
        // Arrange
        var model = new MessageSerializerModel
        {
            Name = "CustomSerializer",
            Namespace = "MyApp.Services",
            UseLz4Compression = false
        };

        // Assert
        Assert.Equal("CustomSerializer", model.Name);
        Assert.Equal("MyApp.Services", model.Namespace);
        Assert.False(model.UseLz4Compression);
    }
}
