// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.AI.Services;

/// <summary>
/// Parses code directories and generates token-efficient summaries for LLM consumption.
/// </summary>
public partial class CodeParser : ICodeParser
{
    private readonly ILogger<CodeParser> _logger;

    private static readonly HashSet<string> SupportedExtensions =
    [
        ".cs", ".ts", ".js", ".tsx", ".jsx", ".py", ".java", ".go", ".rs", ".rb",
        ".html", ".scss", ".css"
    ];

    /// <summary>
    /// Default .NET gitignore patterns based on the standard Visual Studio .gitignore template.
    /// </summary>
    private static readonly string[] DefaultDotNetGitIgnorePatterns =
    [
        // Build results
        "[Dd]ebug/",
        "[Dd]ebugPublic/",
        "[Rr]elease/",
        "[Rr]eleases/",
        "x64/",
        "x86/",
        "[Ww][Ii][Nn]32/",
        "[Aa][Rr][Mm]/",
        "[Aa][Rr][Mm]64/",
        "bld/",
        "[Bb]in/",
        "[Oo]bj/",
        "[Ll]og/",
        "[Ll]ogs/",

        // Visual Studio
        ".vs/",
        "*.user",
        "*.rsuser",
        "*.suo",
        "*.cache",
        "*.userosscache",
        "*.sln.docstates",

        // NuGet
        "**/[Pp]ackages/*",
        "*.nupkg",
        "*.snupkg",
        ".nuget/",

        // Build
        "project.lock.json",
        "project.fragment.lock.json",
        "artifacts/",

        // Test Results
        "[Tt]est[Rr]esult*/",
        "[Bb]uild[Ll]og.*",
        "*.trx",
        "*.coverage",
        "*.coveragexml",
        "coverage*/",

        // NCrunch
        "_NCrunch_*",
        "*.ncrunch*",

        // IDE
        ".idea/",
        "*.resharper*",

        // Node
        "node_modules/",

        // Misc
        "*.log",
        "*.tmp",
        "*.temp",
        ".git/",
        ".gitattributes",
        ".gitignore",
        "*.orig",

        // OS
        ".DS_Store",
        "Thumbs.db",

        // Common output
        "dist/",
        "build/",
        "out/",
        "output/"
    ];

    public CodeParser(ILogger<CodeParser> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    public async Task<CodeSummary> ParseDirectoryAsync(string directory, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Parsing directory: {Directory}", directory);

        var summary = new CodeSummary
        {
            RootDirectory = directory
        };

        // Build gitignore matcher with default patterns and any .gitignore files found
        var gitIgnoreMatcher = new GitIgnoreMatcher(DefaultDotNetGitIgnorePatterns, _logger);
        gitIgnoreMatcher.LoadGitIgnoreFiles(directory);

        var files = GetCodeFiles(directory, directory, gitIgnoreMatcher);
        summary.TotalFiles = files.Count;

        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var content = await File.ReadAllTextAsync(file, cancellationToken);
                var relativePath = Path.GetRelativePath(directory, file);
                var extension = Path.GetExtension(file).ToLowerInvariant();

                var fileSummary = ParseFile(content, relativePath, extension);
                summary.Files.Add(fileSummary);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse file: {File}", file);
            }
        }

