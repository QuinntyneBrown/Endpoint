// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.StaticAnalysis;
using Endpoint.Engineering.StaticAnalysis.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.AcceptanceTests;

/// <summary>
/// Base class for static analysis acceptance tests providing common test infrastructure.
/// </summary>
public abstract class StaticAnalysisTestBase : IDisposable
{
    protected readonly StaticAnalysisService Service;
    protected readonly string TestDirectory;
    private bool _disposed;

    protected StaticAnalysisTestBase()
    {
        var loggerMock = new Mock<ILogger<StaticAnalysisService>>();
        Service = new StaticAnalysisService(loggerMock.Object);

        // Create a temporary test directory
        TestDirectory = Path.Combine(Path.GetTempPath(), "StaticAnalysisTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(TestDirectory);

        // Create a .git folder to make it a valid git repository for analysis
        Directory.CreateDirectory(Path.Combine(TestDirectory, ".git"));
    }

    /// <summary>
    /// Creates a C# file with the specified content in the test directory.
    /// </summary>
    protected string CreateCSharpFile(string fileName, string content)
    {
        var filePath = Path.Combine(TestDirectory, fileName);
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(filePath, content);
        return filePath;
    }

    /// <summary>
    /// Creates a C# file with proper copyright header.
    /// </summary>
    protected string CreateCSharpFileWithHeader(string fileName, string content)
    {
        var fullContent = @"// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

" + content;
        return CreateCSharpFile(fileName, fullContent);
    }

    /// <summary>
    /// Runs analysis on the test directory.
    /// </summary>
    protected async Task<AnalysisResult> RunAnalysisAsync()
    {
        return await Service.AnalyzeAsync(TestDirectory, CancellationToken.None);
    }

    /// <summary>
    /// Asserts that a specific rule violation was found.
    /// </summary>
    protected void AssertViolationExists(AnalysisResult result, string ruleId)
    {
        Assert.Contains(result.Violations, v => v.RuleId == ruleId);
    }

    /// <summary>
    /// Asserts that no violation with the specified rule ID exists.
    /// </summary>
    protected void AssertNoViolation(AnalysisResult result, string ruleId)
    {
        Assert.DoesNotContain(result.Violations, v => v.RuleId == ruleId);
    }

    /// <summary>
    /// Asserts that a specific warning was found.
    /// </summary>
    protected void AssertWarningExists(AnalysisResult result, string ruleId)
    {
        Assert.Contains(result.Warnings, w => w.RuleId == ruleId);
    }

    /// <summary>
    /// Asserts that no warning with the specified rule ID exists.
    /// </summary>
    protected void AssertNoWarning(AnalysisResult result, string ruleId)
    {
        Assert.DoesNotContain(result.Warnings, w => w.RuleId == ruleId);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            // Clean up test directory
            try
            {
                if (Directory.Exists(TestDirectory))
                {
                    Directory.Delete(TestDirectory, recursive: true);
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }

        _disposed = true;
    }
}
