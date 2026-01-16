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
    /// Directory names that indicate test code.
    /// </summary>
    private static readonly HashSet<string> TestDirectoryNames =
    [
        "test", "tests", "spec", "specs", "__tests__", "__test__",
        "unittest", "unittests", "integration", "integrationtests",
        "e2e", "e2e-tests", "test-utils", "testing", "testcases"
    ];

    /// <summary>
    /// Directory name patterns that indicate test projects.
    /// </summary>
    private static readonly string[] TestDirectoryPatterns =
    [
        ".Tests", ".Test", ".Specs", ".Spec",
        ".UnitTests", ".IntegrationTests", ".FunctionalTests",
        "Tests.", "Test.", "Specs.", "Spec."
    ];

    /// <summary>
    /// File name suffixes that indicate test files.
    /// </summary>
    private static readonly string[] TestFileSuffixes =
    [
        ".spec.ts", ".spec.js", ".spec.tsx", ".spec.jsx",
        ".test.ts", ".test.js", ".test.tsx", ".test.jsx",
        ".spec.cs", ".test.cs", "Tests.cs", "Test.cs",
        "_test.py", "_tests.py", "_spec.py",
        "_test.go", "_test.rb", "Spec.rb",
        "Test.java", "Tests.java", "IT.java"
    ];

    /// <summary>
    /// File name patterns that indicate test files.
    /// </summary>
    private static readonly string[] TestFilePatterns =
    [
        "test_", "tests_", "spec_", "specs_",
        "Test", "Tests", "Spec", "Specs"
    ];

    /// <summary>
    /// Import/using patterns that indicate test code.
    /// </summary>
    private static readonly string[] TestImportPatterns =
    [
        // .NET test frameworks
        "using Xunit", "using NUnit", "using Microsoft.VisualStudio.TestTools",
        "using Moq", "using NSubstitute", "using FakeItEasy", "using FluentAssertions",
        "using Shouldly", "using AutoFixture", "using Bogus",
        // JavaScript/TypeScript test frameworks
        "from '@jest'", "from 'jest'", "from '@testing-library'",
        "from 'mocha'", "from 'chai'", "from 'jasmine'",
        "from 'enzyme'", "from '@enzyme'", "from 'sinon'",
        "from 'vitest'", "from '@vitest'", "from 'cypress'",
        "require('jest')", "require('mocha')", "require('chai')",
        "import { test", "import { expect", "import { describe",
        // Python test frameworks
        "import pytest", "import unittest", "from pytest",
        "from unittest", "import mock", "from mock",
        "import hypothesis", "from hypothesis",
        // Go test
        "import \"testing\"",
        // Ruby test frameworks
        "require 'rspec'", "require 'minitest'", "require 'test/unit'",
        // Java test frameworks
        "import org.junit", "import org.testng", "import org.mockito",
        "import org.assertj", "import org.hamcrest"
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

    public async Task<CodeSummary> ParseDirectoryAsync(
        string directory,
        CodeParseOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= CodeParseOptions.Default;

        _logger.LogInformation("Parsing directory: {Directory} with efficiency: {Efficiency}, ignoreTests: {IgnoreTests}",
            directory, options.Efficiency, options.IgnoreTests);

        var summary = new CodeSummary
        {
            RootDirectory = directory,
            Efficiency = options.Efficiency
        };

        // Build gitignore matcher with default patterns and any .gitignore files found
        var gitIgnoreMatcher = new GitIgnoreMatcher(DefaultDotNetGitIgnorePatterns, _logger);
        gitIgnoreMatcher.LoadGitIgnoreFiles(directory);

        var files = GetCodeFiles(directory, directory, gitIgnoreMatcher, options.IgnoreTests, options.TestsOnly);
        summary.TotalFiles = files.Count;

        var skippedFiles = 0;
        var includedFiles = 0;

        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var content = await File.ReadAllTextAsync(file, cancellationToken);
                var relativePath = Path.GetRelativePath(directory, file).Replace('\\', '/');
                var extension = Path.GetExtension(file).ToLowerInvariant();

                var isTestFile = IsTestFileByContent(content);

                // Skip test files when ignoring tests
                if (options.IgnoreTests && isTestFile)
                {
                    skippedFiles++;
                    continue;
                }

                // Skip non-test files when only parsing tests
                if (options.TestsOnly && !isTestFile && !IsTestFileByPath(relativePath))
                {
                    skippedFiles++;
                    continue;
                }

                includedFiles++;
                var fileSummary = ParseFile(content, relativePath, extension);
                summary.Files.Add(fileSummary);

                // Store raw content when efficiency is 0 (verbatim mode)
                if (options.Efficiency == 0)
                {
                    summary.RawContents[relativePath] = content;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse file: {File}", file);
            }
        }

        if (skippedFiles > 0)
        {
            var reason = options.IgnoreTests ? "test" : "non-test";
            _logger.LogInformation("Skipped {SkippedCount} {Reason} files based on content analysis", skippedFiles, reason);
        }

        _logger.LogInformation("Parsed {FileCount} files", summary.Files.Count);
        return summary;
    }

    private List<string> GetCodeFiles(
        string currentDirectory,
        string rootDirectory,
        GitIgnoreMatcher gitIgnoreMatcher,
        bool ignoreTests = false,
        bool testsOnly = false)
    {
        var files = new List<string>();

        try
        {
            // Check if current directory is a test directory
            var currentDirName = Path.GetFileName(currentDirectory);
            var isTestDirectory = IsTestDirectory(currentDirName) ||
                                  IsTestDirectoryByPath(Path.GetRelativePath(rootDirectory, currentDirectory));

            // Skip entire test directories when ignoring tests
            if (ignoreTests && isTestDirectory)
            {
                return files;
            }

            foreach (var file in Directory.GetFiles(currentDirectory))
            {
                var relativePath = Path.GetRelativePath(rootDirectory, file).Replace('\\', '/');

                // Skip if matched by gitignore
                if (gitIgnoreMatcher.IsIgnored(relativePath, isDirectory: false))
                {
                    continue;
                }

                var ext = Path.GetExtension(file).ToLowerInvariant();
                if (!SupportedExtensions.Contains(ext))
                {
                    continue;
                }

                var isTestFile = IsTestFileByPath(relativePath);

                // Skip test files when ignoring tests (path-based check)
                if (ignoreTests && isTestFile)
                {
                    continue;
                }

                // Skip non-test files when only parsing tests (path-based check)
                // Note: Content-based check happens later in ParseDirectoryAsync
                if (testsOnly && !isTestFile && !isTestDirectory)
                {
                    continue;
                }

                files.Add(file);
            }

            foreach (var subDir in Directory.GetDirectories(currentDirectory))
            {
                var relativePath = Path.GetRelativePath(rootDirectory, subDir).Replace('\\', '/') + "/";

                // Skip if matched by gitignore
                if (gitIgnoreMatcher.IsIgnored(relativePath, isDirectory: true))
                {
                    continue;
                }

                files.AddRange(GetCodeFiles(subDir, rootDirectory, gitIgnoreMatcher, ignoreTests, testsOnly));
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error accessing directory: {Directory}", currentDirectory);
        }

        return files;
    }

    /// <summary>
    /// Checks if a directory name indicates a test directory.
    /// </summary>
    private static bool IsTestDirectory(string directoryName)
    {
        var lowerName = directoryName.ToLowerInvariant();
        return TestDirectoryNames.Contains(lowerName);
    }

    /// <summary>
    /// Checks if a directory path indicates a test project/directory.
    /// </summary>
    private static bool IsTestDirectoryByPath(string relativePath)
    {
        var pathParts = relativePath.Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries);

        foreach (var part in pathParts)
        {
            // Check exact matches
            if (TestDirectoryNames.Contains(part.ToLowerInvariant()))
            {
                return true;
            }

            // Check patterns (e.g., "MyProject.Tests", "Tests.Integration")
            foreach (var pattern in TestDirectoryPatterns)
            {
                if (part.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Checks if a file path indicates a test file.
    /// </summary>
    private static bool IsTestFileByPath(string relativePath)
    {
        var fileName = Path.GetFileName(relativePath);

        // Check file suffixes
        foreach (var suffix in TestFileSuffixes)
        {
            if (fileName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        // Check if file is in a test directory
        if (IsTestDirectoryByPath(Path.GetDirectoryName(relativePath) ?? ""))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Checks if file content indicates it's a test file by looking for test framework imports.
    /// </summary>
    private static bool IsTestFileByContent(string content)
    {
        // Check first 2000 characters for test imports (for performance)
        var searchContent = content.Length > 2000 ? content[..2000] : content;

        foreach (var pattern in TestImportPatterns)
        {
            if (searchContent.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        // Check for common test attributes/decorators
        if (searchContent.Contains("[Fact]") ||
            searchContent.Contains("[Theory]") ||
            searchContent.Contains("[Test]") ||
            searchContent.Contains("[TestMethod]") ||
            searchContent.Contains("[TestCase]") ||
            searchContent.Contains("[TestFixture]") ||
            searchContent.Contains("@Test") ||
            searchContent.Contains("@pytest") ||
            searchContent.Contains("def test_") ||
            searchContent.Contains("describe(") ||
            searchContent.Contains("it('") ||
            searchContent.Contains("it(\"") ||
            searchContent.Contains("test('") ||
            searchContent.Contains("test(\""))
        {
            return true;
        }

        return false;
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
