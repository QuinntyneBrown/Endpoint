// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.StaticAnalysis.Html;

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.Html.Seo;

/// <summary>
/// Tests for SEO-META-DESC rule: Pages should have meta description.
/// </summary>
public class MetaDescriptionTests : HtmlStaticAnalysisTestBase
{
    [Fact]
    public async Task PageWithoutMetaDescription_ReportsViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test Page</title></head>
<body>
    <p>Content</p>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertHasIssue(result, "SEO-META-DESC", HtmlIssueSeverity.Warning);
        AssertHasIssueWithCategory(result, "SEO-META-DESC", "SEO");
    }

    [Fact]
    public async Task PageWithMetaDescription_NoViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <title>Test Page</title>
    <meta name=""description"" content=""This is a test page with a proper meta description for SEO purposes."">
</head>
<body>
    <p>Content</p>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "SEO-META-DESC");
    }

    [Fact]
    public async Task PageWithMetaDescriptionDifferentCase_NoViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <title>Test Page</title>
    <meta NAME=""Description"" CONTENT=""This is a test page."">
</head>
<body>
    <p>Content</p>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "SEO-META-DESC");
    }

    [Fact]
    public async Task PageWithOtherMetaTags_NoDescription_ReportsViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <title>Test Page</title>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
    <meta name=""keywords"" content=""test, page"">
</head>
<body>
    <p>Content</p>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertHasIssue(result, "SEO-META-DESC", HtmlIssueSeverity.Warning);
    }
}
