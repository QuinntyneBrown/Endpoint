// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Engineering.AI.Services;

/// <summary>
/// Specifies the level of token efficiency for code parsing output.
/// </summary>
public enum CodeParseEfficiency
{
    /// <summary>
    /// Low efficiency - maximum detail. Include all members, full type info, all imports.
    /// Best for small codebases (under 100 files).
    /// </summary>
    Low = 1,

    /// <summary>
    /// Medium efficiency - balanced detail. Include types with limited members, simplified imports.
    /// Best for medium codebases (100-500 files).
    /// </summary>
    Medium = 2,

    /// <summary>
    /// High efficiency - minimal tokens. Only type names, counts, and structure overview.
    /// Best for large codebases (500+ files).
    /// </summary>
    High = 3,

    /// <summary>
    /// Maximum efficiency - ultra-compact. Only file counts and top-level structure.
    /// Best for very large codebases or initial exploration.
    /// </summary>
    Max = 4
}

/// <summary>
/// Options for code parsing.
/// </summary>
public class CodeParseOptions
{
    /// <summary>
    /// The level of token efficiency for the output.
    /// </summary>
    public CodeParseEfficiency Efficiency { get; set; } = CodeParseEfficiency.Medium;

    /// <summary>
    /// When true, ignores test files and test projects.
    /// </summary>
    public bool IgnoreTests { get; set; } = false;

    /// <summary>
    /// When true, only parses test files and test projects (opposite of IgnoreTests).
    /// </summary>
    public bool TestsOnly { get; set; } = false;

    /// <summary>
    /// Creates default options.
    /// </summary>
    public static CodeParseOptions Default => new();

    /// <summary>
    /// Creates options with specified efficiency.
    /// </summary>
    public static CodeParseOptions WithEfficiency(CodeParseEfficiency efficiency) =>
        new() { Efficiency = efficiency };
}

/// <summary>
/// Service for parsing code directories and generating token-efficient summaries for LLM consumption.
/// </summary>
public interface ICodeParser
{
    /// <summary>
    /// Parses all code files in a directory and generates a token-efficient summary.
    /// </summary>
    /// <param name="directory">The directory to scan for code files.</param>
    /// <param name="options">Parsing options including efficiency level and test filtering.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A token-efficient summary of the codebase.</returns>
    Task<CodeSummary> ParseDirectoryAsync(
        string directory,
        CodeParseOptions? options = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a token-efficient summary of a codebase.
/// </summary>
public class CodeSummary
{
    /// <summary>
    /// The root directory that was parsed.
    /// </summary>
    public string RootDirectory { get; set; } = string.Empty;

    /// <summary>
    /// Summary of files organized by type/extension.
    /// </summary>
    public List<FileSummary> Files { get; set; } = [];

    /// <summary>
    /// Total number of files processed.
    /// </summary>
    public int TotalFiles { get; set; }

    /// <summary>
    /// The efficiency level used during parsing.
    /// </summary>
    public CodeParseEfficiency Efficiency { get; set; } = CodeParseEfficiency.Medium;

    /// <summary>
    /// Generates a token-efficient string representation for LLM consumption.
    /// </summary>
    public string ToLlmString()
    {
        return Efficiency switch
        {
            CodeParseEfficiency.Low => ToLlmStringLow(),
            CodeParseEfficiency.Medium => ToLlmStringMedium(),
            CodeParseEfficiency.High => ToLlmStringHigh(),
            CodeParseEfficiency.Max => ToLlmStringMax(),
            _ => ToLlmStringMedium()
        };
    }

    private string ToLlmStringLow()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"# Codebase: {Path.GetFileName(RootDirectory)}");
        sb.AppendLine($"Files: {TotalFiles}");
        sb.AppendLine();

        foreach (var file in Files)
        {
            sb.AppendLine($"## {file.RelativePath}");

            if (file.Namespaces.Count > 0)
                sb.AppendLine($"namespace: {string.Join(", ", file.Namespaces)}");

            if (file.Imports.Count > 0)
                sb.AppendLine($"imports: {string.Join(", ", file.Imports)}");

            foreach (var type in file.Types)
            {
                var modifiers = type.Modifiers.Count > 0 ? $"{string.Join(" ", type.Modifiers)} " : "";
                var baseTypes = type.BaseTypes.Count > 0 ? $" : {string.Join(", ", type.BaseTypes)}" : "";
                sb.AppendLine($"  {modifiers}{type.Kind} {type.Name}{baseTypes}");

                foreach (var member in type.Members)
                {
                    sb.AppendLine($"    {member}");
                }
            }

            if (file.Functions.Count > 0)
            {
                sb.AppendLine("  Functions:");
                foreach (var func in file.Functions)
                {
                    sb.AppendLine($"    {func}");
                }
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }

    private string ToLlmStringMedium()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"# Codebase: {Path.GetFileName(RootDirectory)}");
        sb.AppendLine($"Files: {TotalFiles}");
        sb.AppendLine();

        foreach (var file in Files)
        {
            sb.AppendLine($"## {file.RelativePath}");

            if (file.Namespaces.Count > 0)
                sb.AppendLine($"ns: {string.Join(", ", file.Namespaces)}");

            if (file.Imports.Count > 0)
                sb.AppendLine($"uses: {string.Join(", ", file.Imports.Take(10))}{(file.Imports.Count > 10 ? "..." : "")}");

            foreach (var type in file.Types)
            {
                var modifiers = type.Modifiers.Count > 0 ? $"{string.Join(" ", type.Modifiers)} " : "";
                var baseTypes = type.BaseTypes.Count > 0 ? $" : {string.Join(", ", type.BaseTypes)}" : "";
                sb.AppendLine($"  {modifiers}{type.Kind} {type.Name}{baseTypes}");

                // Limit members in medium efficiency
                var membersToShow = type.Members.Take(5).ToList();
                foreach (var member in membersToShow)
                {
                    sb.AppendLine($"    {member}");
                }
                if (type.Members.Count > 5)
                {
                    sb.AppendLine($"    ...+{type.Members.Count - 5} more");
                }
            }

            if (file.Functions.Count > 0)
            {
                var funcsToShow = file.Functions.Take(5).ToList();
                foreach (var func in funcsToShow)
                {
                    sb.AppendLine($"  fn: {func}");
                }
                if (file.Functions.Count > 5)
                {
                    sb.AppendLine($"  ...+{file.Functions.Count - 5} more");
                }
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }

    private string ToLlmStringHigh()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"# {Path.GetFileName(RootDirectory)} ({TotalFiles} files)");
        sb.AppendLine();

        // Group files by directory
        var filesByDir = Files
            .GroupBy(f => Path.GetDirectoryName(f.RelativePath) ?? "")
            .OrderBy(g => g.Key);

        foreach (var dirGroup in filesByDir)
        {
            var dirName = string.IsNullOrEmpty(dirGroup.Key) ? "(root)" : dirGroup.Key;
            sb.AppendLine($"## {dirName}/");

            foreach (var file in dirGroup)
            {
                var fileName = Path.GetFileName(file.RelativePath);
                var typeCount = file.Types.Count;
                var funcCount = file.Functions.Count;

                var typeSummary = typeCount > 0
                    ? $"[{string.Join(",", file.Types.Select(t => $"{t.Kind[0]}:{t.Name}"))}]"
                    : "";

                sb.AppendLine($"  {fileName} {typeSummary}");
            }
        }

        return sb.ToString();
    }

