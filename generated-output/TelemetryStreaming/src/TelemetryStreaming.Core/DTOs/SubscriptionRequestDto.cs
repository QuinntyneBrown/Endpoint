// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace TelemetryStreaming.Core.DTOs;

public class SubscriptionRequestDto
{
    public required string ClientId { get; set; }
    public List<string> Metrics { get; set; } = new List<string>();
    public List<string> Sources { get; set; } = new List<string>();
    public int UpdateRateMs { get; set; } = 1000;
}