        _logger.LogInformation("Parsed {FileCount} files", summary.TotalFiles);
        return summary;
    }

    private List<string> GetCodeFiles(string currentDirectory, string rootDirectory, GitIgnoreMatcher gitIgnoreMatcher)
    {
        var files = new List<string>();

        try
        {
            foreach (var file in Directory.GetFiles(currentDirectory))
            {
                var relativePath = Path.GetRelativePath(rootDirectory, file).Replace('\\', '/');

                // Skip if matched by gitignore
                if (gitIgnoreMatcher.IsIgnored(relativePath, isDirectory: false))
                {
                    continue;
                }

                var ext = Path.GetExtension(file).ToLowerInvariant();
                if (SupportedExtensions.Contains(ext))
                {
                    files.Add(file);
                }
            }

            foreach (var subDir in Directory.GetDirectories(currentDirectory))
            {
                var relativePath = Path.GetRelativePath(rootDirectory, subDir).Replace('\\', '/') + "/";

                // Skip if matched by gitignore
                if (gitIgnoreMatcher.IsIgnored(relativePath, isDirectory: true))
                {
                    continue;
                }

                files.AddRange(GetCodeFiles(subDir, rootDirectory, gitIgnoreMatcher));
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error accessing directory: {Directory}", currentDirectory);
        }

        return files;
    }

    private FileSummary ParseFile(string content, string relativePath, string extension)
    {
        var summary = new FileSummary
        {
            RelativePath = relativePath.Replace('\\', '/'),
            Extension = extension
        };

        switch (extension)
        {
            case ".cs":
                ParseCSharpFile(content, summary);
                break;
            case ".ts":
            case ".tsx":
                ParseTypeScriptFile(content, summary);
                break;
            case ".js":
            case ".jsx":
                ParseJavaScriptFile(content, summary);
                break;
            case ".py":
                ParsePythonFile(content, summary);
                break;
            case ".java":
                ParseJavaFile(content, summary);
                break;
            case ".go":
                ParseGoFile(content, summary);
                break;
            case ".html":
                ParseHtmlFile(content, summary);
                break;
            case ".scss":
            case ".css":
                ParseScssFile(content, summary);
                break;
            default:
                ParseGenericFile(content, summary);
                break;
        }

        return summary;
    }

    private void ParseCSharpFile(string content, FileSummary summary)
    {
        // Extract namespace
        var nsMatches = NamespaceRegex().Matches(content);
        foreach (Match match in nsMatches)
        {
            summary.Namespaces.Add(match.Groups[1].Value);
        }

        // Extract file-scoped namespace
        var fileScopedNs = FileScopedNamespaceRegex().Match(content);
        if (fileScopedNs.Success)
        {
            summary.Namespaces.Add(fileScopedNs.Groups[1].Value);
        }

        // Extract usings (simplified - just namespace names)
        var usingMatches = UsingRegex().Matches(content);
        foreach (Match match in usingMatches)
        {
            var ns = match.Groups[1].Value;
            // Only include top-level namespace
            var topLevel = ns.Split('.')[0];
            if (!summary.Imports.Contains(topLevel))
            {
                summary.Imports.Add(topLevel);
            }
        }

        // Extract types (classes, interfaces, structs, records, enums)
        var typeMatches = CSharpTypeRegex().Matches(content);
        foreach (Match match in typeMatches)
        {
            var typeSummary = new TypeSummary
            {
                Name = match.Groups[3].Value,
                Kind = match.Groups[2].Value
            };

            // Parse modifiers
            var modifiers = match.Groups[1].Value.Trim();
            if (!string.IsNullOrEmpty(modifiers))
            {
                typeSummary.Modifiers.AddRange(modifiers.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            }

            // Parse base types
            if (match.Groups[4].Success && !string.IsNullOrWhiteSpace(match.Groups[4].Value))
            {
                var baseTypes = match.Groups[4].Value.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var bt in baseTypes)
                {
                    typeSummary.BaseTypes.Add(bt.Trim().Split('<')[0]); // Remove generic params for brevity
                }
            }

            // Extract type body and parse members
            var bodyStart = content.IndexOf('{', match.Index);
            if (bodyStart >= 0)
            {
                var body = ExtractBody(content, bodyStart);
                ParseCSharpMembers(body, typeSummary);
            }

            summary.Types.Add(typeSummary);
        }
    }

    private void ParseCSharpMembers(string body, TypeSummary typeSummary)
    {
        // Extract methods (simplified signature)
        var methodMatches = CSharpMethodRegex().Matches(body);
        foreach (Match match in methodMatches)
        {
            var returnType = SimplifyType(match.Groups[2].Value);
            var methodName = match.Groups[3].Value;
            var parameters = SimplifyParameters(match.Groups[4].Value);
            typeSummary.Members.Add($"{returnType} {methodName}({parameters})");
        }

        // Extract properties (simplified)
        var propMatches = CSharpPropertyRegex().Matches(body);
        foreach (Match match in propMatches)
        {
            var propType = SimplifyType(match.Groups[2].Value);
            var propName = match.Groups[3].Value;
            typeSummary.Members.Add($"{propType} {propName} {{...}}");
        }
    }

    private void ParseTypeScriptFile(string content, FileSummary summary)
    {
        // Extract imports
        var importMatches = TsImportRegex().Matches(content);
        foreach (Match match in importMatches)
        {
            var from = match.Groups[1].Value;
            if (!from.StartsWith('.'))
            {
                var topLevel = from.Split('/')[0];
                if (!summary.Imports.Contains(topLevel))
                {
                    summary.Imports.Add(topLevel);
                }
            }
        }

        // Extract classes
        var classMatches = TsClassRegex().Matches(content);
        foreach (Match match in classMatches)
        {
            var typeSummary = new TypeSummary
            {
                Name = match.Groups[2].Value,
                Kind = "class"
            };

            if (!string.IsNullOrEmpty(match.Groups[1].Value))
                typeSummary.Modifiers.Add("export");

            if (match.Groups[3].Success && !string.IsNullOrWhiteSpace(match.Groups[3].Value))
            {
                typeSummary.BaseTypes.Add(match.Groups[3].Value.Trim());
            }

            summary.Types.Add(typeSummary);
        }

        // Extract interfaces
        var ifaceMatches = TsInterfaceRegex().Matches(content);
        foreach (Match match in ifaceMatches)
        {
            var typeSummary = new TypeSummary
            {
                Name = match.Groups[2].Value,
                Kind = "interface"
            };

            if (!string.IsNullOrEmpty(match.Groups[1].Value))
                typeSummary.Modifiers.Add("export");

            summary.Types.Add(typeSummary);
        }

        // Extract exported functions
        var funcMatches = TsFunctionRegex().Matches(content);
        foreach (Match match in funcMatches)
        {
            summary.Functions.Add($"{match.Groups[2].Value}({SimplifyParameters(match.Groups[3].Value)})");
        }
    }

    private void ParseJavaScriptFile(string content, FileSummary summary)
    {
        // Extract requires/imports
        var requireMatches = JsRequireRegex().Matches(content);
        foreach (Match match in requireMatches)
        {
            var module = match.Groups[1].Value;
            if (!module.StartsWith('.'))
            {
                var topLevel = module.Split('/')[0];
                if (!summary.Imports.Contains(topLevel))
                {
                    summary.Imports.Add(topLevel);
                }
            }
        }

        // Extract ES imports
        var importMatches = TsImportRegex().Matches(content);
        foreach (Match match in importMatches)
        {
            var from = match.Groups[1].Value;
            if (!from.StartsWith('.'))
            {
                var topLevel = from.Split('/')[0];
                if (!summary.Imports.Contains(topLevel))
                {
                    summary.Imports.Add(topLevel);
                }
            }
        }

        // Extract classes
        var classMatches = JsClassRegex().Matches(content);
        foreach (Match match in classMatches)
        {
            var typeSummary = new TypeSummary
            {
                Name = match.Groups[1].Value,
                Kind = "class"
            };

            if (match.Groups[2].Success && !string.IsNullOrWhiteSpace(match.Groups[2].Value))
            {
                typeSummary.BaseTypes.Add(match.Groups[2].Value.Trim());
            }

            summary.Types.Add(typeSummary);
        }

        // Extract functions
        var funcMatches = JsFunctionRegex().Matches(content);
        foreach (Match match in funcMatches)
        {
            summary.Functions.Add($"{match.Groups[1].Value}()");
        }
    }

    private void ParsePythonFile(string content, FileSummary summary)
    {
        // Extract imports
        var importMatches = PyImportRegex().Matches(content);
        foreach (Match match in importMatches)
        {
            var module = match.Groups[1].Value.Split('.')[0];
            if (!summary.Imports.Contains(module))
            {
                summary.Imports.Add(module);
            }
        }

        var fromImportMatches = PyFromImportRegex().Matches(content);
        foreach (Match match in fromImportMatches)
        {
            var module = match.Groups[1].Value.Split('.')[0];
            if (!summary.Imports.Contains(module))
            {
                summary.Imports.Add(module);
            }
        }

        // Extract classes
        var classMatches = PyClassRegex().Matches(content);
        foreach (Match match in classMatches)
        {
            var typeSummary = new TypeSummary
            {
                Name = match.Groups[1].Value,
                Kind = "class"
            };

            if (match.Groups[2].Success && !string.IsNullOrWhiteSpace(match.Groups[2].Value))
            {
                var bases = match.Groups[2].Value.Trim('(', ')').Split(',');
                foreach (var b in bases)
                {
                    var baseName = b.Trim();
                    if (!string.IsNullOrEmpty(baseName))
                        typeSummary.BaseTypes.Add(baseName);
                }
            }

            summary.Types.Add(typeSummary);
        }

        // Extract functions
        var funcMatches = PyFunctionRegex().Matches(content);
        foreach (Match match in funcMatches)
        {
            var funcName = match.Groups[1].Value;
            if (!funcName.StartsWith('_')) // Skip private functions
            {
                summary.Functions.Add($"{funcName}()");
            }
        }
    }

    private void ParseJavaFile(string content, FileSummary summary)
    {
        // Extract package
        var packageMatch = JavaPackageRegex().Match(content);
        if (packageMatch.Success)
        {
            summary.Namespaces.Add(packageMatch.Groups[1].Value);
        }

        // Extract imports
        var importMatches = JavaImportRegex().Matches(content);
        foreach (Match match in importMatches)
        {
            var package = match.Groups[1].Value.Split('.')[0];
            if (!summary.Imports.Contains(package))
            {
                summary.Imports.Add(package);
            }
        }

        // Extract classes
        var classMatches = JavaClassRegex().Matches(content);
        foreach (Match match in classMatches)
        {
            var typeSummary = new TypeSummary
            {
                Name = match.Groups[3].Value,
                Kind = match.Groups[2].Value
            };

            var modifiers = match.Groups[1].Value.Trim();
            if (!string.IsNullOrEmpty(modifiers))
            {
                typeSummary.Modifiers.AddRange(modifiers.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            }

            summary.Types.Add(typeSummary);
        }
    }

    private void ParseGoFile(string content, FileSummary summary)
    {
        // Extract package
        var packageMatch = GoPackageRegex().Match(content);
        if (packageMatch.Success)
        {
            summary.Namespaces.Add(packageMatch.Groups[1].Value);
        }

        // Extract imports
        var importMatches = GoImportRegex().Matches(content);
        foreach (Match match in importMatches)
        {
            var pkg = match.Groups[1].Value.Trim('"').Split('/').Last();
            if (!summary.Imports.Contains(pkg))
            {
                summary.Imports.Add(pkg);
            }
        }

        // Extract structs
        var structMatches = GoStructRegex().Matches(content);
        foreach (Match match in structMatches)
        {
            var typeSummary = new TypeSummary
            {
                Name = match.Groups[1].Value,
                Kind = "struct"
            };
            summary.Types.Add(typeSummary);
        }

        // Extract interfaces
        var ifaceMatches = GoInterfaceRegex().Matches(content);
        foreach (Match match in ifaceMatches)
        {
            var typeSummary = new TypeSummary
            {
                Name = match.Groups[1].Value,
                Kind = "interface"
            };
            summary.Types.Add(typeSummary);
        }

        // Extract functions
        var funcMatches = GoFunctionRegex().Matches(content);
        foreach (Match match in funcMatches)
        {
            var funcName = match.Groups[1].Value;
            if (char.IsUpper(funcName[0])) // Only exported functions
            {
                summary.Functions.Add($"{funcName}()");
            }
        }
    }

    private void ParseHtmlFile(string content, FileSummary summary)
    {
        // Extract Angular/Vue component selectors
        var componentMatches = HtmlComponentRegex().Matches(content);
        foreach (Match match in componentMatches)
        {
            var tagName = match.Groups[1].Value;
            // Only include custom components (contain hyphen or start with app-)
            if (tagName.Contains('-') || tagName.StartsWith("app", StringComparison.OrdinalIgnoreCase))
            {
                if (!summary.Imports.Contains(tagName))
                {
                    summary.Imports.Add(tagName);
                }
            }
        }

        // Extract form elements with names/ids
        var formElementMatches = HtmlFormElementRegex().Matches(content);
        foreach (Match match in formElementMatches)
        {
            var elementType = match.Groups[1].Value;
            var identifier = match.Groups[2].Value;
            summary.Functions.Add($"{elementType}#{identifier}");
        }

        // Extract Angular bindings
        var bindingMatches = AngularBindingRegex().Matches(content);
        var bindings = new HashSet<string>();
        foreach (Match match in bindingMatches)
        {
            bindings.Add(match.Groups[1].Value);
        }

        if (bindings.Count > 0)
        {
            var typeSummary = new TypeSummary
            {
                Name = "Bindings",
                Kind = "template"
            };
            typeSummary.Members.AddRange(bindings.Take(10));
            if (bindings.Count > 10)
            {
                typeSummary.Members.Add($"...+{bindings.Count - 10} more");
            }
            summary.Types.Add(typeSummary);
        }
    }

    private void ParseScssFile(string content, FileSummary summary)
    {
        // Extract SCSS imports
        var importMatches = ScssImportRegex().Matches(content);
        foreach (Match match in importMatches)
        {
            var importPath = match.Groups[1].Value.Trim('\'', '"');
            var fileName = Path.GetFileNameWithoutExtension(importPath).TrimStart('_');
            if (!summary.Imports.Contains(fileName))
            {
                summary.Imports.Add(fileName);
            }
        }

        // Extract SCSS variables
        var variableMatches = ScssVariableRegex().Matches(content);
        var variables = new List<string>();
        foreach (Match match in variableMatches)
        {
            variables.Add(match.Groups[1].Value);
        }

        if (variables.Count > 0)
        {
            var typeSummary = new TypeSummary
            {
                Name = "Variables",
                Kind = "scss"
            };
            typeSummary.Members.AddRange(variables.Take(10));
            if (variables.Count > 10)
            {
                typeSummary.Members.Add($"...+{variables.Count - 10} more");
            }
            summary.Types.Add(typeSummary);
        }

        // Extract SCSS mixins
        var mixinMatches = ScssMixinRegex().Matches(content);
        foreach (Match match in mixinMatches)
        {
            summary.Functions.Add($"@mixin {match.Groups[1].Value}");
        }

        // Extract top-level selectors (classes and ids)
        var selectorMatches = ScssSelectorRegex().Matches(content);
        var selectors = new HashSet<string>();
        foreach (Match match in selectorMatches)
        {
            selectors.Add(match.Groups[1].Value);
        }

        if (selectors.Count > 0)
        {
            var typeSummary = new TypeSummary
            {
                Name = "Selectors",
                Kind = "css"
            };
            typeSummary.Members.AddRange(selectors.Take(15));
            if (selectors.Count > 15)
            {
                typeSummary.Members.Add($"...+{selectors.Count - 15} more");
            }
            summary.Types.Add(typeSummary);
        }
    }

    private void ParseGenericFile(string content, FileSummary summary)
    {
        // Basic parsing for unsupported languages
        var lines = content.Split('\n').Length;
        summary.Functions.Add($"({lines} lines)");
    }

    private static string ExtractBody(string content, int startBrace)
    {
        var depth = 0;
        var start = startBrace;

        for (var i = startBrace; i < content.Length; i++)
        {
            if (content[i] == '{')
                depth++;
            else if (content[i] == '}')
            {
                depth--;
                if (depth == 0)
                {
                    return content.Substring(start + 1, i - start - 1);
                }
            }
        }

        return content.Substring(start + 1);
    }

    private static string SimplifyType(string type)
    {
        // Remove common prefixes and simplify generic types
        type = type.Trim();
        type = type.Replace("System.", "");
        type = type.Replace("Collections.Generic.", "");
        type = type.Replace("Threading.Tasks.", "");

        // Simplify common types
        type = type switch
        {
            "String" => "string",
            "Int32" => "int",
            "Int64" => "long",
            "Boolean" => "bool",
            "Void" or "void" => "void",
            _ => type
        };

        return type;
    }

    private static string SimplifyParameters(string parameters)
    {
        if (string.IsNullOrWhiteSpace(parameters))
            return "";

        var parts = parameters.Split(',');
        var simplified = new List<string>();

        foreach (var part in parts)
        {
            var trimmed = part.Trim();
            if (string.IsNullOrEmpty(trimmed))
                continue;

            // Just get the type name, not the parameter name
            var tokens = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length > 0)
            {
                var typeName = SimplifyType(tokens[0]);
                simplified.Add(typeName);
            }
        }

        return string.Join(", ", simplified);
    }

    // C# Regex patterns
    [GeneratedRegex(@"namespace\s+([\w.]+)\s*\{", RegexOptions.Multiline)]
    private static partial Regex NamespaceRegex();

    [GeneratedRegex(@"namespace\s+([\w.]+)\s*;", RegexOptions.Multiline)]
    private static partial Regex FileScopedNamespaceRegex();

    [GeneratedRegex(@"using\s+(?:static\s+)?(?![\w.]+\s*=)([\w.]+)\s*;", RegexOptions.Multiline)]
    private static partial Regex UsingRegex();

    [GeneratedRegex(@"((?:public|private|protected|internal|static|abstract|sealed|partial)\s+)*\s*(class|interface|struct|record|enum)\s+(\w+)(?:<[^>]+>)?(?:\s*:\s*([^{]+))?", RegexOptions.Multiline)]
    private static partial Regex CSharpTypeRegex();

    [GeneratedRegex(@"((?:public|private|protected|internal|static|virtual|override|async|abstract)\s+)+(\w+(?:<[^>]+>)?)\s+(\w+)\s*\(([^)]*)\)", RegexOptions.Multiline)]
    private static partial Regex CSharpMethodRegex();

    [GeneratedRegex(@"((?:public|private|protected|internal|static)\s+)+(\w+(?:<[^>]+>)?(?:\?)?)\s+(\w+)\s*\{", RegexOptions.Multiline)]
    private static partial Regex CSharpPropertyRegex();

    // TypeScript Regex patterns
    [GeneratedRegex(@"import\s+.*?\s+from\s+['""]([^'""]+)['""]", RegexOptions.Multiline)]
    private static partial Regex TsImportRegex();

    [GeneratedRegex(@"(export\s+)?class\s+(\w+)(?:\s+extends\s+(\w+))?", RegexOptions.Multiline)]
    private static partial Regex TsClassRegex();

    [GeneratedRegex(@"(export\s+)?interface\s+(\w+)", RegexOptions.Multiline)]
    private static partial Regex TsInterfaceRegex();

    [GeneratedRegex(@"(export\s+)?(?:async\s+)?function\s+(\w+)\s*\(([^)]*)\)", RegexOptions.Multiline)]
    private static partial Regex TsFunctionRegex();

    // JavaScript Regex patterns
    [GeneratedRegex(@"require\s*\(\s*['""]([^'""]+)['""]\s*\)", RegexOptions.Multiline)]
    private static partial Regex JsRequireRegex();

    [GeneratedRegex(@"class\s+(\w+)(?:\s+extends\s+(\w+))?", RegexOptions.Multiline)]
    private static partial Regex JsClassRegex();

    [GeneratedRegex(@"(?:function|const|let|var)\s+(\w+)\s*=?\s*(?:function\s*)?\(", RegexOptions.Multiline)]
    private static partial Regex JsFunctionRegex();

    // Python Regex patterns
    [GeneratedRegex(@"^import\s+([\w.]+)", RegexOptions.Multiline)]
    private static partial Regex PyImportRegex();

    [GeneratedRegex(@"^from\s+([\w.]+)\s+import", RegexOptions.Multiline)]
    private static partial Regex PyFromImportRegex();

    [GeneratedRegex(@"^class\s+(\w+)(\([^)]*\))?:", RegexOptions.Multiline)]
    private static partial Regex PyClassRegex();

    [GeneratedRegex(@"^def\s+(\w+)\s*\(", RegexOptions.Multiline)]
    private static partial Regex PyFunctionRegex();

    // Java Regex patterns
    [GeneratedRegex(@"package\s+([\w.]+)\s*;", RegexOptions.Multiline)]
    private static partial Regex JavaPackageRegex();

    [GeneratedRegex(@"import\s+([\w.]+)\s*;", RegexOptions.Multiline)]
    private static partial Regex JavaImportRegex();

    [GeneratedRegex(@"((?:public|private|protected|static|abstract|final)\s+)*(class|interface)\s+(\w+)", RegexOptions.Multiline)]
    private static partial Regex JavaClassRegex();

    // Go Regex patterns
    [GeneratedRegex(@"^package\s+(\w+)", RegexOptions.Multiline)]
    private static partial Regex GoPackageRegex();

    [GeneratedRegex(@"import\s+(?:\(\s*)?(""[^""]+"")", RegexOptions.Multiline)]
    private static partial Regex GoImportRegex();

    [GeneratedRegex(@"type\s+(\w+)\s+struct\s*\{", RegexOptions.Multiline)]
    private static partial Regex GoStructRegex();

    [GeneratedRegex(@"type\s+(\w+)\s+interface\s*\{", RegexOptions.Multiline)]
    private static partial Regex GoInterfaceRegex();

    [GeneratedRegex(@"^func\s+(?:\([^)]+\)\s+)?(\w+)\s*\(", RegexOptions.Multiline)]
    private static partial Regex GoFunctionRegex();

    // HTML Regex patterns
    [GeneratedRegex(@"<([a-zA-Z][\w-]*)[^>]*>", RegexOptions.Multiline)]
    private static partial Regex HtmlComponentRegex();

    [GeneratedRegex(@"<(input|select|textarea|button|form)[^>]*(?:id|name)=[""']([^""']+)[""']", RegexOptions.IgnoreCase | RegexOptions.Multiline)]
    private static partial Regex HtmlFormElementRegex();

    [GeneratedRegex(@"\[\(?\w+\)?\]=""([^""]+)""|\{\{([^}]+)\}\}", RegexOptions.Multiline)]
    private static partial Regex AngularBindingRegex();

    // SCSS/CSS Regex patterns
    [GeneratedRegex(@"@import\s+['""]?([^'"";\s]+)['""]?\s*;", RegexOptions.Multiline)]
    private static partial Regex ScssImportRegex();

    [GeneratedRegex(@"^\s*(\$[\w-]+)\s*:", RegexOptions.Multiline)]
    private static partial Regex ScssVariableRegex();

    [GeneratedRegex(@"@mixin\s+([\w-]+)", RegexOptions.Multiline)]
    private static partial Regex ScssMixinRegex();

    [GeneratedRegex(@"^([.#][\w-]+)\s*\{", RegexOptions.Multiline)]
    private static partial Regex ScssSelectorRegex();
}

