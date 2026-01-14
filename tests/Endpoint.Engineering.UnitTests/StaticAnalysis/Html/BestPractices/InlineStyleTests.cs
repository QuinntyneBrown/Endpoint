// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.StaticAnalysis.Html;

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.Html.BestPractices;

/// <summary>
/// Tests for BP-INLINE-STYLE rule: Excessive inline styles should be avoided.
/// </summary>
public class InlineStyleTests : HtmlStaticAnalysisTestBase
{
    [Fact]
    public async Task PageWithManyInlineStyles_ReportsViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <div style=""color: red"">Text 1</div>
    <div style=""color: blue"">Text 2</div>
    <div style=""color: green"">Text 3</div>
    <div style=""color: yellow"">Text 4</div>
    <div style=""color: purple"">Text 5</div>
    <div style=""color: orange"">Text 6</div>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertHasIssue(result, "BP-INLINE-STYLE", HtmlIssueSeverity.Warning);
        AssertHasIssueWithCategory(result, "BP-INLINE-STYLE", "BestPractices");
    }

    [Fact]
    public async Task PageWithFewInlineStyles_NoViolation()
    {
        // Arrange - Only 3 inline styles, below threshold
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <div style=""color: red"">Text 1</div>
    <div style=""color: blue"">Text 2</div>
    <div style=""color: green"">Text 3</div>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "BP-INLINE-STYLE");
    }

    [Fact]
    public async Task PageWithExternalStylesheet_NoViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <title>Test</title>
    <link rel=""stylesheet"" href=""styles.css"">
</head>
<body>
    <div class=""red"">Text 1</div>
    <div class=""blue"">Text 2</div>
    <div class=""green"">Text 3</div>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "BP-INLINE-STYLE");
    }

    [Fact]
    public async Task PageWithInternalStylesheet_NoViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <title>Test</title>
    <style>
        .red { color: red; }
        .blue { color: blue; }
    </style>
</head>
<body>
    <div class=""red"">Text 1</div>
    <div class=""blue"">Text 2</div>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "BP-INLINE-STYLE");
    }

    [Fact]
    public async Task PageWithNoStyles_NoViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <div>Plain text</div>
    <p>More plain text</p>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "BP-INLINE-STYLE");
    }
}
