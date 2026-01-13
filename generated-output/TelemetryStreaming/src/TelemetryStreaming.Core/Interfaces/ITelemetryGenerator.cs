// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using TelemetryStreaming.Core.Entities;

namespace TelemetryStreaming.Core.Interfaces;

public interface ITelemetryGenerator
{
    Task<TelemetryMessage> GenerateAsync(string source, string metricName, CancellationToken cancellationToken = default);
    Task<IEnumerable<TelemetryMessage>> GenerateBatchAsync(string source, IEnumerable<string> metricNames, CancellationToken cancellationToken = default);
}