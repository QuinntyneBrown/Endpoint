// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using TelemetryStreaming.Core.Entities;
using TelemetryStreaming.Core.Interfaces;

namespace TelemetryStreaming.Infrastructure.Services;

public class TelemetryGenerator : ITelemetryGenerator
{
    private readonly Random random = new Random();

    public Task<TelemetryMessage> GenerateAsync(string source, string metricName, CancellationToken cancellationToken = default)
    {
        var message = new TelemetryMessage
        {
            TelemetryMessageId = Guid.NewGuid(),
            Source = source,
            MetricName = metricName,
            Value = random.Next(0, 100).ToString(),
            Unit = "units",
            Type = TelemetryType.Metric,
            Timestamp = DateTime.UtcNow,
            Tags = new Dictionary<string, string>
            {
                { "generated", "true" },
                { "version", "1.0" }
            }
        };

        return Task.FromResult(message);
    }

    public async Task<IEnumerable<TelemetryMessage>> GenerateBatchAsync(string source, IEnumerable<string> metricNames, CancellationToken cancellationToken = default)
    {
        var messages = new List<TelemetryMessage>();
        foreach (var metricName in metricNames)
        {
            messages.Add(await GenerateAsync(source, metricName, cancellationToken));
        }
        return messages;
    }
}