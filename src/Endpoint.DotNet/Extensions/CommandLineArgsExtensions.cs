// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.CommandLine;
using System.CommandLine.Parsing;
using System.Linq;
using MediatR;

namespace Endpoint.DotNet.Extensions;

public static class CommandLineArgsExtensions
{
    public static async Task<int> ParseAndExecuteAsync(this string[] args, IServiceProvider serviceProvider)
    {
        var lastArg = args.FirstOrDefault() ?? string.Empty;

        if (lastArg.EndsWith("dll"))
        {
            args = args.Skip(1).ToArray();
        }

        var rootCommand = serviceProvider.BuildRootCommand(args);
        
        return await rootCommand.InvokeAsync(args);
    }
}
