// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.StaticAnalysis.Html;

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.Html.Seo;

/// <summary>
/// Tests for SEO-H1-MISSING and SEO-H1-MULTIPLE rules.
/// </summary>
public class HeadingTests : HtmlStaticAnalysisTestBase
{
    [Fact]
    public async Task PageWithoutH1_ReportsViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test Page</title></head>
<body>
    <h2>Subheading</h2>
    <p>Content without main heading</p>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertHasIssue(result, "SEO-H1-MISSING", HtmlIssueSeverity.Warning);
        AssertHasIssueWithCategory(result, "SEO-H1-MISSING", "SEO");
    }

    [Fact]
    public async Task PageWithSingleH1_NoViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test Page</title></head>
<body>
    <h1>Main Heading</h1>
    <p>Content</p>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "SEO-H1-MISSING");
        AssertNoIssue(result, "SEO-H1-MULTIPLE");
    }

    [Fact]
    public async Task PageWithMultipleH1_ReportsViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test Page</title></head>
<body>
    <h1>First Heading</h1>
    <p>Content</p>
    <h1>Second Heading</h1>
    <p>More content</p>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertHasIssue(result, "SEO-H1-MULTIPLE", HtmlIssueSeverity.Warning);
        AssertHasIssueWithCategory(result, "SEO-H1-MULTIPLE", "SEO");
    }

    [Fact]
    public async Task PageWithThreeH1_ReportsViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test Page</title></head>
<body>
    <h1>First</h1>
    <h1>Second</h1>
    <h1>Third</h1>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertHasIssue(result, "SEO-H1-MULTIPLE", HtmlIssueSeverity.Warning);
    }

    [Fact]
    public async Task PageWithH1WithAttributes_NoViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test Page</title></head>
<body>
    <h1 class=""main-title"" id=""page-heading"">Main Heading</h1>
    <p>Content</p>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "SEO-H1-MISSING");
        AssertNoIssue(result, "SEO-H1-MULTIPLE");
    }
}
