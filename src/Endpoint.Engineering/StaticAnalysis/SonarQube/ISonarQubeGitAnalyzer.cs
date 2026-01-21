// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Engineering.StaticAnalysis.SonarQube;

/// <summary>
/// Service for performing SonarQube-based static analysis on git changes.
/// </summary>
public interface ISonarQubeGitAnalyzer
{
    /// <summary>
    /// Analyzes code changes between the current branch and a base branch
    /// using SonarQube rules.
    /// </summary>
    /// <param name="directory">The directory to analyze (must be within a git repository).</param>
    /// <param name="baseBranch">The base branch to compare against (default: master).</param>
    /// <param name="rulesFilePath">Optional path to the SonarQube rules markdown file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The analysis result containing all issues found.</returns>
    Task<SonarQubeAnalysisResult> AnalyzeAsync(
        string directory,
        string baseBranch = "master",
        string? rulesFilePath = null,
        CancellationToken cancellationToken = default);
}
