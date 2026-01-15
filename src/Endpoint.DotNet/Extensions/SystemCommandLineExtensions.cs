// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Reflection;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Endpoint.DotNet.Extensions;

public static class SystemCommandLineExtensions
{
    public static RootCommand BuildRootCommand(this IServiceProvider serviceProvider, string[] args)
    {
        var rootCommand = new RootCommand("Endpoint - A powerful template-based design-time code generator for .NET applications");

        // Add global options
        var logLevelOption = new Option<string>(
            aliases: new[] { "--log-level", "-l" },
            description: "Set the log level (Verbose, Debug, Information, Warning, Error, Fatal)",
            getDefaultValue: () => "Debug");
        rootCommand.AddGlobalOption(logLevelOption);

        // Discover all command request types (those implementing IRequest)
        var commandTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => 
            {
                try
                {
                    return assembly.GetTypes();
                }
                catch
                {
                    return Array.Empty<Type>();
                }
            })
            .Where(type => type.IsClass && !type.IsAbstract)
            .Where(type => typeof(IRequest).IsAssignableFrom(type))
            .ToList();

        foreach (var requestType in commandTypes)
        {
            var command = CreateCommandFromRequestType(requestType, serviceProvider);
            if (command != null)
            {
                rootCommand.AddCommand(command);
            }
        }

        return rootCommand;
    }

    private static Command? CreateCommandFromRequestType(Type requestType, IServiceProvider serviceProvider)
    {
        // Get the command name from the request type name
        // e.g., EntityCreateRequest -> entity-create
        var typeName = requestType.Name;
        if (typeName.EndsWith("Request"))
        {
            typeName = typeName.Substring(0, typeName.Length - "Request".Length);
        }

        // Convert PascalCase to kebab-case
        var commandName = ConvertToKebabCase(typeName);

        var command = new Command(commandName, GetCommandDescription(requestType));

        // Add options based on properties
        var properties = requestType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite && p.CanRead)
            .ToList();
        
        var optionPropertyMap = new Dictionary<Option, PropertyInfo>();

        foreach (var property in properties)
        {
            var option = CreateOptionFromProperty(property);
            if (option != null)
            {
                command.AddOption(option);
                optionPropertyMap[option] = property;
            }
        }

        // Set the handler
        command.SetHandler(async (InvocationContext context) =>
        {
            var mediator = serviceProvider.GetRequiredService<IMediator>();
            var request = Activator.CreateInstance(requestType);

            if (request == null)
            {
                throw new InvalidOperationException($"Failed to create instance of {requestType.Name}");
            }

            // Bind option values to request properties
            foreach (var kvp in optionPropertyMap)
            {
                var option = kvp.Key;
                var property = kvp.Value;
                
                var optionValue = context.ParseResult.GetValueForOption(option);
                
                // Only set non-null values or if there's no default
                if (optionValue != null)
                {
                    property.SetValue(request, optionValue);
                }
            }

            await mediator.Send(request, context.GetCancellationToken());
        });

        return command;
    }

    private static Option? CreateOptionFromProperty(PropertyInfo property)
    {
        var propertyType = property.PropertyType;
        var propertyName = ConvertToKebabCase(property.Name);

        var aliases = new List<string> { $"--{propertyName}" };
        
        // Add short alias based on common patterns
        var shortAlias = GetShortAlias(property.Name);
        if (!string.IsNullOrEmpty(shortAlias))
        {
            aliases.Add($"-{shortAlias}");
        }

        Option? option = null;

        // Create strongly-typed option based on property type
        if (propertyType == typeof(string))
        {
            option = new Option<string>(aliases.ToArray(), GetPropertyDescription(property));
        }
        else if (propertyType == typeof(int))
        {
            option = new Option<int>(aliases.ToArray(), GetPropertyDescription(property));
        }
        else if (propertyType == typeof(int?))
        {
            option = new Option<int?>(aliases.ToArray(), GetPropertyDescription(property));
        }
        else if (propertyType == typeof(bool))
        {
            option = new Option<bool>(aliases.ToArray(), GetPropertyDescription(property));
        }
        else if (propertyType == typeof(bool?))
        {
            option = new Option<bool?>(aliases.ToArray(), GetPropertyDescription(property));
        }
        else
        {
            // For other types, use string and let the handler deal with conversion
            option = new Option<string>(aliases.ToArray(), GetPropertyDescription(property));
        }

        // Set default value if property has one
        if (option != null)
        {
            var defaultValue = GetDefaultValue(property);
            if (defaultValue != null && !IsDefaultForType(defaultValue, propertyType))
            {
                option.SetDefaultValue(defaultValue);
            }
        }

        return option;
    }

    private static string? GetShortAlias(string propertyName)
    {
        var lowerName = propertyName.ToLowerInvariant();
        
        return lowerName switch
        {
            "name" => "n",
            "directory" => "d",
            "properties" => "p",
            "file" => "f",
            "type" => "t",
            "output" => "o",
            "input" => "i",
            _ => null
        };
    }

    private static string ConvertToKebabCase(string pascalCase)
    {
        if (string.IsNullOrEmpty(pascalCase))
            return pascalCase;

        var result = new System.Text.StringBuilder();
        result.Append(char.ToLowerInvariant(pascalCase[0]));

        for (int i = 1; i < pascalCase.Length; i++)
        {
            if (char.IsUpper(pascalCase[i]))
            {
                result.Append('-');
                result.Append(char.ToLowerInvariant(pascalCase[i]));
            }
            else
            {
                result.Append(pascalCase[i]);
            }
        }

        return result.ToString();
    }

    private static string GetCommandDescription(Type type)
    {
        var name = type.Name.Replace("Request", "");
        return $"Execute {ConvertToKebabCase(name)} command";
    }

    private static string GetPropertyDescription(PropertyInfo property)
    {
        return $"{property.Name} option";
    }

    private static object? GetDefaultValue(PropertyInfo property)
    {
        var declaringType = property.DeclaringType;
        if (declaringType != null)
        {
            try
            {
                var instance = Activator.CreateInstance(declaringType);
                return property.GetValue(instance);
            }
            catch
            {
                return null;
            }
        }
        return null;
    }

    private static bool IsDefaultForType(object value, Type type)
    {
        if (type.IsValueType)
        {
            var defaultValue = Activator.CreateInstance(type);
            return Equals(value, defaultValue);
        }
        return value == null;
    }
}
