// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using CommandLine;

namespace Endpoint.DotNet.Services;

/// <summary>
/// Service for displaying CLI help information.
/// </summary>
public static class HelpService
{
    /// <summary>
    /// Displays the complete help screen with all available commands and options.
    /// </summary>
    public static void DisplayHelp()
    {
        var commands = GetAllCommands();

        Console.WriteLine();
        Console.WriteLine("Endpoint Engineering CLI");
        Console.WriteLine("========================");
        Console.WriteLine();
        Console.WriteLine("A code generation tool for .NET, Angular, React, and full-stack development.");
        Console.WriteLine();
        Console.WriteLine("Usage: endpoint <command> [options]");
        Console.WriteLine();
        Console.WriteLine("Global Options:");
        Console.WriteLine("  -l, --log-level    Set the logging level (Verbose, Debug, Information, Warning, Error, Fatal)");
        Console.WriteLine("  --help             Display this help screen");
        Console.WriteLine("  --version          Display version information");
        Console.WriteLine();

        var groupedCommands = GroupCommandsByCategory(commands);

        foreach (var group in groupedCommands.OrderBy(g => g.Key))
        {
            Console.WriteLine($"{group.Key}:");
            Console.WriteLine(new string('-', group.Key.Length + 1));

            foreach (var cmd in group.Value.OrderBy(c => c.VerbName))
            {
                Console.WriteLine($"  {cmd.VerbName,-40} {cmd.Description}");
            }

            Console.WriteLine();
        }

        Console.WriteLine("Examples:");
        Console.WriteLine("  endpoint solution-create -n MyApp");
        Console.WriteLine("  endpoint entity-create -n Customer -p \"Name:string,Email:string\"");
        Console.WriteLine("  endpoint ddd-app-create -n OrderService");
        Console.WriteLine("  endpoint <command> --help              Show help for a specific command");
        Console.WriteLine();
    }

    /// <summary>
    /// Displays detailed help for a specific command.
    /// </summary>
    public static void DisplayCommandHelp(string commandName)
    {
        var commands = GetAllCommands();
        var command = commands.FirstOrDefault(c =>
            c.VerbName.Equals(commandName, StringComparison.OrdinalIgnoreCase));

        if (command == null)
        {
            Console.WriteLine($"Unknown command: {commandName}");
            Console.WriteLine("Use 'endpoint --help' to see all available commands.");
            return;
        }

        Console.WriteLine();
        Console.WriteLine($"Command: {command.VerbName}");
        Console.WriteLine(new string('=', command.VerbName.Length + 9));
        Console.WriteLine();

        if (!string.IsNullOrEmpty(command.Description))
        {
            Console.WriteLine($"Description: {command.Description}");
            Console.WriteLine();
        }

        Console.WriteLine($"Usage: endpoint {command.VerbName} [options]");
        Console.WriteLine();

        if (command.Options.Any())
        {
            Console.WriteLine("Options:");

            foreach (var option in command.Options.OrderBy(o => o.LongName))
            {
                var shortName = !string.IsNullOrEmpty(option.ShortName) ? $"-{option.ShortName}, " : "    ";
                var longName = !string.IsNullOrEmpty(option.LongName) ? $"--{option.LongName}" : "";
                var required = option.Required ? " (Required)" : "";
                var defaultValue = !string.IsNullOrEmpty(option.DefaultValue) ? $" [Default: {option.DefaultValue}]" : "";

                Console.WriteLine($"  {shortName}{longName,-30}{required}{defaultValue}");

                if (!string.IsNullOrEmpty(option.HelpText))
                {
                    Console.WriteLine($"      {option.HelpText}");
                }
            }

            Console.WriteLine();
        }
    }

    private static List<CommandInfo> GetAllCommands()
    {
        var commands = new List<CommandInfo>();

        var verbs = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s =>
            {
                try
                {
                    return s.GetTypes();
                }
                catch
                {
                    return Array.Empty<Type>();
                }
            })
            .Where(type => type.GetCustomAttributes(typeof(VerbAttribute), true).Length > 0)
            .ToList();

        foreach (var verb in verbs)
        {
            var verbAttr = verb.GetCustomAttribute<VerbAttribute>();

            if (verbAttr == null)
            {
                continue;
            }

            var commandInfo = new CommandInfo
            {
                VerbName = verbAttr.Name,
                Description = verbAttr.HelpText ?? GetDescriptionFromVerbName(verbAttr.Name),
                Type = verb,
            };

            var properties = verb.GetProperties();

            foreach (var prop in properties)
            {
                var optionAttr = prop.GetCustomAttribute<OptionAttribute>();

                if (optionAttr != null)
                {
                    var defaultValue = prop.GetValue(Activator.CreateInstance(verb))?.ToString();

                    commandInfo.Options.Add(new OptionInfo
                    {
                        ShortName = optionAttr.ShortName != default ? optionAttr.ShortName.ToString() : null,
                        LongName = optionAttr.LongName,
                        HelpText = optionAttr.HelpText,
                        Required = optionAttr.Required,
                        DefaultValue = defaultValue != null && !defaultValue.Equals(GetTypeDefault(prop.PropertyType)?.ToString())
                            ? defaultValue
                            : null,
                    });
                }
            }

            commands.Add(commandInfo);
        }

