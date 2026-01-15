// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using CommandLine;
using Endpoint.DotNet.Services;

namespace Endpoint.DotNet.Extensions;

public static class CommandLineArgsExtensions
{
    public static object ParseArguments(this string[] args)
    {
        var lastArg = args.FirstOrDefault() ?? string.Empty;

        if (lastArg.EndsWith("dll"))
        {
            args = args.Skip(1).ToArray();
        }

        // Check for global --help or -h flag (no command specified)
        if (args.Length == 0 ||
            (args.Length == 1 && (args[0] == "--help" || args[0] == "-h" || args[0] == "-?" || args[0] == "help")))
        {
            HelpService.DisplayHelp();
            Environment.Exit(0);
        }

        // Check for version flags
        if (args.Length == 1 && (args[0] == "--version" || args[0] == "-v"))
        {
            var assembly = typeof(CommandLineArgsExtensions).Assembly;
            var version = assembly.GetName().Version;
            var informationalVersion = assembly
                .GetCustomAttribute<System.Reflection.AssemblyInformationalVersionAttribute>()?
                .InformationalVersion ?? version?.ToString() ?? "Unknown";
            
            Console.WriteLine($"Endpoint.Engineering.Cli {informationalVersion}");
            Console.WriteLine("Copyright (C) 2026 Quinntyne Brown");
            Environment.Exit(0);
        }

        // Check for help on a specific command: endpoint <command> --help
        if (args.Length == 2 && (args[1] == "--help" || args[1] == "-h" || args[1] == "-?"))
        {
            HelpService.DisplayCommandHelp(args[0]);
            Environment.Exit(0);
        }

        // Check for help command with argument: endpoint help <command>
        if (args.Length == 2 && args[0] == "help")
        {
            HelpService.DisplayCommandHelp(args[1]);
            Environment.Exit(0);
        }

        if (args.Length == 0)
        {
            args =
                [Environment.GetEnvironmentVariable("ENDPOINT_DEFAULT", EnvironmentVariableTarget.Machine)!];
        }

        var parser = new Parser(with =>
        {
            with.CaseSensitive = false;
            with.HelpWriter = Console.Out;
            with.IgnoreUnknownArguments = true;
        });

        if (args.Length == 0 || args[0].StartsWith('-'))
        {
            args = new string[1] { "default" }.Concat(args).ToArray();
        }

        var verbs = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(type => type.GetCustomAttributes(typeof(VerbAttribute), true).Length > 0)
            .ToArray();

        var parsedResult = parser.ParseArguments(args, verbs);

        if (parsedResult.Errors.SingleOrDefault() is HelpRequestedError || parsedResult.Errors.SingleOrDefault() is HelpVerbRequestedError)
        {
            HelpService.DisplayHelp();
            Environment.Exit(0);
        }

        if (parsedResult.Errors.SingleOrDefault() is BadVerbSelectedError error)
        {
            throw new Exception($"{error.Tag}:{error.Token}");
        }

        return parsedResult.Value;
    }
}
