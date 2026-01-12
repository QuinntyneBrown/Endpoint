// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.RedisPubSub.Models;

namespace Endpoint.Engineering.UnitTests.RedisPubSub.Models;

public class MessageHeaderModelTests
{
    [Fact]
    public void SchemaVersion_ShouldDefaultToOne()
    {
        // Arrange & Act
        var model = new MessageHeaderModel();

        // Assert
        Assert.Equal(1, model.SchemaVersion);
    }

    [Fact]
    public void MessageType_ShouldDefaultToEmptyString()
    {
        // Arrange & Act
        var model = new MessageHeaderModel();

        // Assert
        Assert.Equal(string.Empty, model.MessageType);
    }

    [Fact]
    public void MessageId_ShouldDefaultToEmptyString()
    {
        // Arrange & Act
        var model = new MessageHeaderModel();

        // Assert
        Assert.Equal(string.Empty, model.MessageId);
    }

    [Fact]
    public void CorrelationId_ShouldDefaultToEmptyString()
    {
        // Arrange & Act
        var model = new MessageHeaderModel();

        // Assert
        Assert.Equal(string.Empty, model.CorrelationId);
    }

    [Fact]
    public void CausationId_ShouldDefaultToEmptyString()
    {
        // Arrange & Act
        var model = new MessageHeaderModel();

        // Assert
        Assert.Equal(string.Empty, model.CausationId);
    }

    [Fact]
    public void SourceService_ShouldDefaultToEmptyString()
    {
        // Arrange & Act
        var model = new MessageHeaderModel();

        // Assert
        Assert.Equal(string.Empty, model.SourceService);
    }

    [Fact]
    public void TimestampUnixMs_ShouldDefaultToZero()
    {
        // Arrange & Act
        var model = new MessageHeaderModel();

        // Assert
        Assert.Equal(0, model.TimestampUnixMs);
    }

    [Fact]
    public void AllProperties_ShouldBeSettable()
    {
        // Arrange
        var model = new MessageHeaderModel
        {
            MessageType = "test.message.v1",
            MessageId = "msg-123",
            CorrelationId = "corr-456",
            CausationId = "cause-789",
            TimestampUnixMs = 1234567890,
            SourceService = "test-service",
            SchemaVersion = 2
        };

        // Assert
        Assert.Equal("test.message.v1", model.MessageType);
        Assert.Equal("msg-123", model.MessageId);
        Assert.Equal("corr-456", model.CorrelationId);
        Assert.Equal("cause-789", model.CausationId);
        Assert.Equal(1234567890, model.TimestampUnixMs);
        Assert.Equal("test-service", model.SourceService);
        Assert.Equal(2, model.SchemaVersion);
    }
}
