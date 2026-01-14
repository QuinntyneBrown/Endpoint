// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.AcceptanceTests.MessageDesignSpec;

/// <summary>
/// Acceptance tests for REQ-MSG-002: Message headers MUST contain standardized metadata.
/// Spec: message-design.spec.md
/// </summary>
public class MessageHeaderFieldsTests : StaticAnalysisTestBase
{
    [Fact]
    public async Task REQ_MSG_002_MissingMessageType_ShouldReportViolation()
    {
        // Arrange
        CreateCSharpFileWithHeader("MessageHeader.cs", @"
using MessagePack;

namespace TestNamespace;

[MessagePackObject]
public class MessageHeader
{
    [Key(0)]
    public string MessageId { get; init; }

    [Key(1)]
    public string CorrelationId { get; init; }

    [Key(2)]
    public string CausationId { get; init; }

    [Key(3)]
    public long TimestampUnixMs { get; init; }

    [Key(4)]
    public string SourceService { get; init; }

    [Key(5)]
    public int SchemaVersion { get; init; }
}
");

        // Act
        var result = await RunAnalysisAsync();

        // Assert
        AssertViolationExists(result, "REQ-MSG-002");
        Assert.Contains(result.Violations, v =>
            v.RuleId == "REQ-MSG-002" &&
            v.Message.Contains("MessageType") &&
            v.SpecSource == "message-design.spec.md");
    }

    [Fact]
    public async Task REQ_MSG_002_MissingCorrelationId_ShouldReportViolation()
    {
        // Arrange
        CreateCSharpFileWithHeader("MessageHeader.cs", @"
using MessagePack;

namespace TestNamespace;

[MessagePackObject]
public class MessageHeader
{
    [Key(0)]
    public string MessageType { get; init; }

    [Key(1)]
    public string MessageId { get; init; }

    [Key(2)]
    public string CausationId { get; init; }

    [Key(3)]
    public long TimestampUnixMs { get; init; }

    [Key(4)]
    public string SourceService { get; init; }

    [Key(5)]
    public int SchemaVersion { get; init; }
}
");

        // Act
        var result = await RunAnalysisAsync();

        // Assert
        AssertViolationExists(result, "REQ-MSG-002");
        Assert.Contains(result.Violations, v =>
            v.RuleId == "REQ-MSG-002" &&
            v.Message.Contains("CorrelationId"));
    }

    [Fact]
    public async Task REQ_MSG_002_MissingSchemaVersion_ShouldReportViolation()
    {
        // Arrange
        CreateCSharpFileWithHeader("MessageHeader.cs", @"
using MessagePack;

namespace TestNamespace;

[MessagePackObject]
public class MessageHeader
{
    [Key(0)]
    public string MessageType { get; init; }

    [Key(1)]
    public string MessageId { get; init; }

    [Key(2)]
    public string CorrelationId { get; init; }

    [Key(3)]
    public string CausationId { get; init; }

    [Key(4)]
    public long TimestampUnixMs { get; init; }

    [Key(5)]
    public string SourceService { get; init; }
}
");

        // Act
        var result = await RunAnalysisAsync();

        // Assert
        AssertViolationExists(result, "REQ-MSG-002");
        Assert.Contains(result.Violations, v =>
            v.RuleId == "REQ-MSG-002" &&
            v.Message.Contains("SchemaVersion"));
    }

    [Fact]
    public async Task REQ_MSG_002_AllFieldsPresent_ShouldPass()
    {
        // Arrange
        CreateCSharpFileWithHeader("MessageHeader.cs", @"
using MessagePack;

namespace TestNamespace;

[MessagePackObject]
public class MessageHeader
{
    [Key(0)]
    public string MessageType { get; init; }

    [Key(1)]
    public string MessageId { get; init; }

    [Key(2)]
    public string CorrelationId { get; init; }

    [Key(3)]
    public string CausationId { get; init; }

    [Key(4)]
    public long TimestampUnixMs { get; init; }

    [Key(5)]
    public string SourceService { get; init; }

    [Key(6)]
    public int SchemaVersion { get; init; }
}
");

        // Act
        var result = await RunAnalysisAsync();

        // Assert
        AssertNoViolation(result, "REQ-MSG-002");
    }

    [Fact]
    public async Task REQ_MSG_002_MessageEnvelopeWithAllFields_ShouldPass()
    {
        // Arrange
        CreateCSharpFileWithHeader("MessageEnvelope.cs", @"
using MessagePack;

namespace TestNamespace;

[MessagePackObject]
public class MessageEnvelope<T>
{
    [Key(0)]
    public string MessageType { get; init; }

    [Key(1)]
    public string MessageId { get; init; }

    [Key(2)]
    public string CorrelationId { get; init; }

    [Key(3)]
    public string CausationId { get; init; }

    [Key(4)]
    public long TimestampUnixMs { get; init; }

    [Key(5)]
    public string SourceService { get; init; }

    [Key(6)]
    public int SchemaVersion { get; init; }

    [Key(7)]
    public T Payload { get; init; }
}
");

        // Act
        var result = await RunAnalysisAsync();

        // Assert
        AssertNoViolation(result, "REQ-MSG-002");
    }
}
