// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.StaticAnalysis.Html;

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.Html.Seo;

/// <summary>
/// Tests for SEO-VIEWPORT rule: Pages should have viewport meta tag.
/// </summary>
public class ViewportTests : HtmlStaticAnalysisTestBase
{
    [Fact]
    public async Task PageWithoutViewport_ReportsViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <title>Test Page</title>
</head>
<body>
    <p>Content</p>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertHasIssue(result, "SEO-VIEWPORT", HtmlIssueSeverity.Warning);
        AssertHasIssueWithCategory(result, "SEO-VIEWPORT", "SEO");
    }

    [Fact]
    public async Task PageWithViewport_NoViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <title>Test Page</title>
    <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
</head>
<body>
    <p>Content</p>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "SEO-VIEWPORT");
    }

    [Fact]
    public async Task PageWithViewportDifferentCase_NoViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <title>Test Page</title>
    <meta NAME=""Viewport"" CONTENT=""width=device-width, initial-scale=1"">
</head>
<body>
    <p>Content</p>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "SEO-VIEWPORT");
    }

    [Fact]
    public async Task PageWithViewportSingleQuotes_NoViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <title>Test Page</title>
    <meta name='viewport' content='width=device-width'>
</head>
<body>
    <p>Content</p>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "SEO-VIEWPORT");
    }
}
