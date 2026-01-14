// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.StaticAnalysis.Html;

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.Html.Accessibility;

/// <summary>
/// Tests for A11Y-HTML-LANG rule: HTML element must have lang attribute.
/// </summary>
public class HtmlLangAttributeTests : HtmlStaticAnalysisTestBase
{
    [Fact]
    public async Task HtmlWithoutLang_ReportsViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html>
<head><title>Test</title></head>
<body>
    <p>Hello World</p>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertHasIssue(result, "A11Y-HTML-LANG", HtmlIssueSeverity.Error);
        AssertHasIssueWithCategory(result, "A11Y-HTML-LANG", "Accessibility");
    }

    [Fact]
    public async Task HtmlWithLang_NoViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <p>Hello World</p>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "A11Y-HTML-LANG");
    }

    [Fact]
    public async Task HtmlWithLangFrench_NoViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""fr"">
<head><title>Test</title></head>
<body>
    <p>Bonjour le monde</p>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "A11Y-HTML-LANG");
    }

    [Fact]
    public async Task HtmlWithLangAndRegion_NoViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en-US"">
<head><title>Test</title></head>
<body>
    <p>Hello World</p>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "A11Y-HTML-LANG");
    }

    [Fact]
    public async Task HtmlWithSingleQuoteLang_NoViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang='en'>
<head><title>Test</title></head>
<body>
    <p>Hello World</p>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "A11Y-HTML-LANG");
    }
}