        return commands;
    }

    private static object GetTypeDefault(Type type)
    {
        return type.IsValueType ? Activator.CreateInstance(type) : null;
    }

    private static string GetDescriptionFromVerbName(string verbName)
    {
        var parts = verbName.Split('-');
        var action = parts.LastOrDefault()?.ToLower();
        var subject = string.Join(" ", parts.Take(parts.Length - 1));

        return action switch
        {
            "create" => $"Create a new {subject}",
            "add" => $"Add {subject}",
            "remove" => $"Remove {subject}",
            "update" => $"Update {subject}",
            "move" => $"Move {subject}",
            "parse" => $"Parse {subject}",
            "validate" => $"Validate {subject}",
            "nest" => $"Nest {subject}",
            "unnest" => $"Unnest {subject}",
            "reset" => $"Reset {subject}",
            "embed" => $"Embed {subject}",
            "input" => $"{subject} input",
            "rename" => $"Rename {subject}",
            _ => $"{subject} {action}".Trim(),
        };
    }

    private static Dictionary<string, List<CommandInfo>> GroupCommandsByCategory(List<CommandInfo> commands)
    {
        var groups = new Dictionary<string, List<CommandInfo>>();

        foreach (var command in commands)
        {
            var category = GetCategory(command.VerbName);

            if (!groups.ContainsKey(category))
            {
                groups[category] = new List<CommandInfo>();
            }

            groups[category].Add(command);
        }

        return groups;
    }

    private static string GetCategory(string verbName)
    {
        var lowerName = verbName.ToLower();

        if (lowerName.StartsWith("solution") || lowerName.StartsWith("project") || lowerName.StartsWith("ddd-app"))
        {
            return "Solution & Project";
        }

        if (lowerName.StartsWith("entity") || lowerName.StartsWith("aggregate") || lowerName.StartsWith("value-object") || lowerName.StartsWith("value-type"))
        {
            return "Domain Modeling";
        }

        if (lowerName.StartsWith("class") || lowerName.StartsWith("interface") || lowerName.StartsWith("enum") || lowerName.StartsWith("record"))
        {
            return "Code Generation";
        }

        if (lowerName.StartsWith("controller") || lowerName.StartsWith("service") || lowerName.StartsWith("command") || lowerName.StartsWith("query") || lowerName.StartsWith("request") || lowerName.StartsWith("response"))
        {
            return "API & Services";
        }

        if (lowerName.StartsWith("db-context") || lowerName.StartsWith("migration") || lowerName.StartsWith("entity-configuration"))
        {
            return "Database & EF Core";
        }

        if (lowerName.StartsWith("react") || lowerName.StartsWith("lit") || lowerName.StartsWith("ts-") || lowerName.StartsWith("typescript") || lowerName.StartsWith("component"))
        {
            return "Frontend";
        }

        if (lowerName.StartsWith("test") || lowerName.StartsWith("unit-test") || lowerName.StartsWith("spec") || lowerName.StartsWith("playwright") || lowerName.StartsWith("benchmark"))
        {
            return "Testing";
        }

        if (lowerName.StartsWith("signalr") || lowerName.StartsWith("message") || lowerName.StartsWith("microservice") || lowerName.StartsWith("event") || lowerName.StartsWith("service-bus") || lowerName.StartsWith("udp") || lowerName.StartsWith("worker") || lowerName.StartsWith("websocket"))
        {
            return "Messaging & Real-time";
        }

        if (lowerName.StartsWith("git") || lowerName.StartsWith("docker") || lowerName.StartsWith("editor") || lowerName.StartsWith("readme") || lowerName.StartsWith("copyright") || lowerName.StartsWith("usings"))
        {
            return "Configuration & Tools";
        }

        if (lowerName.StartsWith("namespace") || lowerName.StartsWith("file") || lowerName.StartsWith("replace") || lowerName.StartsWith("code-parse") || lowerName.StartsWith("html-parse") || lowerName.StartsWith("package") || lowerName.StartsWith("reference") || lowerName.StartsWith("index"))
        {
            return "Utilities";
        }

        if (lowerName.StartsWith("plantuml") || lowerName.StartsWith("puml") || lowerName.StartsWith("drawio") || lowerName.StartsWith("open-api") || lowerName.StartsWith("http-project"))
        {
            return "Diagrams & API Docs";
        }

        if (lowerName.StartsWith("modern-web-app") || lowerName.StartsWith("building-block") || lowerName.StartsWith("public-api") || lowerName.StartsWith("configure-services") || lowerName.StartsWith("syntax"))
        {
            return "Advanced";
        }

        return "Other";
    }

    private class CommandInfo
    {
        public string VerbName { get; set; }

        public string Description { get; set; }

        public Type Type { get; set; }

        public List<OptionInfo> Options { get; set; } = new List<OptionInfo>();
    }

    private class OptionInfo
    {
        public string ShortName { get; set; }

        public string LongName { get; set; }

        public string HelpText { get; set; }

        public bool Required { get; set; }

        public string DefaultValue { get; set; }
    }
}
