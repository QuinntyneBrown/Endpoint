// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.RedisPubSub.Models;

namespace Endpoint.Engineering.UnitTests.RedisPubSub.Models;

public class MessageEnvelopeModelTests
{
    [Fact]
    public void Name_ShouldDefaultToMessageEnvelope()
    {
        // Arrange & Act
        var model = new MessageEnvelopeModel();

        // Assert
        Assert.Equal("MessageEnvelope", model.Name);
    }

    [Fact]
    public void Namespace_ShouldDefaultToEmptyString()
    {
        // Arrange & Act
        var model = new MessageEnvelopeModel();

        // Assert
        Assert.Equal(string.Empty, model.Namespace);
    }

    [Fact]
    public void GenericPayload_ShouldDefaultToTrue()
    {
        // Arrange & Act
        var model = new MessageEnvelopeModel();

        // Assert
        Assert.True(model.GenericPayload);
    }

    [Fact]
    public void AllProperties_ShouldBeSettable()
    {
        // Arrange
        var model = new MessageEnvelopeModel
        {
            Name = "CustomEnvelope",
            Namespace = "MyApp.Messaging",
            GenericPayload = false
        };

        // Assert
        Assert.Equal("CustomEnvelope", model.Name);
        Assert.Equal("MyApp.Messaging", model.Namespace);
        Assert.False(model.GenericPayload);
    }
}
