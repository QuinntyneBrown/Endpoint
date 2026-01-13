// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.AspNetCore.SignalR;
using TelemetryStreaming.Core.DTOs;
using TelemetryStreaming.Core.Interfaces;

namespace TelemetryStreaming.Api.Hubs;

public class TelemetryHub : Hub
{
    private readonly ILogger<TelemetryHub> logger;
    private readonly ISubscriptionManager subscriptionManager;
    private readonly ITelemetryGenerator telemetryGenerator;

    public TelemetryHub(
        ILogger<TelemetryHub> logger,
        ISubscriptionManager subscriptionManager,
        ITelemetryGenerator telemetryGenerator)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.subscriptionManager = subscriptionManager ?? throw new ArgumentNullException(nameof(subscriptionManager));
        this.telemetryGenerator = telemetryGenerator ?? throw new ArgumentNullException(nameof(telemetryGenerator));
    }

    public override async Task OnConnectedAsync()
    {
        logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        await subscriptionManager.RemoveSubscriptionAsync(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task Subscribe(SubscriptionRequestDto request)
    {
        logger.LogInformation("Client {ClientId} subscribing to telemetry", request.ClientId);
        
        var subscription = await subscriptionManager.CreateSubscriptionAsync(Context.ConnectionId, request.ClientId);
        await subscriptionManager.UpdateSubscriptionAsync(
            subscription.SubscriptionId,
            request.Metrics,
            request.Sources,
            request.UpdateRateMs);

        await Clients.Caller.SendAsync("SubscriptionConfirmed", subscription.SubscriptionId);
    }

    public async Task Unsubscribe()
    {
        logger.LogInformation("Client {ConnectionId} unsubscribing from telemetry", Context.ConnectionId);
        await subscriptionManager.RemoveSubscriptionAsync(Context.ConnectionId);
        await Clients.Caller.SendAsync("UnsubscriptionConfirmed");
    }

    public async Task UpdateSubscription(SubscriptionRequestDto request)
    {
        logger.LogInformation("Client updating subscription: {ConnectionId}", Context.ConnectionId);
        var subscription = await subscriptionManager.GetSubscriptionAsync(Context.ConnectionId);
        
        if (subscription != null)
        {
            await subscriptionManager.UpdateSubscriptionAsync(
                subscription.SubscriptionId,
                request.Metrics,
                request.Sources,
                request.UpdateRateMs);

            await Clients.Caller.SendAsync("SubscriptionUpdated");
        }
    }
}