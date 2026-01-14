// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.AcceptanceTests.SubscriptionDesignSpec;

/// <summary>
/// Acceptance tests for REQ-SUB-009: Handlers MUST implement idempotency checks.
/// Spec: subscription-design.spec.md
/// </summary>
public class IdempotencyTests : StaticAnalysisTestBase
{
    [Fact]
    public async Task REQ_SUB_009_HandlerWithoutIdempotency_ShouldReportWarning()
    {
        // Arrange
        CreateCSharpFileWithHeader("OrderCreatedHandler.cs", @"
namespace TestNamespace;

public class OrderCreatedHandler : IMessageHandler<OrderCreatedEvent>
{
    private readonly IOrderRepository _repository;

    public OrderCreatedHandler(IOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task HandleAsync(OrderCreatedEvent message, MessageContext context, CancellationToken ct)
    {
        // No idempotency check
        await _repository.AddAsync(new Order { Id = message.OrderId });
    }
}
");

        // Act
        var result = await RunAnalysisAsync();

        // Assert
        AssertWarningExists(result, "REQ-SUB-009");
        Assert.Contains(result.Warnings, w =>
            w.RuleId == "REQ-SUB-009" &&
            w.Message.Contains("idempotency") &&
            w.SpecSource == "subscription-design.spec.md");
    }

    [Fact]
    public async Task REQ_SUB_009_HandlerWithIIdempotencyStore_ShouldPass()
    {
        // Arrange
        CreateCSharpFileWithHeader("OrderCreatedHandler.cs", @"
namespace TestNamespace;

public class OrderCreatedHandler : IMessageHandler<OrderCreatedEvent>
{
    private readonly IOrderRepository _repository;
    private readonly IIdempotencyStore _idempotency;

    public OrderCreatedHandler(IOrderRepository repository, IIdempotencyStore idempotency)
    {
        _repository = repository;
        _idempotency = idempotency;
    }

    public async Task HandleAsync(OrderCreatedEvent message, MessageContext context, CancellationToken ct)
    {
        var key = $""order-created:{message.OrderId}:{context.Header.MessageId}"";
        if (!await _idempotency.TryProcessAsync(key, TimeSpan.FromDays(7), ct))
        {
            return;
        }

        await _repository.AddAsync(new Order { Id = message.OrderId });
    }
}
");

        // Act
        var result = await RunAnalysisAsync();

        // Assert
        AssertNoWarning(result, "REQ-SUB-009");
    }

    [Fact]
    public async Task REQ_SUB_009_HandlerWithHasProcessed_ShouldPass()
    {
        // Arrange
        CreateCSharpFileWithHeader("OrderCreatedHandler.cs", @"
namespace TestNamespace;

public class OrderCreatedHandler : IMessageHandler<OrderCreatedEvent>
{
    private readonly IOrderRepository _repository;
    private readonly IIdempotencyStore _store;

    public OrderCreatedHandler(IOrderRepository repository, IIdempotencyStore store)
    {
        _repository = repository;
        _store = store;
    }

    public async Task HandleAsync(OrderCreatedEvent message, MessageContext context, CancellationToken ct)
    {
        if (await _store.HasProcessed(message.OrderId))
        {
            return;
        }

        await _repository.AddAsync(new Order { Id = message.OrderId });
        await _store.MarkProcessed(message.OrderId);
    }
}
");

        // Act
        var result = await RunAnalysisAsync();

        // Assert
        AssertNoWarning(result, "REQ-SUB-009");
    }

    [Fact]
    public async Task REQ_SUB_009_HandlerWithTryProcess_ShouldPass()
    {
        // Arrange
        CreateCSharpFileWithHeader("VehicleListedHandler.cs", @"
namespace TestNamespace;

public sealed class VehicleListedHandler : MessageHandlerBase<VehicleListedEvent>
{
    private readonly IVehicleRepository _repository;
    private readonly IIdempotencyStore _idempotency;

    public VehicleListedHandler(
        IVehicleRepository repository,
        IIdempotencyStore idempotency,
        ILogger<VehicleListedHandler> logger) : base(logger)
    {
        _repository = repository;
        _idempotency = idempotency;
    }

    public override async Task HandleAsync(
        VehicleListedEvent message,
        MessageContext context,
        CancellationToken ct)
    {
        var key = $""vehicle-listed:{message.VehicleId}:{context.Header.MessageId}"";
        if (!await _idempotency.TryProcessAsync(key, TimeSpan.FromDays(7), ct))
        {
            Logger.LogDebug(""Duplicate message {MessageId}, skipping"", context.Header.MessageId);
            return;
        }

        var vehicle = Vehicle.FromListedEvent(message);
        await _repository.AddAsync(vehicle, ct);
    }
}
");

        // Act
        var result = await RunAnalysisAsync();

        // Assert
        AssertNoWarning(result, "REQ-SUB-009");
    }

    [Fact]
    public async Task REQ_SUB_009_MessageHandlerBase_WithoutIdempotency_ShouldReportWarning()
    {
        // Arrange
        CreateCSharpFileWithHeader("VehicleListedHandler.cs", @"
namespace TestNamespace;

public sealed class VehicleListedHandler : MessageHandlerBase<VehicleListedEvent>
{
    private readonly IVehicleRepository _repository;

    public VehicleListedHandler(
        IVehicleRepository repository,
        ILogger<VehicleListedHandler> logger) : base(logger)
    {
        _repository = repository;
    }

    public override async Task HandleAsync(
        VehicleListedEvent message,
        MessageContext context,
        CancellationToken ct)
    {
        // No idempotency check!
        var vehicle = Vehicle.FromListedEvent(message);
        await _repository.AddAsync(vehicle, ct);
    }
}
");

        // Act
        var result = await RunAnalysisAsync();

        // Assert
        AssertWarningExists(result, "REQ-SUB-009");
    }
}
