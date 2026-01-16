// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Engineering.AI.Services;

/// <summary>
/// Options for code parsing.
/// </summary>
public class CodeParseOptions
{
    /// <summary>
    /// The level of token efficiency for the output (0-100).
    /// 0 = verbatim (parse all code with full content)
    /// 100 = minimal (most compact summary)
    /// Values in between produce progressively smaller output.
    /// </summary>
    public int Efficiency { get; set; } = 50;

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
    public static CodeParseOptions WithEfficiency(int efficiency) =>
        new() { Efficiency = Math.Clamp(efficiency, 0, 100) };
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
    /// The efficiency level used during parsing (0-100).
    /// 0 = verbatim (full code content)
    /// 100 = minimal (most compact summary)
    /// </summary>
    public int Efficiency { get; set; } = 50;

    /// <summary>
    /// Raw file contents indexed by relative path (populated when Efficiency is low).
    /// </summary>
    public Dictionary<string, string> RawContents { get; set; } = [];

    /// <summary>
    /// Generates a token-efficient string representation for LLM consumption.
    /// The output size decreases as efficiency increases from 0 to 100.
    /// </summary>
    public string ToLlmString()
    {
        // Efficiency ranges:
        // 0: Verbatim - full file contents
        // 1-25: Full detail - all members, all imports, complete structure
        // 26-50: Balanced - limited members, simplified imports
        // 51-75: Compact - type summaries, grouped by directory
        // 76-100: Minimal - file counts, type counts, directory overview

        if (Efficiency == 0)
            return ToLlmStringVerbatim();

        if (Efficiency <= 25)
            return ToLlmStringFullDetail();

        if (Efficiency <= 50)
            return ToLlmStringBalanced();

        if (Efficiency <= 75)
            return ToLlmStringCompact();

        return ToLlmStringMinimal();
    }

