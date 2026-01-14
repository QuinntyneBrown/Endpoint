// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.StaticAnalysis.Html;

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.Html.BestPractices;

/// <summary>
/// Tests for BP-DOCTYPE rule: Documents should have DOCTYPE declaration.
/// </summary>
public class DoctypeTests : HtmlStaticAnalysisTestBase
{
    [Fact]
    public async Task PageWithoutDoctype_ReportsViolation()
    {
        // Arrange
        var html = @"<html lang=""en"">
<head><title>Test</title></head>
<body>
    <p>Content without doctype</p>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertHasIssue(result, "BP-DOCTYPE", HtmlIssueSeverity.Error);
        AssertHasIssueWithCategory(result, "BP-DOCTYPE", "BestPractices");
    }

    [Fact]
    public async Task PageWithDoctype_NoViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <p>Content</p>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "BP-DOCTYPE");
    }

    [Fact]
    public async Task PageWithHtml5Doctype_NoViolation()
    {
        // Arrange
        var html = @"<!doctype html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <p>Content</p>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "BP-DOCTYPE");
    }

    [Fact]
    public async Task PageWithXhtmlDoctype_NoViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Strict//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd"">
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <p>Content</p>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "BP-DOCTYPE");
    }

    [Fact]
    public async Task PageWithDoctypeWithExtraSpaces_NoViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE   html  >
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <p>Content</p>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "BP-DOCTYPE");
    }
}
