// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.AcceptanceTests.MessageDesignSpec;

/// <summary>
/// Acceptance tests for REQ-MSG-001: Messages MUST implement IMessage marker interface.
/// Spec: message-design.spec.md
/// </summary>
public class MessageInterfaceTests : StaticAnalysisTestBase
{
    [Fact]
    public async Task REQ_MSG_001_MessagePackObjectWithoutIMessage_ShouldReportWarning()
    {
        // Arrange
        CreateCSharpFileWithHeader("OrderCreatedEvent.cs", @"
using MessagePack;

namespace TestNamespace;

[MessagePackObject]
public class OrderCreatedEvent
{
    [Key(0)]
    public string OrderId { get; init; }
}
");

        // Act
        var result = await RunAnalysisAsync();

        // Assert
        AssertWarningExists(result, "REQ-MSG-001");
        Assert.Contains(result.Warnings, w =>
            w.RuleId == "REQ-MSG-001" &&
            w.Message.Contains("IMessage") &&
            w.SpecSource == "message-design.spec.md");
    }

    [Fact]
    public async Task REQ_MSG_001_MessagePackObjectWithIMessage_ShouldPass()
    {
        // Arrange
        CreateCSharpFileWithHeader("OrderCreatedEvent.cs", @"
using MessagePack;

namespace TestNamespace;

[MessagePackObject]
public class OrderCreatedEvent : IMessage
{
    [Key(0)]
    public string OrderId { get; init; }
}
");

        // Act
        var result = await RunAnalysisAsync();

        // Assert
        AssertNoWarning(result, "REQ-MSG-001");
    }

    [Fact]
    public async Task REQ_MSG_001_MessagePackObjectWithIDomainEvent_ShouldPass()
    {
        // Arrange
        CreateCSharpFileWithHeader("OrderCreatedEvent.cs", @"
using MessagePack;

namespace TestNamespace;

[MessagePackObject]
public class OrderCreatedEvent : IDomainEvent
{
    [Key(0)]
    public string OrderId { get; init; }

    public string AggregateId => OrderId;
    public string AggregateType => ""Order"";
}
");

        // Act
        var result = await RunAnalysisAsync();

        // Assert
        AssertNoWarning(result, "REQ-MSG-001");
    }

    [Fact]
    public async Task REQ_MSG_001_MessagePackObjectWithICommand_ShouldPass()
    {
        // Arrange
        CreateCSharpFileWithHeader("CreateOrderCommand.cs", @"
using MessagePack;

namespace TestNamespace;

[MessagePackObject]
public class CreateOrderCommand : ICommand
{
    [Key(0)]
    public string CustomerId { get; init; }

    public string TargetId => CustomerId;
}
");

        // Act
        var result = await RunAnalysisAsync();

        // Assert
        AssertNoWarning(result, "REQ-MSG-001");
    }

    [Fact]
    public async Task REQ_MSG_001_MessageEnvelopeClass_ShouldPass()
    {
        // Arrange - MessageEnvelope classes are infrastructure, not messages
        CreateCSharpFileWithHeader("MessageEnvelope.cs", @"
using MessagePack;

namespace TestNamespace;

[MessagePackObject]
public class MessageEnvelope<T> where T : IMessage
{
    [Key(0)]
    public MessageHeader Header { get; init; }

    [Key(1)]
    public T Payload { get; init; }
}
");

        // Act
        var result = await RunAnalysisAsync();

        // Assert
        AssertNoWarning(result, "REQ-MSG-001");
    }
}
