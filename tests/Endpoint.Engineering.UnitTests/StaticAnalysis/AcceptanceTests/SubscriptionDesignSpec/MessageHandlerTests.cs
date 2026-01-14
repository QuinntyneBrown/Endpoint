// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.AcceptanceTests.SubscriptionDesignSpec;

/// <summary>
/// Acceptance tests for AC-SUB-004.1: Message handlers MUST implement HandleAsync method.
/// Spec: subscription-design.spec.md
/// </summary>
public class MessageHandlerTests : StaticAnalysisTestBase
{
    [Fact]
    public async Task AC_SUB_004_1_MissingHandleAsync_ShouldReportViolation()
    {
        // Arrange
        CreateCSharpFileWithHeader("OrderCreatedHandler.cs", @"
namespace TestNamespace;

public class OrderCreatedHandler : IMessageHandler<OrderCreatedEvent>
{
    public void Process(OrderCreatedEvent message)
    {
        // Wrong method name
    }
}
");

        // Act
        var result = await RunAnalysisAsync();

        // Assert
        AssertViolationExists(result, "AC-SUB-004.1");
        Assert.Contains(result.Violations, v =>
            v.RuleId == "AC-SUB-004.1" &&
            v.Message.Contains("HandleAsync") &&
            v.SpecSource == "subscription-design.spec.md");
    }

    [Fact]
    public async Task AC_SUB_004_1_WithHandleAsync_ShouldPass()
    {
        // Arrange
        CreateCSharpFileWithHeader("OrderCreatedHandler.cs", @"
namespace TestNamespace;

public class OrderCreatedHandler : IMessageHandler<OrderCreatedEvent>
{
    public async Task HandleAsync(OrderCreatedEvent message, MessageContext context, CancellationToken ct)
    {
        await Task.CompletedTask;
    }
}
");

        // Act
        var result = await RunAnalysisAsync();

        // Assert
        AssertNoViolation(result, "AC-SUB-004.1");
    }

    [Fact]
    public async Task AC_SUB_004_1_MessageHandlerBase_WithHandleAsync_ShouldPass()
    {
        // Arrange
        CreateCSharpFileWithHeader("VehicleListedHandler.cs", @"
namespace TestNamespace;

public sealed class VehicleListedHandler : MessageHandlerBase<VehicleListedEvent>
{
    public override async Task HandleAsync(
        VehicleListedEvent message,
        MessageContext context,
        CancellationToken ct)
    {
        await Task.CompletedTask;
    }
}
");

        // Act
        var result = await RunAnalysisAsync();

        // Assert
        AssertNoViolation(result, "AC-SUB-004.1");
    }

    [Fact]
    public async Task AC_SUB_004_1_MessageHandlerBase_MissingHandleAsync_ShouldReportViolation()
    {
        // Arrange
        CreateCSharpFileWithHeader("VehicleListedHandler.cs", @"
namespace TestNamespace;

public sealed class VehicleListedHandler : MessageHandlerBase<VehicleListedEvent>
{
    public void Handle(VehicleListedEvent message)
    {
        // Wrong method
    }
}
");

        // Act
        var result = await RunAnalysisAsync();

        // Assert
        AssertViolationExists(result, "AC-SUB-004.1");
    }
}
