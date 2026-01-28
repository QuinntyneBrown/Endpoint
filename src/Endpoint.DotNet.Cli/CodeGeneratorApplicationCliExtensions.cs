// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Endpoint.DotNet.Cli.Extensions;

namespace Endpoint.DotNet.Cli;

/// <summary>
/// CLI-enabled application wrapper for CodeGeneratorApplication.
/// Provides command-line argument parsing using CommandLineParser.
/// </summary>
public static class CodeGeneratorApplicationCliExtensions
{
    /// <summary>
    /// Runs the application with command-line argument parsing.
    /// </summary>
    /// <param name="application">The CodeGeneratorApplication instance.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public static async Task RunCliAsync(this CodeGeneratorApplication application, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(application);
        
        var parsedArgs = Environment.GetCommandLineArgs().ParseArguments();
        await application.RunAsync(parsedArgs, cancellationToken);
    }
}
