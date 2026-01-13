// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace TelemetryStreaming.Core.Events;

public record TelemetryDataPublishedEvent(string Source, string MetricName, string Value, DateTime Timestamp);