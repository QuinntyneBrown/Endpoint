// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.AcceptanceTests.MessageDesignSpec;

/// <summary>
/// Acceptance tests for AC-MSG-007: Message channel naming conventions.
/// - AC-MSG-007.2: Domain MUST be lowercase
/// - AC-MSG-007.3: Aggregate MUST be lowercase
/// - AC-MSG-007.4: Event type MUST be lowercase
/// Spec: message-design.spec.md
/// </summary>
public class MessageChannelNamingTests : StaticAnalysisTestBase
{
    [Fact]
    public async Task AC_MSG_007_2_UppercaseDomain_ShouldReportViolation()
    {
        // Arrange
        CreateCSharpFileWithHeader("OrderCreatedEvent.cs", @"
using MessagePack;

namespace TestNamespace;

[MessagePackObject]
[MessageChannel(""Orders"", ""order"", ""created"", version: 1)]
public class OrderCreatedEvent : IDomainEvent
{
    [Key(0)]
    public string OrderId { get; init; }
}
");

        // Act
        var result = await RunAnalysisAsync();

        // Assert
        AssertViolationExists(result, "AC-MSG-007.2");
        Assert.Contains(result.Violations, v =>
            v.RuleId == "AC-MSG-007.2" &&
            v.Message.Contains("lowercase") &&
            v.Message.Contains("Orders") &&
            v.SpecSource == "message-design.spec.md");
    }

    [Fact]
    public async Task AC_MSG_007_3_UppercaseAggregate_ShouldReportViolation()
    {
        // Arrange
        CreateCSharpFileWithHeader("OrderCreatedEvent.cs", @"
using MessagePack;

namespace TestNamespace;

[MessagePackObject]
[MessageChannel(""orders"", ""Order"", ""created"", version: 1)]
public class OrderCreatedEvent : IDomainEvent
{
    [Key(0)]
    public string OrderId { get; init; }
}
");

        // Act
        var result = await RunAnalysisAsync();

        // Assert
        AssertViolationExists(result, "AC-MSG-007.3");
        Assert.Contains(result.Violations, v =>
            v.RuleId == "AC-MSG-007.3" &&
            v.Message.Contains("lowercase") &&
            v.Message.Contains("Order") &&
            v.SpecSource == "message-design.spec.md");
    }

    [Fact]
    public async Task AC_MSG_007_4_UppercaseEventType_ShouldReportViolation()
    {
        // Arrange
        CreateCSharpFileWithHeader("OrderCreatedEvent.cs", @"
using MessagePack;

namespace TestNamespace;

[MessagePackObject]
[MessageChannel(""orders"", ""order"", ""Created"", version: 1)]
public class OrderCreatedEvent : IDomainEvent
{
    [Key(0)]
    public string OrderId { get; init; }
}
");

        // Act
        var result = await RunAnalysisAsync();

        // Assert
        AssertViolationExists(result, "AC-MSG-007.4");
        Assert.Contains(result.Violations, v =>
            v.RuleId == "AC-MSG-007.4" &&
            v.Message.Contains("lowercase") &&
            v.Message.Contains("Created") &&
            v.SpecSource == "message-design.spec.md");
    }

    [Fact]
    public async Task AC_MSG_007_AllLowercase_ShouldPass()
    {
        // Arrange
        CreateCSharpFileWithHeader("OrderCreatedEvent.cs", @"
using MessagePack;

namespace TestNamespace;

[MessagePackObject]
[MessageChannel(""orders"", ""order"", ""created"", version: 1)]
public class OrderCreatedEvent : IDomainEvent
{
    [Key(0)]
    public string OrderId { get; init; }
}
");

        // Act
        var result = await RunAnalysisAsync();

        // Assert
        AssertNoViolation(result, "AC-MSG-007.2");
        AssertNoViolation(result, "AC-MSG-007.3");
        AssertNoViolation(result, "AC-MSG-007.4");
    }

    [Fact]
    public async Task AC_MSG_007_MultipleViolations_ShouldReportAll()
    {
        // Arrange - All three parts are uppercase
        CreateCSharpFileWithHeader("OrderCreatedEvent.cs", @"
using MessagePack;

namespace TestNamespace;

[MessagePackObject]
[MessageChannel(""Orders"", ""Order"", ""Created"", version: 1)]
public class OrderCreatedEvent : IDomainEvent
{
    [Key(0)]
    public string OrderId { get; init; }
}
");

        // Act
        var result = await RunAnalysisAsync();

        // Assert - Should have three violations
        AssertViolationExists(result, "AC-MSG-007.2");
        AssertViolationExists(result, "AC-MSG-007.3");
        AssertViolationExists(result, "AC-MSG-007.4");
    }

    [Fact]
    public async Task AC_MSG_007_RealWorldExample_ShouldPass()
    {
        // Arrange - Example from spec: vehicles.listing.created.v1
        CreateCSharpFileWithHeader("VehicleListedEvent.cs", @"
using MessagePack;

namespace TestNamespace;

[MessagePackObject]
[MessageChannel(""vehicles"", ""listing"", ""created"", version: 1)]
public class VehicleListedEvent : IDomainEvent
{
    [Key(0)]
    public required string VehicleId { get; init; }

    [Key(1)]
    public required string DealerId { get; init; }

    [Key(2)]
    public required string Vin { get; init; }
}
");

        // Act
        var result = await RunAnalysisAsync();

        // Assert
        AssertNoViolation(result, "AC-MSG-007.2");
        AssertNoViolation(result, "AC-MSG-007.3");
        AssertNoViolation(result, "AC-MSG-007.4");
    }
}
