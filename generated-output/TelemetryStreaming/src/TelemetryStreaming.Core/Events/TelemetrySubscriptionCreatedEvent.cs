// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace TelemetryStreaming.Core.Events;

public record TelemetrySubscriptionCreatedEvent(Guid SubscriptionId, string ConnectionId, string ClientId, DateTime CreatedAt);