    private string ToLlmStringMax()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"# {Path.GetFileName(RootDirectory)}");

        // Count by extension
        var byExtension = Files
            .GroupBy(f => f.Extension)
            .OrderByDescending(g => g.Count())
            .Select(g => $"{g.Key}:{g.Count()}");

        sb.AppendLine($"Files: {TotalFiles} ({string.Join(" ", byExtension)})");

        // Count types
        var totalTypes = Files.Sum(f => f.Types.Count);
        var typesByKind = Files
            .SelectMany(f => f.Types)
            .GroupBy(t => t.Kind)
            .OrderByDescending(g => g.Count())
            .Select(g => $"{g.Key}:{g.Count()}");

        if (totalTypes > 0)
            sb.AppendLine($"Types: {totalTypes} ({string.Join(" ", typesByKind)})");

        // List directories with file counts
        var dirs = Files
            .Select(f => Path.GetDirectoryName(f.RelativePath) ?? "(root)")
            .GroupBy(d => d)
            .OrderByDescending(g => g.Count())
            .Take(10)
            .Select(g => $"{g.Key}({g.Count()})");

        sb.AppendLine($"Dirs: {string.Join(" ", dirs)}");

        return sb.ToString();
    }
}

/// <summary>
/// Summary of a single file.
/// </summary>
public class FileSummary
{
    public string RelativePath { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public List<string> Namespaces { get; set; } = [];
    public List<string> Imports { get; set; } = [];
    public List<TypeSummary> Types { get; set; } = [];
    public List<string> Functions { get; set; } = [];
}

/// <summary>
/// Summary of a type (class, interface, struct, enum, record).
/// </summary>
public class TypeSummary
{
    public string Name { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty; // class, interface, struct, enum, record
    public List<string> Modifiers { get; set; } = []; // public, abstract, static, etc.
    public List<string> BaseTypes { get; set; } = []; // inherited classes and interfaces
    public List<string> Members { get; set; } = []; // simplified member signatures
}
