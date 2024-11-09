// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using CommandLine;
using Serilog.Events;

public class EndpointOptions
{
    [Option('l', "log-level")]
    public LogEventLevel LogEventLevel { get; set; } = LogEventLevel.Debug;
}