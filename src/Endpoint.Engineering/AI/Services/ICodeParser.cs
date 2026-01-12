// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Engineering.AI.Services;

/// <summary>
/// Service for parsing code directories and generating token-efficient summaries for LLM consumption.
/// </summary>
public interface ICodeParser
{
    /// <summary>
    /// Parses all code files in a directory and generates a token-efficient summary.
    /// </summary>
    /// <param name="directory">The directory to scan for code files.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A token-efficient summary of the codebase.</returns>
    Task<CodeSummary> ParseDirectoryAsync(string directory, CancellationToken cancellationToken = default);
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
    /// Generates a token-efficient string representation for LLM consumption.
    /// </summary>
    public string ToLlmString()
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
