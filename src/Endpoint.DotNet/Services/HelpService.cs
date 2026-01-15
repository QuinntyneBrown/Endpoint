// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;

namespace Endpoint.DotNet.Services;

/// <summary>
/// Service for displaying CLI help information.
/// NOTE: This service is deprecated and no longer used with System.CommandLine.
/// System.CommandLine provides built-in help functionality.
/// </summary>
[Obsolete("This service is deprecated. System.CommandLine provides built-in help functionality.")]
public static class HelpService
{
    /// <summary>
    /// Displays the complete help screen with all available commands and options.
    /// This method is deprecated and no longer functional.
    /// </summary>
    [Obsolete("Use System.CommandLine's built-in help functionality instead.")]
    public static void DisplayHelp()
    {
        Console.WriteLine("Help is now handled by System.CommandLine. Use --help or -h to see available commands.");
    }

    /// <summary>
    /// Displays detailed help for a specific command.
    /// This method is deprecated and no longer functional.
    /// </summary>
    [Obsolete("Use System.CommandLine's built-in help functionality instead.")]
    public static void DisplayCommandHelp(string commandName)
    {
        Console.WriteLine($"Help for command '{commandName}' is now handled by System.CommandLine. Use: endpoint {commandName} --help");
    }
}
