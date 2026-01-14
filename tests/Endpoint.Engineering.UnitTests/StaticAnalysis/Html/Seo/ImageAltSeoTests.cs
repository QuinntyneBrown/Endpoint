// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.StaticAnalysis.Html;

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.Html.Seo;

/// <summary>
/// Tests for SEO-IMG-ALT-SHORT rule: Image alt text should be descriptive for SEO.
/// </summary>
public class ImageAltSeoTests : HtmlStaticAnalysisTestBase
{
    [Fact]
    public async Task ImageWithShortAlt_ReportsViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <img src=""photo.jpg"" alt=""Hi"">
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertHasIssue(result, "SEO-IMG-ALT-SHORT", HtmlIssueSeverity.Info);
        AssertHasIssueWithCategory(result, "SEO-IMG-ALT-SHORT", "SEO");
    }

    [Fact]
    public async Task ImageWithDescriptiveAlt_NoViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <img src=""photo.jpg"" alt=""A beautiful sunset over the ocean with orange and pink clouds"">
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "SEO-IMG-ALT-SHORT");
    }

    [Fact]
    public async Task ImageWithEmptyAlt_NoViolationForSeoRule()
    {
        // Arrange - Empty alt is valid for decorative images, shouldn't trigger SEO-IMG-ALT-SHORT
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <img src=""decorative.jpg"" alt="""">
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "SEO-IMG-ALT-SHORT");
    }

    [Fact]
    public async Task ImageWithFiveCharAlt_NoViolation()
    {
        // Arrange - Exactly 5 characters should not trigger the violation
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <img src=""photo.jpg"" alt=""Hello"">
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "SEO-IMG-ALT-SHORT");
    }

    [Fact]
    public async Task MultipleImagesWithShortAlt_ReportsMultipleViolations()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <img src=""photo1.jpg"" alt=""a"">
    <img src=""photo2.jpg"" alt=""ab"">
    <img src=""photo3.jpg"" alt=""abc"">
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        Assert.Equal(3, CountIssues(result, "SEO-IMG-ALT-SHORT"));
    }
}
