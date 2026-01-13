// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace TelemetryStreaming.Core.Entities;

public class TelemetrySubscription
{
    public Guid SubscriptionId { get; set; }
    public required string ConnectionId { get; set; }
    public required string ClientId { get; set; }
    public List<string> SubscribedMetrics { get; set; } = new List<string>();
    public List<string> SubscribedSources { get; set; } = new List<string>();
    public int UpdateRateMs { get; set; } = 1000;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastUpdateAt { get; set; }
    public bool IsActive { get; set; } = true;
}