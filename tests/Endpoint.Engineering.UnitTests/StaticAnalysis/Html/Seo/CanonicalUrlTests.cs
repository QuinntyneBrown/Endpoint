// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.StaticAnalysis.Html;

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.Html.Seo;

/// <summary>
/// Tests for SEO-CANONICAL rule: Pages should have canonical URL.
/// </summary>
public class CanonicalUrlTests : HtmlStaticAnalysisTestBase
{
    [Fact]
    public async Task PageWithoutCanonical_ReportsViolation()
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
        AssertHasIssue(result, "SEO-CANONICAL", HtmlIssueSeverity.Info);
        AssertHasIssueWithCategory(result, "SEO-CANONICAL", "SEO");
    }

    [Fact]
    public async Task PageWithCanonical_NoViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <title>Test Page</title>
    <link rel=""canonical"" href=""https://example.com/page"">
</head>
<body>
    <p>Content</p>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "SEO-CANONICAL");
    }

    [Fact]
    public async Task PageWithCanonicalDifferentCase_NoViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <title>Test Page</title>
    <link REL=""Canonical"" HREF=""https://example.com/page"">
</head>
<body>
    <p>Content</p>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "SEO-CANONICAL");
    }

    [Fact]
    public async Task PageWithOtherLinkTags_NoCanonical_ReportsViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <title>Test Page</title>
    <link rel=""stylesheet"" href=""styles.css"">
    <link rel=""icon"" href=""favicon.ico"">
</head>
<body>
    <p>Content</p>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertHasIssue(result, "SEO-CANONICAL", HtmlIssueSeverity.Info);
    }
}
