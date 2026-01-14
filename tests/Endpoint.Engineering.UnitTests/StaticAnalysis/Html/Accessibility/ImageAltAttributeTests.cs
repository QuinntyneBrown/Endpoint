// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.StaticAnalysis.Html;

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.Html.Accessibility;

/// <summary>
/// Tests for A11Y-IMG-ALT rule: Images must have alt attributes.
/// </summary>
public class ImageAltAttributeTests : HtmlStaticAnalysisTestBase
{
    [Fact]
    public async Task ImageWithoutAlt_ReportsViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <img src=""photo.jpg"">
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertHasIssue(result, "A11Y-IMG-ALT", HtmlIssueSeverity.Error);
        AssertHasIssueWithCategory(result, "A11Y-IMG-ALT", "Accessibility");
    }

    [Fact]
    public async Task ImageWithAlt_NoViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <img src=""photo.jpg"" alt=""A beautiful sunset"">
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "A11Y-IMG-ALT");
    }

    [Fact]
    public async Task ImageWithEmptyAlt_NoViolation()
    {
        // Arrange - Empty alt is valid for decorative images
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
        AssertNoIssue(result, "A11Y-IMG-ALT");
    }

    [Fact]
    public async Task MultipleImagesWithoutAlt_ReportsMultipleViolations()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <img src=""photo1.jpg"">
    <img src=""photo2.jpg"">
    <img src=""photo3.jpg"">
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        Assert.Equal(3, CountIssues(result, "A11Y-IMG-ALT"));
    }

    [Fact]
    public async Task ImageWithSingleQuoteAlt_NoViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <img src=""photo.jpg"" alt='Description with single quotes'>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "A11Y-IMG-ALT");
    }
}
