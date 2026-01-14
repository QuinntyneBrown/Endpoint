// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.StaticAnalysis.Html;
using Microsoft.Extensions.Logging;
using Moq;

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.Html;

/// <summary>
/// Base class for HTML Static Analysis tests providing common test infrastructure.
/// </summary>
public abstract class HtmlStaticAnalysisTestBase : IDisposable
{
    protected readonly string TestDirectory;
    protected readonly IHtmlStaticAnalysisService Service;
    protected readonly Mock<ILogger<HtmlStaticAnalysisService>> LoggerMock;

    protected HtmlStaticAnalysisTestBase()
    {
        TestDirectory = Path.Combine(Path.GetTempPath(), $"HtmlAnalysisTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(TestDirectory);

        LoggerMock = new Mock<ILogger<HtmlStaticAnalysisService>>();
        Service = new HtmlStaticAnalysisService(LoggerMock.Object);
    }

    protected async Task<HtmlStaticAnalysisResult> AnalyzeHtmlAsync(
        string htmlContent,
        string fileName = "test.html",
        HtmlAnalysisOptions? options = null)
    {
        var filePath = Path.Combine(TestDirectory, fileName);
        await File.WriteAllTextAsync(filePath, htmlContent);

        return await Service.AnalyzeDirectoryAsync(TestDirectory, options);
    }

    protected static void AssertHasIssue(
        HtmlStaticAnalysisResult result,
        string ruleId,
        HtmlIssueSeverity? expectedSeverity = null)
    {
        var issues = result.FileResults.SelectMany(f => f.Issues).ToList();
        var matchingIssue = issues.FirstOrDefault(i => i.RuleId == ruleId);

        Assert.NotNull(matchingIssue);

        if (expectedSeverity.HasValue)
        {
            Assert.Equal(expectedSeverity.Value, matchingIssue.Severity);
        }
    }

    protected static void AssertNoIssue(HtmlStaticAnalysisResult result, string ruleId)
    {
        var issues = result.FileResults.SelectMany(f => f.Issues).ToList();
        Assert.DoesNotContain(issues, i => i.RuleId == ruleId);
    }

    protected static void AssertHasIssueWithCategory(
        HtmlStaticAnalysisResult result,
        string ruleId,
        string expectedCategory)
    {
        var issues = result.FileResults.SelectMany(f => f.Issues).ToList();
        var matchingIssue = issues.FirstOrDefault(i => i.RuleId == ruleId);

        Assert.NotNull(matchingIssue);
        Assert.Equal(expectedCategory, matchingIssue.Category);
    }

    protected static int CountIssues(HtmlStaticAnalysisResult result, string ruleId)
    {
        return result.FileResults.SelectMany(f => f.Issues).Count(i => i.RuleId == ruleId);
    }

    public void Dispose()
    {
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

        GC.SuppressFinalize(this);
    }
}
