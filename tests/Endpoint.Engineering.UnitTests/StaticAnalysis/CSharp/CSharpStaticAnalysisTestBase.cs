// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Extensions.Logging;
using Moq;

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.CSharp;

/// <summary>
/// Base class for C# static analysis acceptance tests.
/// Provides helper methods for creating test files and running analysis.
/// </summary>
public abstract class CSharpStaticAnalysisTestBase : IDisposable
{
    protected readonly ICSharpStaticAnalysisService _service;
    protected readonly string _testDirectory;
    private bool _disposed;

    protected CSharpStaticAnalysisTestBase()
    {
        var loggerMock = new Mock<ILogger<CSharpStaticAnalysisService>>();
        _service = new CSharpStaticAnalysisService(loggerMock.Object);
        _testDirectory = Path.Combine(Path.GetTempPath(), $"StaticAnalysisTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testDirectory);
    }

    /// <summary>
    /// Creates a test C# file with the specified content.
    /// </summary>
    protected string CreateTestFile(string fileName, string content)
    {
        var filePath = Path.Combine(_testDirectory, fileName);
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        File.WriteAllText(filePath, content);
        return filePath;
    }

    /// <summary>
    /// Runs static analysis on the test directory with specified options.
    /// </summary>
    protected async Task<CSharpStaticAnalysisResult> AnalyzeAsync(CSharpStaticAnalysisOptions? options = null)
    {
        return await _service.AnalyzeAsync(_testDirectory, options);
    }

    /// <summary>
    /// Runs static analysis for a specific category only.
    /// </summary>
    protected async Task<CSharpStaticAnalysisResult> AnalyzeCategoryAsync(IssueCategory category)
    {
        var options = new CSharpStaticAnalysisOptions
        {
            Categories = new HashSet<IssueCategory> { category }
        };
        return await _service.AnalyzeAsync(_testDirectory, options);
    }

    /// <summary>
    /// Asserts that the result contains at least one issue with the specified rule ID.
    /// </summary>
    protected static void AssertHasIssue(CSharpStaticAnalysisResult result, string ruleId)
    {
        Assert.True(
            result.Issues.Any(i => i.RuleId == ruleId),
            $"Expected issue with rule ID '{ruleId}' but found: [{string.Join(", ", result.Issues.Select(i => i.RuleId))}]");
    }

    /// <summary>
    /// Asserts that the result contains at least one issue in the specified category.
    /// </summary>
    protected static void AssertHasCategory(CSharpStaticAnalysisResult result, IssueCategory category)
    {
        Assert.True(
            result.Issues.Any(i => i.Category == category),
            $"Expected issue in category '{category}' but found: [{string.Join(", ", result.Issues.Select(i => i.Category).Distinct())}]");
    }

    /// <summary>
    /// Asserts that the result contains at least one issue with the specified severity.
    /// </summary>
    protected static void AssertHasSeverity(CSharpStaticAnalysisResult result, IssueSeverity severity)
    {
        Assert.True(
            result.Issues.Any(i => i.Severity == severity),
            $"Expected issue with severity '{severity}' but found: [{string.Join(", ", result.Issues.Select(i => i.Severity).Distinct())}]");
    }

    /// <summary>
    /// Asserts that the result contains no issues.
    /// </summary>
    protected static void AssertNoIssues(CSharpStaticAnalysisResult result)
    {
        Assert.Empty(result.Issues);
    }

    /// <summary>
    /// Asserts that the result contains an issue matching the predicate.
    /// </summary>
    protected static void AssertHasIssueMatching(CSharpStaticAnalysisResult result, Func<StaticAnalysisIssue, bool> predicate, string description)
    {
        Assert.True(
            result.Issues.Any(predicate),
            $"Expected issue matching: {description}. Found issues: [{string.Join(", ", result.Issues.Select(i => $"{i.RuleId}:{i.Message}"))}]");
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Clean up test directory
                if (Directory.Exists(_testDirectory))
                {
                    try
                    {
                        Directory.Delete(_testDirectory, recursive: true);
                    }
                    catch
                    {
                        // Ignore cleanup errors in tests
                    }
                }
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