/// <summary>
/// Matches file paths against gitignore patterns.
/// </summary>
internal class GitIgnoreMatcher
{
    private readonly List<GitIgnorePattern> _patterns = [];
    private readonly ILogger _logger;

    public GitIgnoreMatcher(string[] defaultPatterns, ILogger logger)
    {
        _logger = logger;

        foreach (var pattern in defaultPatterns)
        {
            AddPattern(pattern, isFromFile: false);
        }
    }

    public void LoadGitIgnoreFiles(string rootDirectory)
    {
        LoadGitIgnoreFilesRecursive(rootDirectory, rootDirectory);
    }

    private void LoadGitIgnoreFilesRecursive(string currentDirectory, string rootDirectory)
    {
        var gitIgnorePath = Path.Combine(currentDirectory, ".gitignore");

        if (File.Exists(gitIgnorePath))
        {
            try
            {
                var relativeDirPath = Path.GetRelativePath(rootDirectory, currentDirectory).Replace('\\', '/');
                var prefix = relativeDirPath == "." ? "" : relativeDirPath + "/";

                var lines = File.ReadAllLines(gitIgnorePath);
                foreach (var line in lines)
                {
                    var trimmed = line.Trim();

                    // Skip empty lines and comments
                    if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith('#'))
                        continue;

                    AddPattern(trimmed, isFromFile: true, prefix: prefix);
                }

                _logger.LogDebug("Loaded .gitignore from: {Path}", gitIgnorePath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read .gitignore: {Path}", gitIgnorePath);
            }
        }

        // Recursively check subdirectories for .gitignore files
        try
        {
            foreach (var subDir in Directory.GetDirectories(currentDirectory))
            {
                var dirName = Path.GetFileName(subDir);
                // Don't descend into .git directory
                if (dirName != ".git")
                {
                    LoadGitIgnoreFilesRecursive(subDir, rootDirectory);
                }
            }
        }
        catch
        {
            // Ignore directory access errors
        }
    }

