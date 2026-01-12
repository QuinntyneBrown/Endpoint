// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Identity.Core.Interfaces;

/// <summary>
/// Interface for domain events.
/// </summary>
public interface IDomainEvent
{
    Guid AggregateId { get; }

    string AggregateType { get; }

    DateTime OccurredAt { get; }

    string CorrelationId { get; }
}