    private string ToLlmStringVerbatim()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"# Codebase: {Path.GetFileName(RootDirectory)}");
        sb.AppendLine($"Files: {TotalFiles}");
        sb.AppendLine();

        foreach (var file in Files)
        {
            sb.AppendLine($"## {file.RelativePath}");

            // Include raw content if available
            if (RawContents.TryGetValue(file.RelativePath, out var content))
            {
                sb.AppendLine("```");
                sb.AppendLine(content);
                sb.AppendLine("```");
            }
            else
            {
                // Fallback to structured output
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
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }

    private string ToLlmStringFullDetail()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"# Codebase: {Path.GetFileName(RootDirectory)}");
        sb.AppendLine($"Files: {TotalFiles}");
        sb.AppendLine();

        // Calculate how many members to show based on efficiency (1-25 maps to showing all to ~75%)
        var memberRatio = 1.0 - ((Efficiency - 1) / 24.0 * 0.25);
        var importRatio = 1.0 - ((Efficiency - 1) / 24.0 * 0.25);

        foreach (var file in Files)
        {
            sb.AppendLine($"## {file.RelativePath}");

            if (file.Namespaces.Count > 0)
                sb.AppendLine($"namespace: {string.Join(", ", file.Namespaces)}");

            if (file.Imports.Count > 0)
            {
                var importsToShow = (int)Math.Ceiling(file.Imports.Count * importRatio);
                var imports = file.Imports.Take(importsToShow);
                var remaining = file.Imports.Count - importsToShow;
                sb.AppendLine($"imports: {string.Join(", ", imports)}{(remaining > 0 ? $" (+{remaining})" : "")}");
            }

            foreach (var type in file.Types)
            {
                var modifiers = type.Modifiers.Count > 0 ? $"{string.Join(" ", type.Modifiers)} " : "";
                var baseTypes = type.BaseTypes.Count > 0 ? $" : {string.Join(", ", type.BaseTypes)}" : "";
                sb.AppendLine($"  {modifiers}{type.Kind} {type.Name}{baseTypes}");

                var membersToShow = (int)Math.Ceiling(type.Members.Count * memberRatio);
                var members = type.Members.Take(membersToShow);
                foreach (var member in members)
                {
                    sb.AppendLine($"    {member}");
                }
                var remainingMembers = type.Members.Count - membersToShow;
                if (remainingMembers > 0)
                {
                    sb.AppendLine($"    ...+{remainingMembers} more");
                }
            }

            if (file.Functions.Count > 0)
            {
                sb.AppendLine("  Functions:");
                var funcsToShow = (int)Math.Ceiling(file.Functions.Count * memberRatio);
                var funcs = file.Functions.Take(funcsToShow);
                foreach (var func in funcs)
                {
                    sb.AppendLine($"    {func}");
                }
                var remainingFuncs = file.Functions.Count - funcsToShow;
                if (remainingFuncs > 0)
                {
                    sb.AppendLine($"    ...+{remainingFuncs} more");
                }
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }

    private string ToLlmStringBalanced()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"# Codebase: {Path.GetFileName(RootDirectory)}");
        sb.AppendLine($"Files: {TotalFiles}");
        sb.AppendLine();

        // Calculate limits based on efficiency (26-50 maps to showing less progressively)
        var normalizedEfficiency = (Efficiency - 26) / 24.0; // 0 to 1
        var maxMembers = (int)Math.Max(1, 10 - normalizedEfficiency * 8); // 10 down to 2
        var maxImports = (int)Math.Max(1, 15 - normalizedEfficiency * 12); // 15 down to 3
        var maxFunctions = (int)Math.Max(1, 8 - normalizedEfficiency * 6); // 8 down to 2

        foreach (var file in Files)
        {
            sb.AppendLine($"## {file.RelativePath}");

            if (file.Namespaces.Count > 0)
                sb.AppendLine($"ns: {string.Join(", ", file.Namespaces)}");

            if (file.Imports.Count > 0)
            {
                var imports = file.Imports.Take(maxImports);
                var remaining = file.Imports.Count - maxImports;
                sb.AppendLine($"uses: {string.Join(", ", imports)}{(remaining > 0 ? "..." : "")}");
            }

            foreach (var type in file.Types)
            {
                var modifiers = type.Modifiers.Count > 0 ? $"{string.Join(" ", type.Modifiers)} " : "";
                var baseTypes = type.BaseTypes.Count > 0 ? $" : {string.Join(", ", type.BaseTypes)}" : "";
                sb.AppendLine($"  {modifiers}{type.Kind} {type.Name}{baseTypes}");

                var membersToShow = type.Members.Take(maxMembers).ToList();
                foreach (var member in membersToShow)
                {
                    sb.AppendLine($"    {member}");
                }
                if (type.Members.Count > maxMembers)
                {
                    sb.AppendLine($"    ...+{type.Members.Count - maxMembers} more");
                }
            }

            if (file.Functions.Count > 0)
            {
                var funcsToShow = file.Functions.Take(maxFunctions).ToList();
                foreach (var func in funcsToShow)
                {
                    sb.AppendLine($"  fn: {func}");
                }
                if (file.Functions.Count > maxFunctions)
                {
                    sb.AppendLine($"  ...+{file.Functions.Count - maxFunctions} more");
                }
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }

    private string ToLlmStringCompact()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"# {Path.GetFileName(RootDirectory)} ({TotalFiles} files)");
        sb.AppendLine();

        // Calculate how much detail to show (51-75 maps to progressively less)
        var normalizedEfficiency = (Efficiency - 51) / 24.0; // 0 to 1
        var showTypes = normalizedEfficiency < 0.5; // Show types only in first half
        var maxTypesPerFile = (int)Math.Max(1, 5 - normalizedEfficiency * 4); // 5 down to 1

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

                if (showTypes && file.Types.Count > 0)
                {
                    var types = file.Types.Take(maxTypesPerFile);
                    var typeSummary = $"[{string.Join(",", types.Select(t => $"{t.Kind[0]}:{t.Name}"))}]";
                    var remaining = file.Types.Count - maxTypesPerFile;
                    sb.AppendLine($"  {fileName} {typeSummary}{(remaining > 0 ? $"+{remaining}" : "")}");
                }
                else
                {
                    var counts = new List<string>();
                    if (file.Types.Count > 0) counts.Add($"{file.Types.Count}t");
                    if (file.Functions.Count > 0) counts.Add($"{file.Functions.Count}f");
                    var summary = counts.Count > 0 ? $"({string.Join(",", counts)})" : "";
                    sb.AppendLine($"  {fileName} {summary}");
                }
            }
        }

        return sb.ToString();
    }

    private string ToLlmStringMinimal()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"# {Path.GetFileName(RootDirectory)}");

        // Calculate how minimal to be (76-100 maps to progressively less)
        var normalizedEfficiency = (Efficiency - 76) / 24.0; // 0 to 1
        var showTypeBreakdown = normalizedEfficiency < 0.5;
        var maxDirs = (int)Math.Max(3, 10 - normalizedEfficiency * 7); // 10 down to 3

        // Count by extension
        var byExtension = Files
            .GroupBy(f => f.Extension)
            .OrderByDescending(g => g.Count())
            .Select(g => $"{g.Key}:{g.Count()}");

        sb.AppendLine($"Files: {TotalFiles} ({string.Join(" ", byExtension)})");

        // Count types
        var totalTypes = Files.Sum(f => f.Types.Count);
        if (totalTypes > 0 && showTypeBreakdown)
        {
            var typesByKind = Files
                .SelectMany(f => f.Types)
                .GroupBy(t => t.Kind)
                .OrderByDescending(g => g.Count())
                .Select(g => $"{g.Key}:{g.Count()}");

            sb.AppendLine($"Types: {totalTypes} ({string.Join(" ", typesByKind)})");
        }
        else if (totalTypes > 0)
        {
            sb.AppendLine($"Types: {totalTypes}");
        }

        // List directories with file counts
        var dirs = Files
            .Select(f => Path.GetDirectoryName(f.RelativePath) ?? "(root)")
            .GroupBy(d => d)
            .OrderByDescending(g => g.Count())
            .Take(maxDirs)
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
