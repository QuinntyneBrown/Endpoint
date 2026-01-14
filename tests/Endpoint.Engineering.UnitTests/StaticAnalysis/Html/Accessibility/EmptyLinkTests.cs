// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.StaticAnalysis.Html;

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.Html.Accessibility;

/// <summary>
/// Tests for A11Y-EMPTY-LINK rule: Links must have text content.
/// </summary>
public class EmptyLinkTests : HtmlStaticAnalysisTestBase
{
    [Fact]
    public async Task EmptyLink_ReportsViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <a href=""https://example.com""></a>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertHasIssue(result, "A11Y-EMPTY-LINK", HtmlIssueSeverity.Warning);
        AssertHasIssueWithCategory(result, "A11Y-EMPTY-LINK", "Accessibility");
    }

    [Fact]
    public async Task LinkWithText_NoViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <a href=""https://example.com"">Visit Example</a>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "A11Y-EMPTY-LINK");
    }

    [Fact]
    public async Task LinkWithWhitespaceOnly_ReportsViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <a href=""https://example.com"">   </a>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertHasIssue(result, "A11Y-EMPTY-LINK", HtmlIssueSeverity.Warning);
    }

    [Fact]
    public async Task LinkWithNbspOnly_ReportsViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <a href=""https://example.com"">&nbsp;</a>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertHasIssue(result, "A11Y-EMPTY-LINK", HtmlIssueSeverity.Warning);
    }

    [Fact]
    public async Task MultipleEmptyLinks_ReportsMultipleViolations()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <a href=""link1.html""></a>
    <a href=""link2.html""></a>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        Assert.Equal(2, CountIssues(result, "A11Y-EMPTY-LINK"));
    }
}
