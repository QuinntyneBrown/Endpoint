// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Identity.Core.Interfaces;

namespace Identity.Core.Events;

/// <summary>
/// Event raised when a new user is created.
/// </summary>
public sealed class UserCreatedEvent : IDomainEvent
{
    public Guid AggregateId { get; init; }

    public string AggregateType => "User";

    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;

    public required string CorrelationId { get; init; }

    public required string Username { get; init; }

    public required string Email { get; init; }
}