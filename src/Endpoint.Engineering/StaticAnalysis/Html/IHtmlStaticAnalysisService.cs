// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Engineering.StaticAnalysis.Html;

/// <summary>
/// Service for performing static analysis on HTML files in a directory.
/// </summary>
public interface IHtmlStaticAnalysisService
{
    /// <summary>
    /// Analyzes HTML files in the specified directory and returns analysis results.
    /// </summary>
    /// <param name="directoryPath">The directory path to analyze.</param>
    /// <param name="options">Analysis options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>HTML static analysis results.</returns>
    Task<HtmlStaticAnalysisResult> AnalyzeDirectoryAsync(
        string directoryPath,
        HtmlAnalysisOptions? options = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Options for HTML static analysis.
/// </summary>
public class HtmlAnalysisOptions
{
    /// <summary>
    /// Whether to include subdirectories in the analysis.
    /// </summary>
    public bool Recursive { get; set; } = true;

    /// <summary>
    /// File extensions to analyze (defaults to .html, .htm).
    /// </summary>
    public string[] Extensions { get; set; } = [".html", ".htm"];

    /// <summary>
    /// Whether to check for accessibility issues (WCAG compliance).
    /// </summary>
    public bool CheckAccessibility { get; set; } = true;

    /// <summary>
    /// Whether to check for SEO issues.
    /// </summary>
    public bool CheckSeo { get; set; } = true;

    /// <summary>
    /// Whether to check for security issues.
    /// </summary>
    public bool CheckSecurity { get; set; } = true;

    /// <summary>
    /// Whether to check for best practices.
    /// </summary>
    public bool CheckBestPractices { get; set; } = true;

    /// <summary>
    /// Creates default options.
    /// </summary>
    public static HtmlAnalysisOptions Default => new();
}
