// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.StaticAnalysis.Html;

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.Html.BestPractices;

/// <summary>
/// Tests for BP-LONG-LINE rule: Very long lines should be avoided.
/// </summary>
public class LongLineTests : HtmlStaticAnalysisTestBase
{
    [Fact]
    public async Task PageWithVeryLongLine_ReportsViolation()
    {
        // Arrange - Create a line longer than 500 characters
        var longContent = new string('x', 600);
        var html = $@"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <div>{longContent}</div>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertHasIssue(result, "BP-LONG-LINE", HtmlIssueSeverity.Info);
        AssertHasIssueWithCategory(result, "BP-LONG-LINE", "BestPractices");
    }

    [Fact]
    public async Task PageWithNormalLines_NoViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <div>Normal content that is not too long</div>
    <p>Another normal paragraph with reasonable length</p>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "BP-LONG-LINE");
    }

    [Fact]
    public async Task PageWithLineExactly500Chars_NoViolation()
    {
        // Arrange - Line exactly at threshold should not trigger
        var content = new string('x', 480); // Account for the <div></div> tags
        var html = $@"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <div>{content}</div>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "BP-LONG-LINE");
    }

    [Fact]
    public async Task PageWithMultipleLongLines_ReportsOnce()
    {
        // Arrange - Multiple long lines should only report once
        var longContent = new string('x', 600);
        var html = $@"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <div>{longContent}</div>
    <div>{longContent}</div>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert - Should only report once per file (as per implementation)
        Assert.Equal(1, CountIssues(result, "BP-LONG-LINE"));
    }

    [Fact]
    public async Task MinifiedHtml_ReportsViolation()
    {
        // Arrange - Minified HTML often has very long lines
        var content = string.Concat(Enumerable.Repeat("<div>content</div>", 50));
        var html = $"<!DOCTYPE html><html lang=\"en\"><head><title>Test</title></head><body>{content}</body></html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertHasIssue(result, "BP-LONG-LINE", HtmlIssueSeverity.Info);
    }
}
