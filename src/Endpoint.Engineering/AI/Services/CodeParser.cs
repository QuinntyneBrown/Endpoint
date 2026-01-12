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
        ".cs", ".ts", ".js", ".tsx", ".jsx", ".py", ".java", ".go", ".rs", ".rb"
    ];

    private static readonly HashSet<string> IgnoredDirectories =
    [
        "bin", "obj", "node_modules", ".git", ".vs", ".idea", "dist", "build",
        "packages", ".nuget", "TestResults", "coverage"
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

        var files = GetCodeFiles(directory);
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

    private List<string> GetCodeFiles(string directory)
    {
        var files = new List<string>();

        try
        {
            foreach (var file in Directory.GetFiles(directory))
            {
                var ext = Path.GetExtension(file).ToLowerInvariant();
                if (SupportedExtensions.Contains(ext))
                {
                    files.Add(file);
                }
            }

            foreach (var subDir in Directory.GetDirectories(directory))
            {
                var dirName = Path.GetFileName(subDir);
                if (!IgnoredDirectories.Contains(dirName) && !dirName.StartsWith('.'))
                {
                    files.AddRange(GetCodeFiles(subDir));
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error accessing directory: {Directory}", directory);
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
}
