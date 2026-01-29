// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text.RegularExpressions;
using Endpoint.Engineering.Testing.Cli.Models;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.Testing.Cli.Services;

public partial class TypeScriptParser : ITypeScriptParser
{
    private readonly ILogger<TypeScriptParser> _logger;

    public TypeScriptParser(ILogger<TypeScriptParser> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public TypeScriptFileInfo Parse(string filePath, string content)
    {
        var fileName = Path.GetFileName(filePath);
        var fileInfo = new TypeScriptFileInfo
        {
            FilePath = filePath,
            FileName = fileName,
            Content = content,
            FileType = DetermineFileType(fileName, content)
        };

        ParseImports(content, fileInfo);
        ParseClass(content, fileInfo);
        ParseMethods(content, fileInfo);
        ParseProperties(content, fileInfo);
        ParseDecorators(content, fileInfo);

        return fileInfo;
    }

    private static AngularFileType DetermineFileType(string fileName, string content)
    {
        if (fileName.EndsWith(".component.ts", StringComparison.OrdinalIgnoreCase) ||
            content.Contains("@Component"))
        {
            return AngularFileType.Component;
        }

        if (fileName.EndsWith(".service.ts", StringComparison.OrdinalIgnoreCase) ||
            content.Contains("@Injectable"))
        {
            return AngularFileType.Service;
        }

        if (fileName.EndsWith(".directive.ts", StringComparison.OrdinalIgnoreCase) ||
            content.Contains("@Directive"))
        {
            return AngularFileType.Directive;
        }

        if (fileName.EndsWith(".pipe.ts", StringComparison.OrdinalIgnoreCase) ||
            content.Contains("@Pipe"))
        {
            return AngularFileType.Pipe;
        }

        if (fileName.EndsWith(".guard.ts", StringComparison.OrdinalIgnoreCase) ||
            content.Contains("CanActivate") || content.Contains("CanDeactivate"))
        {
            return AngularFileType.Guard;
        }

        if (fileName.EndsWith(".interceptor.ts", StringComparison.OrdinalIgnoreCase) ||
            content.Contains("HttpInterceptor"))
        {
            return AngularFileType.Interceptor;
        }

        if (fileName.EndsWith(".resolver.ts", StringComparison.OrdinalIgnoreCase) ||
            content.Contains("Resolve<"))
        {
            return AngularFileType.Resolver;
        }

        if (content.Contains("interface "))
        {
            return AngularFileType.Interface;
        }

        if (content.Contains("enum "))
        {
            return AngularFileType.Enum;
        }

        if (content.Contains("class "))
        {
            return AngularFileType.Class;
        }

        return AngularFileType.Unknown;
    }

    private void ParseImports(string content, TypeScriptFileInfo fileInfo)
    {
        var importRegex = ImportRegex();
        var matches = importRegex.Matches(content);

        foreach (Match match in matches)
        {
            fileInfo.Imports.Add(match.Value);
        }
    }

    private void ParseClass(string content, TypeScriptFileInfo fileInfo)
    {
        var classRegex = ClassNameRegex();
        var match = classRegex.Match(content);

        if (match.Success)
        {
            fileInfo.ClassName = match.Groups[1].Value;
        }
    }

    private void ParseMethods(string content, TypeScriptFileInfo fileInfo)
    {
        // Match method signatures - handles async, access modifiers, and various patterns
        var methodRegex = MethodRegex();
        var matches = methodRegex.Matches(content);

        foreach (Match match in matches)
        {
            var method = new TypeScriptMethod
            {
                AccessModifier = match.Groups[1].Value.Trim(),
                IsAsync = !string.IsNullOrEmpty(match.Groups[2].Value),
                Name = match.Groups[3].Value,
                ReturnType = string.IsNullOrEmpty(match.Groups[5].Value) ? "void" : match.Groups[5].Value.TrimStart(':').Trim()
            };

            // Parse parameters
            var paramsString = match.Groups[4].Value;
            if (!string.IsNullOrWhiteSpace(paramsString))
            {
                ParseMethodParameters(paramsString, method);
            }

            // Skip constructor, lifecycle hooks we don't need to test directly
            if (method.Name != "constructor" &&
                !method.Name.StartsWith("ng", StringComparison.OrdinalIgnoreCase))
            {
                fileInfo.Methods.Add(method);
            }
        }
    }

    private void ParseMethodParameters(string paramsString, TypeScriptMethod method)
    {
        var paramRegex = ParameterRegex();
        var matches = paramRegex.Matches(paramsString);

        foreach (Match match in matches)
        {
            var param = new TypeScriptParameter
            {
                Name = match.Groups[1].Value.TrimEnd('?'),
                IsOptional = match.Groups[1].Value.EndsWith('?'),
                Type = string.IsNullOrEmpty(match.Groups[2].Value) ? "any" : match.Groups[2].Value.TrimStart(':').Trim()
            };

            if (!string.IsNullOrEmpty(match.Groups[3].Value))
            {
                param.DefaultValue = match.Groups[3].Value.TrimStart('=').Trim();
                param.IsOptional = true;
            }

            method.Parameters.Add(param);
        }
    }

    private void ParseProperties(string content, TypeScriptFileInfo fileInfo)
    {
        var propertyRegex = PropertyRegex();
        var matches = propertyRegex.Matches(content);

        foreach (Match match in matches)
        {
            var property = new TypeScriptProperty
            {
                Decorator = match.Groups[1].Success ? match.Groups[1].Value : null,
                AccessModifier = match.Groups[2].Success ? match.Groups[2].Value : "public",
                IsReadonly = match.Groups[3].Success,
                Name = match.Groups[4].Value.TrimEnd('?', '!'),
                IsOptional = match.Groups[4].Value.EndsWith('?'),
                Type = match.Groups[5].Success ? match.Groups[5].Value.TrimStart(':').Trim() : "any"
            };

            fileInfo.Properties.Add(property);
        }
    }

    private void ParseDecorators(string content, TypeScriptFileInfo fileInfo)
    {
        // Parse @Component decorator
        var componentRegex = ComponentDecoratorRegex();
        var componentMatch = componentRegex.Match(content);

        if (componentMatch.Success)
        {
            var decoratorContent = componentMatch.Groups[1].Value;

            var selectorMatch = SelectorRegex().Match(decoratorContent);
            if (selectorMatch.Success)
            {
                fileInfo.Selector = selectorMatch.Groups[1].Value;
            }

            var templateUrlMatch = TemplateUrlRegex().Match(decoratorContent);
            if (templateUrlMatch.Success)
            {
                fileInfo.TemplateUrl = templateUrlMatch.Groups[1].Value;
            }

            fileInfo.IsStandalone = decoratorContent.Contains("standalone: true");
        }

        // Parse constructor dependencies
        var constructorRegex = ConstructorRegex();
        var constructorMatch = constructorRegex.Match(content);

        if (constructorMatch.Success)
        {
            var paramsContent = constructorMatch.Groups[1].Value;
            var depRegex = DependencyRegex();
            var depMatches = depRegex.Matches(paramsContent);

            foreach (Match depMatch in depMatches)
            {
                var typeName = depMatch.Groups[2].Value.Trim();
                if (!string.IsNullOrEmpty(typeName))
                {
                    fileInfo.Dependencies.Add(typeName);
                }
            }
        }
    }

    [GeneratedRegex(@"import\s+.*?from\s+['""][^'""]+['""];?", RegexOptions.Singleline)]
    private static partial Regex ImportRegex();

    [GeneratedRegex(@"(?:export\s+)?class\s+(\w+)")]
    private static partial Regex ClassNameRegex();

    [GeneratedRegex(@"(public|private|protected)?\s*(async)?\s*(\w+)\s*\(([^)]*)\)\s*(:\s*[\w<>\[\]|&\s]+)?(?:\s*\{)", RegexOptions.Multiline)]
    private static partial Regex MethodRegex();

    [GeneratedRegex(@"(\w+\??)\s*(:\s*[\w<>\[\]|&\s]+)?\s*(=\s*[^,)]+)?")]
    private static partial Regex ParameterRegex();

    [GeneratedRegex(@"(@\w+\([^)]*\))?\s*(public|private|protected)?\s*(readonly)?\s*(\w+[?!]?)\s*(:\s*[\w<>\[\]|&\s]+)?(?:\s*[=;])")]
    private static partial Regex PropertyRegex();

    [GeneratedRegex(@"@Component\s*\(\s*\{([^}]+)\}", RegexOptions.Singleline)]
    private static partial Regex ComponentDecoratorRegex();

    [GeneratedRegex(@"selector:\s*['""]([^'""]+)['""]")]
    private static partial Regex SelectorRegex();

    [GeneratedRegex(@"templateUrl:\s*['""]([^'""]+)['""]")]
    private static partial Regex TemplateUrlRegex();

    [GeneratedRegex(@"constructor\s*\(([^)]*)\)", RegexOptions.Singleline)]
    private static partial Regex ConstructorRegex();

    [GeneratedRegex(@"(?:private|public|protected)?\s*(?:readonly)?\s*(\w+)\s*:\s*([\w<>]+)")]
    private static partial Regex DependencyRegex();
}
