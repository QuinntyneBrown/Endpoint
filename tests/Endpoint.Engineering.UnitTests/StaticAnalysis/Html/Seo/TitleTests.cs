// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.StaticAnalysis.Html;

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.Html.Seo;

/// <summary>
/// Tests for SEO-TITLE, SEO-TITLE-SHORT, and SEO-TITLE-LONG rules.
/// </summary>
public class TitleTests : HtmlStaticAnalysisTestBase
{
    [Fact]
    public async Task PageWithoutTitle_ReportsViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head></head>
<body>
    <p>Content without title</p>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertHasIssue(result, "SEO-TITLE", HtmlIssueSeverity.Error);
        AssertHasIssueWithCategory(result, "SEO-TITLE", "SEO");
    }

    [Fact]
    public async Task PageWithEmptyTitle_ReportsViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title></title></head>
<body>
    <p>Content</p>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertHasIssue(result, "SEO-TITLE", HtmlIssueSeverity.Error);
    }

    [Fact]
    public async Task PageWithShortTitle_ReportsViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Hi</title></head>
<body>
    <p>Content</p>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertHasIssue(result, "SEO-TITLE-SHORT", HtmlIssueSeverity.Warning);
        AssertHasIssueWithCategory(result, "SEO-TITLE-SHORT", "SEO");
    }

    [Fact]
    public async Task PageWithLongTitle_ReportsViolation()
    {
        // Arrange
        var longTitle = new string('A', 70); // More than 60 characters
        var html = $@"<!DOCTYPE html>
<html lang=""en"">
<head><title>{longTitle}</title></head>
<body>
    <p>Content</p>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertHasIssue(result, "SEO-TITLE-LONG", HtmlIssueSeverity.Warning);
        AssertHasIssueWithCategory(result, "SEO-TITLE-LONG", "SEO");
    }

    [Fact]
    public async Task PageWithOptimalTitle_NoViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Welcome to My Awesome Website - Best Products Online</title></head>
<body>
    <p>Content</p>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "SEO-TITLE");
        AssertNoIssue(result, "SEO-TITLE-SHORT");
        AssertNoIssue(result, "SEO-TITLE-LONG");
    }

    [Fact]
    public async Task PageWithWhitespaceTitle_ReportsViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>   </title></head>
<body>
    <p>Content</p>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertHasIssue(result, "SEO-TITLE", HtmlIssueSeverity.Error);
    }
}
