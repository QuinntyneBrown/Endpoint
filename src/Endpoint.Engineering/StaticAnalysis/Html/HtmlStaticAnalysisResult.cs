// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text;

namespace Endpoint.Engineering.StaticAnalysis.Html;

/// <summary>
/// Results of HTML static analysis for a directory.
/// </summary>
public class HtmlStaticAnalysisResult
{
    /// <summary>
    /// The directory that was analyzed.
    /// </summary>
    public string DirectoryPath { get; set; } = string.Empty;

    /// <summary>
    /// Total number of files analyzed.
    /// </summary>
    public int TotalFilesAnalyzed { get; set; }

    /// <summary>
    /// Analysis results for individual files.
    /// </summary>
    public List<HtmlFileAnalysisResult> FileResults { get; set; } = [];

    /// <summary>
    /// Summary of all issues found.
    /// </summary>
    public HtmlAnalysisSummary Summary { get; set; } = new();

    /// <summary>
    /// Generates a formatted string representation of the analysis results.
    /// </summary>
    public string ToFormattedString()
    {
        var sb = new StringBuilder();

        sb.AppendLine("=".PadRight(60, '='));
        sb.AppendLine("HTML STATIC ANALYSIS RESULTS");
        sb.AppendLine("=".PadRight(60, '='));
        sb.AppendLine();

        sb.AppendLine($"Directory: {DirectoryPath}");
        sb.AppendLine($"Files Analyzed: {TotalFilesAnalyzed}");
        sb.AppendLine();

        // Summary
        sb.AppendLine("-".PadRight(40, '-'));
        sb.AppendLine("SUMMARY");
        sb.AppendLine("-".PadRight(40, '-'));
        sb.AppendLine($"  Errors:   {Summary.ErrorCount}");
        sb.AppendLine($"  Warnings: {Summary.WarningCount}");
        sb.AppendLine($"  Info:     {Summary.InfoCount}");
        sb.AppendLine($"  Total:    {Summary.TotalIssueCount}");
        sb.AppendLine();

        if (Summary.IssuesByCategory.Count > 0)
        {
            sb.AppendLine("Issues by Category:");
            foreach (var category in Summary.IssuesByCategory.OrderByDescending(x => x.Value))
            {
                sb.AppendLine($"  {category.Key}: {category.Value}");
            }
            sb.AppendLine();
        }

        // File details
        if (FileResults.Count > 0)
        {
            sb.AppendLine("-".PadRight(40, '-'));
            sb.AppendLine("FILE DETAILS");
            sb.AppendLine("-".PadRight(40, '-'));

            foreach (var file in FileResults.Where(f => f.Issues.Count > 0))
            {
                sb.AppendLine();
                sb.AppendLine($"File: {file.RelativePath}");
                sb.AppendLine($"  Issues: {file.Issues.Count}");

                foreach (var issue in file.Issues)
                {
                    var locationInfo = issue.LineNumber.HasValue
                        ? $"[Line {issue.LineNumber}] "
                        : "";
                    sb.AppendLine($"    [{issue.Severity}] {locationInfo}{issue.Category}: {issue.Message}");
                    if (!string.IsNullOrEmpty(issue.Suggestion))
                    {
                        sb.AppendLine($"      Suggestion: {issue.Suggestion}");
                    }
                }
            }
        }

        sb.AppendLine();
        sb.AppendLine("=".PadRight(60, '='));

        return sb.ToString();
    }
}

/// <summary>
/// Analysis results for a single HTML file.
/// </summary>
public class HtmlFileAnalysisResult
{
    /// <summary>
    /// Relative path to the file from the analyzed directory.
    /// </summary>
    public string RelativePath { get; set; } = string.Empty;

    /// <summary>
    /// Absolute path to the file.
    /// </summary>
    public string FullPath { get; set; } = string.Empty;

    /// <summary>
    /// List of issues found in this file.
    /// </summary>
    public List<HtmlAnalysisIssue> Issues { get; set; } = [];

    /// <summary>
    /// File metadata extracted during analysis.
    /// </summary>
    public HtmlFileMetadata Metadata { get; set; } = new();
}

/// <summary>
/// Metadata extracted from an HTML file.
/// </summary>
public class HtmlFileMetadata
{
    /// <summary>
    /// Document title if present.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Whether the document has a DOCTYPE declaration.
    /// </summary>
    public bool HasDoctype { get; set; }

    /// <summary>
    /// Document language if specified.
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// Character encoding if specified.
    /// </summary>
    public string? Charset { get; set; }

    /// <summary>
    /// Number of images in the document.
    /// </summary>
    public int ImageCount { get; set; }

    /// <summary>
    /// Number of links in the document.
    /// </summary>
    public int LinkCount { get; set; }

    /// <summary>
    /// Number of forms in the document.
    /// </summary>
    public int FormCount { get; set; }

    /// <summary>
    /// Number of scripts in the document.
    /// </summary>
    public int ScriptCount { get; set; }

    /// <summary>
    /// Number of stylesheets in the document.
    /// </summary>
    public int StylesheetCount { get; set; }
}

/// <summary>
/// An issue found during HTML analysis.
/// </summary>
public class HtmlAnalysisIssue
{
    /// <summary>
    /// Category of the issue (Accessibility, SEO, Security, BestPractices).
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Severity of the issue.
    /// </summary>
    public HtmlIssueSeverity Severity { get; set; }

    /// <summary>
    /// Rule code or identifier.
    /// </summary>
    public string RuleId { get; set; } = string.Empty;

    /// <summary>
    /// Description of the issue.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Line number where the issue was found (if applicable).
    /// </summary>
    public int? LineNumber { get; set; }

    /// <summary>
    /// Column number where the issue was found (if applicable).
    /// </summary>
    public int? ColumnNumber { get; set; }

    /// <summary>
    /// The problematic HTML element or snippet.
    /// </summary>
    public string? Element { get; set; }

    /// <summary>
    /// Suggestion for fixing the issue.
    /// </summary>
    public string? Suggestion { get; set; }
}

/// <summary>
/// Severity level for HTML analysis issues.
/// </summary>
public enum HtmlIssueSeverity
{
    /// <summary>
    /// Informational - not an issue, just a note.
    /// </summary>
    Info,

    /// <summary>
    /// Warning - potential issue that should be reviewed.
    /// </summary>
    Warning,

    /// <summary>
    /// Error - definite issue that should be fixed.
    /// </summary>
    Error
}

/// <summary>
/// Summary statistics for HTML analysis.
/// </summary>
public class HtmlAnalysisSummary
{
    /// <summary>
    /// Total number of issues found.
    /// </summary>
    public int TotalIssueCount { get; set; }

    /// <summary>
    /// Number of error-level issues.
    /// </summary>
    public int ErrorCount { get; set; }

    /// <summary>
    /// Number of warning-level issues.
    /// </summary>
    public int WarningCount { get; set; }

    /// <summary>
    /// Number of info-level issues.
    /// </summary>
    public int InfoCount { get; set; }

    /// <summary>
    /// Issues grouped by category.
    /// </summary>
    public Dictionary<string, int> IssuesByCategory { get; set; } = [];
}