    private void AddPattern(string pattern, bool isFromFile, string prefix = "")
    {
        var isNegation = pattern.StartsWith('!');
        if (isNegation)
        {
            pattern = pattern[1..];
        }

        // Handle patterns that start with /
        var isRooted = pattern.StartsWith('/');
        if (isRooted)
        {
            pattern = pattern[1..];
        }

        // Add prefix for patterns from nested .gitignore files
        if (!string.IsNullOrEmpty(prefix) && !isRooted)
        {
            // Non-rooted patterns in nested .gitignore still apply from that directory
        }
        else if (!string.IsNullOrEmpty(prefix) && isRooted)
        {
            pattern = prefix + pattern;
        }

        var gitPattern = new GitIgnorePattern
        {
            OriginalPattern = pattern,
            IsNegation = isNegation,
            IsDirectoryOnly = pattern.EndsWith('/'),
            IsRooted = isRooted || pattern.Contains('/'),
            Regex = ConvertToRegex(pattern)
        };

        _patterns.Add(gitPattern);
    }

    public bool IsIgnored(string relativePath, bool isDirectory)
    {
        var normalizedPath = relativePath.Replace('\\', '/');
        if (isDirectory && !normalizedPath.EndsWith('/'))
        {
            normalizedPath += "/";
        }

        var isIgnored = false;

        foreach (var pattern in _patterns)
        {
            // Directory-only patterns don't match files
            if (pattern.IsDirectoryOnly && !isDirectory)
                continue;

            var matches = pattern.Regex.IsMatch(normalizedPath);

            if (!matches && !pattern.IsRooted)
            {
                // For non-rooted patterns, also try matching against the filename only
                var fileName = Path.GetFileName(relativePath.TrimEnd('/'));
                if (isDirectory)
                    fileName += "/";
                matches = pattern.Regex.IsMatch(fileName);
            }

            if (matches)
            {
                isIgnored = !pattern.IsNegation;
            }
        }

        return isIgnored;
    }

