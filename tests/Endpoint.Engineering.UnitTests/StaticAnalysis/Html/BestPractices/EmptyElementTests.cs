// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.StaticAnalysis.Html;

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.Html.BestPractices;

/// <summary>
/// Tests for BP-EMPTY-ELEMENT rule: Empty container elements should be avoided.
/// </summary>
public class EmptyElementTests : HtmlStaticAnalysisTestBase
{
    [Fact]
    public async Task PageWithEmptyDiv_ReportsViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <div></div>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertHasIssue(result, "BP-EMPTY-ELEMENT", HtmlIssueSeverity.Info);
        AssertHasIssueWithCategory(result, "BP-EMPTY-ELEMENT", "BestPractices");
    }

    [Fact]
    public async Task PageWithEmptySpan_ReportsViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <span></span>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertHasIssue(result, "BP-EMPTY-ELEMENT", HtmlIssueSeverity.Info);
    }

    [Fact]
    public async Task PageWithEmptyParagraph_ReportsViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <p></p>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertHasIssue(result, "BP-EMPTY-ELEMENT", HtmlIssueSeverity.Info);
    }

    [Fact]
    public async Task PageWithEmptySection_ReportsViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <section></section>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertHasIssue(result, "BP-EMPTY-ELEMENT", HtmlIssueSeverity.Info);
    }

    [Fact]
    public async Task PageWithEmptyArticle_ReportsViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <article></article>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertHasIssue(result, "BP-EMPTY-ELEMENT", HtmlIssueSeverity.Info);
    }

    [Fact]
    public async Task PageWithContentInElements_NoViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <div>Content</div>
    <span>Inline content</span>
    <p>Paragraph content</p>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "BP-EMPTY-ELEMENT");
    }

    [Fact]
    public async Task PageWithEmptyDivWithWhitespace_ReportsViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <div>   </div>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertHasIssue(result, "BP-EMPTY-ELEMENT", HtmlIssueSeverity.Info);
    }

    [Fact]
    public async Task MultipleEmptyElements_ReportsMultipleViolations()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <div></div>
    <span></span>
    <p></p>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        Assert.Equal(3, CountIssues(result, "BP-EMPTY-ELEMENT"));
    }
}
