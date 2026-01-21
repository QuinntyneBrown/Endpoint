// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Engineering.StaticAnalysis.SonarQube;

/// <summary>
/// Represents a SonarQube rule loaded from the rules markdown file.
/// </summary>
public class SonarQubeRule
{
    /// <summary>
    /// Gets or sets the rule ID (e.g., "S1234").
    /// </summary>
    public string RuleId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the rule title/description.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the language this rule applies to (csharp, typescript).
    /// </summary>
    public string Language { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the category (Vulnerability, Bug, Security Hotspot, Code Smell).
    /// </summary>
    public string Category { get; set; } = string.Empty;
}

/// <summary>
/// Represents a file that was changed in the git diff.
/// </summary>
public class ChangedFile
{
    /// <summary>
    /// Gets or sets the relative path to the file.
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the full path to the file.
    /// </summary>
    public string FullPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the change status (Added, Modified, Deleted, etc.).
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the language of the file (csharp, typescript).
    /// </summary>
    public string Language { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the set of line numbers that were added or modified.
    /// </summary>
    public HashSet<int> AddedLines { get; set; } = new();
}

/// <summary>
/// Represents an issue found during SonarQube analysis.
/// </summary>
public class SonarQubeIssue
{
    /// <summary>
    /// Gets or sets the relative path to the file.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the line number where the issue was found.
    /// </summary>
    public int LineNumber { get; set; }

    /// <summary>
    /// Gets or sets the content of the line.
    /// </summary>
    public string LineContent { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the rule ID.
    /// </summary>
    public string RuleId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the issue message/description.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the category (Vulnerability, Bug, Security Hotspot, Code Smell).
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the language (csharp, typescript).
    /// </summary>
    public string Language { get; set; } = string.Empty;
}

/// <summary>
/// Represents the result of a SonarQube analysis.
/// </summary>
public class SonarQubeAnalysisResult
{
    /// <summary>
    /// Gets or sets the current branch name.
    /// </summary>
    public string CurrentBranch { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the base branch name that was compared against.
    /// </summary>
    public string BaseBranch { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the root directory of the repository.
    /// </summary>
    public string RootDirectory { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of changed files.
    /// </summary>
    public List<ChangedFile> ChangedFiles { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of issues found.
    /// </summary>
    public List<SonarQubeIssue> Issues { get; set; } = new();

    /// <summary>
    /// Gets or sets the timestamp when the analysis was performed.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets the number of rules loaded.
    /// </summary>
    public int RulesLoaded { get; set; }
}