    private static Regex ConvertToRegex(string pattern)
    {
        // Remove trailing slash for processing
        var isDir = pattern.EndsWith('/');
        if (isDir)
        {
            pattern = pattern[..^1];
        }

        var regexPattern = "^";

        for (var i = 0; i < pattern.Length; i++)
        {
            var c = pattern[i];

            switch (c)
            {
                case '*':
                    if (i + 1 < pattern.Length && pattern[i + 1] == '*')
                    {
                        // ** matches everything including /
                        if (i + 2 < pattern.Length && pattern[i + 2] == '/')
                        {
                            regexPattern += "(.*/)?";
                            i += 2;
                        }
                        else
                        {
                            regexPattern += ".*";
                            i++;
                        }
                    }
                    else
                    {
                        // * matches everything except /
                        regexPattern += "[^/]*";
                    }
                    break;

                case '?':
                    regexPattern += "[^/]";
                    break;

                case '[':
                    // Character class - find the closing bracket
                    var end = pattern.IndexOf(']', i + 1);
                    if (end > i)
                    {
                        regexPattern += pattern[i..(end + 1)];
                        i = end;
                    }
                    else
                    {
                        regexPattern += @"\[";
                    }
                    break;

                case '.':
                case '(':
                case ')':
                case '+':
                case '|':
                case '^':
                case '$':
                case '@':
                case '%':
                case '{':
                case '}':
                case '\\':
                    regexPattern += "\\" + c;
                    break;

                default:
                    regexPattern += c;
                    break;
            }
        }

        if (isDir)
        {
            regexPattern += "/";
        }

        regexPattern += "$";

        return new Regex(regexPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
    }

    private class GitIgnorePattern
    {
        public string OriginalPattern { get; init; } = string.Empty;
        public bool IsNegation { get; init; }
        public bool IsDirectoryOnly { get; init; }
        public bool IsRooted { get; init; }
        public Regex Regex { get; init; } = null!;
    }
}
