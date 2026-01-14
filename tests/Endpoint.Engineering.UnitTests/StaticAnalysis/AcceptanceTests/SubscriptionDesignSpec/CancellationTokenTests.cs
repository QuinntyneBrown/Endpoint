// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.AcceptanceTests.SubscriptionDesignSpec;

/// <summary>
/// Acceptance tests for AC5.4: Generation methods MUST accept CancellationToken.
/// Spec: implementation.spec.md (applied to message handlers)
/// </summary>
public class CancellationTokenTests : StaticAnalysisTestBase
{
    [Fact]
    public async Task AC5_4_HandleAsyncWithoutCancellationToken_ShouldReportWarning()
    {
        // Arrange
        CreateCSharpFileWithHeader("OrderCreatedHandler.cs", @"
namespace TestNamespace;

public class OrderCreatedHandler : IMessageHandler<OrderCreatedEvent>
{
    public async Task HandleAsync(OrderCreatedEvent message, MessageContext context)
    {
        // Missing CancellationToken parameter
        await Task.CompletedTask;
    }
}
");

        // Act
        var result = await RunAnalysisAsync();

        // Assert
        AssertWarningExists(result, "AC5.4");
        Assert.Contains(result.Warnings, w =>
            w.RuleId == "AC5.4" &&
            w.Message.Contains("CancellationToken") &&
            w.SpecSource == "implementation.spec.md");
    }

    [Fact]
    public async Task AC5_4_HandleAsyncWithCancellationToken_ShouldPass()
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
        AssertNoWarning(result, "AC5.4");
    }

    [Fact]
    public async Task AC5_4_HandleAsyncWithCancellationTokenDefaultValue_ShouldPass()
    {
        // Arrange
        CreateCSharpFileWithHeader("OrderCreatedHandler.cs", @"
namespace TestNamespace;

public class OrderCreatedHandler : IMessageHandler<OrderCreatedEvent>
{
    public async Task HandleAsync(
        OrderCreatedEvent message,
        MessageContext context,
        CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
    }
}
");

        // Act
        var result = await RunAnalysisAsync();

        // Assert
        AssertNoWarning(result, "AC5.4");
    }

    [Fact]
    public async Task AC5_4_MessageHandlerBase_WithCancellationToken_ShouldPass()
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
        AssertNoWarning(result, "AC5.4");
    }
}
