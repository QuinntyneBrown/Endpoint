// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.StaticAnalysis.Html;

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.Html.BestPractices;

/// <summary>
/// Tests for BP-DEPRECATED rule: Deprecated HTML elements should not be used.
/// </summary>
public class DeprecatedElementTests : HtmlStaticAnalysisTestBase
{
    [Fact]
    public async Task PageWithCenterElement_ReportsViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <center>Centered content</center>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertHasIssue(result, "BP-DEPRECATED", HtmlIssueSeverity.Warning);
        AssertHasIssueWithCategory(result, "BP-DEPRECATED", "BestPractices");
    }

    [Fact]
    public async Task PageWithFontElement_ReportsViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <font color=""red"">Red text</font>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertHasIssue(result, "BP-DEPRECATED", HtmlIssueSeverity.Warning);
    }

    [Fact]
    public async Task PageWithMarqueeElement_ReportsViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <marquee>Scrolling text</marquee>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertHasIssue(result, "BP-DEPRECATED", HtmlIssueSeverity.Warning);
    }

    [Fact]
    public async Task PageWithStrikeElement_ReportsViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <strike>Strikethrough text</strike>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertHasIssue(result, "BP-DEPRECATED", HtmlIssueSeverity.Warning);
    }

    [Fact]
    public async Task PageWithBigElement_ReportsViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <big>Big text</big>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertHasIssue(result, "BP-DEPRECATED", HtmlIssueSeverity.Warning);
    }

    [Fact]
    public async Task PageWithTtElement_ReportsViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <tt>Teletype text</tt>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertHasIssue(result, "BP-DEPRECATED", HtmlIssueSeverity.Warning);
    }

    [Fact]
    public async Task PageWithModernElements_NoViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <div style=""text-align: center"">Centered content</div>
    <span style=""color: red"">Red text</span>
    <del>Deleted text</del>
    <code>Code text</code>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "BP-DEPRECATED");
    }

    [Fact]
    public async Task PageWithMultipleDeprecatedElements_ReportsMultipleViolations()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <center>Centered</center>
    <font color=""red"">Red</font>
    <big>Big</big>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        Assert.Equal(3, CountIssues(result, "BP-DEPRECATED"));
    }
}
