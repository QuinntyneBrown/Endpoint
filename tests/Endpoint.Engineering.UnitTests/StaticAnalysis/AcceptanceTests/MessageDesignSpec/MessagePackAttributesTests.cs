// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.AcceptanceTests.MessageDesignSpec;

/// <summary>
/// Acceptance tests for AC-MSG-005.2: All serialized properties MUST be decorated with [Key(n)] attribute.
/// Spec: message-design.spec.md
/// </summary>
public class MessagePackAttributesTests : StaticAnalysisTestBase
{
    [Fact]
    public async Task AC_MSG_005_2_PropertyWithoutKeyAttribute_ShouldReportWarning()
    {
        // Arrange
        CreateCSharpFileWithHeader("OrderEvent.cs", @"
using MessagePack;

namespace TestNamespace;

[MessagePackObject]
public class OrderEvent : IMessage
{
    [Key(0)]
    public string OrderId { get; init; }

    // Missing [Key] attribute
    public string CustomerName { get; init; }
}
");

        // Act
        var result = await RunAnalysisAsync();

        // Assert
        AssertWarningExists(result, "AC-MSG-005.2");
        Assert.Contains(result.Warnings, w =>
            w.RuleId == "AC-MSG-005.2" &&
            w.Message.Contains("[Key(n)]") &&
            w.SpecSource == "message-design.spec.md");
    }

    [Fact]
    public async Task AC_MSG_005_2_PropertyWithIgnoreMemberAttribute_ShouldPass()
    {
        // Arrange
        CreateCSharpFileWithHeader("OrderEvent.cs", @"
using MessagePack;

namespace TestNamespace;

[MessagePackObject]
public class OrderEvent : IMessage
{
    [Key(0)]
    public string OrderId { get; init; }

    [IgnoreMember]
    public string ComputedValue { get; init; }
}
");

        // Act
        var result = await RunAnalysisAsync();

        // Assert
        AssertNoWarning(result, "AC-MSG-005.2");
    }

    [Fact]
    public async Task AC_MSG_005_2_AllPropertiesWithKeyAttribute_ShouldPass()
    {
        // Arrange
        CreateCSharpFileWithHeader("OrderEvent.cs", @"
using MessagePack;

namespace TestNamespace;

[MessagePackObject]
public class OrderEvent : IMessage
{
    [Key(0)]
    public string OrderId { get; init; }

    [Key(1)]
    public string CustomerName { get; init; }

    [Key(2)]
    public decimal Amount { get; init; }
}
");

        // Act
        var result = await RunAnalysisAsync();

        // Assert
        AssertNoWarning(result, "AC-MSG-005.2");
    }

    [Fact]
    public async Task AC_MSG_005_2_MixOfKeyAndIgnoreMember_ShouldPass()
    {
        // Arrange
        CreateCSharpFileWithHeader("VehicleListedEvent.cs", @"
using MessagePack;

namespace TestNamespace;

[MessagePackObject]
public class VehicleListedEvent : IDomainEvent
{
    [Key(0)]
    public required string VehicleId { get; init; }

    [Key(1)]
    public required string DealerId { get; init; }

    [Key(2)]
    public required decimal AskingPrice { get; init; }

    // IDomainEvent implementation
    [IgnoreMember]
    public string AggregateId => VehicleId;

    [IgnoreMember]
    public string AggregateType => ""Vehicle"";
}
");

        // Act
        var result = await RunAnalysisAsync();

        // Assert
        AssertNoWarning(result, "AC-MSG-005.2");
    }

    [Fact]
    public async Task AC_MSG_005_2_NonMessagePackClass_ShouldNotBeAnalyzed()
    {
        // Arrange - Regular class without [MessagePackObject] should not trigger warnings
        CreateCSharpFileWithHeader("RegularClass.cs", @"
namespace TestNamespace;

public class RegularClass
{
    public string Name { get; init; }
    public int Value { get; init; }
}
");

        // Act
        var result = await RunAnalysisAsync();

        // Assert
        AssertNoWarning(result, "AC-MSG-005.2");
    }
}
