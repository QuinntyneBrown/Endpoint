// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.StaticAnalysis.Html;

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.Html.BestPractices;

/// <summary>
/// Tests for BP-DEPRECATED-ATTR rule: Deprecated HTML attributes should not be used excessively.
/// </summary>
public class DeprecatedAttributeTests : HtmlStaticAnalysisTestBase
{
    [Fact]
    public async Task PageWithManyAlignAttributes_ReportsViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <div align=""center"">Content 1</div>
    <div align=""left"">Content 2</div>
    <div align=""right"">Content 3</div>
    <div align=""center"">Content 4</div>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertHasIssue(result, "BP-DEPRECATED-ATTR", HtmlIssueSeverity.Info);
        AssertHasIssueWithCategory(result, "BP-DEPRECATED-ATTR", "BestPractices");
    }

    [Fact]
    public async Task PageWithManyBgcolorAttributes_ReportsViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body bgcolor=""white"">
    <table bgcolor=""gray"">
        <tr bgcolor=""white"">
            <td bgcolor=""blue"">Cell</td>
        </tr>
    </table>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertHasIssue(result, "BP-DEPRECATED-ATTR", HtmlIssueSeverity.Info);
    }

    [Fact]
    public async Task PageWithManyBorderAttributes_ReportsViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <table border=""1"">
        <tr>
            <td border=""0"">Cell 1</td>
            <td border=""0"">Cell 2</td>
            <td border=""0"">Cell 3</td>
        </tr>
    </table>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertHasIssue(result, "BP-DEPRECATED-ATTR", HtmlIssueSeverity.Info);
    }

    [Fact]
    public async Task PageWithFewDeprecatedAttributes_NoViolation()
    {
        // Arrange - Only 2 uses, below threshold
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <div align=""center"">Content 1</div>
    <table border=""1"">
        <tr><td>Cell</td></tr>
    </table>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "BP-DEPRECATED-ATTR");
    }

    [Fact]
    public async Task PageWithCssStyles_NoViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <title>Test</title>
    <style>
        .centered { text-align: center; }
        .bordered { border: 1px solid black; }
    </style>
</head>
<body>
    <div class=""centered"">Content 1</div>
    <div class=""centered"">Content 2</div>
    <div class=""centered"">Content 3</div>
    <div class=""centered"">Content 4</div>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "BP-DEPRECATED-ATTR");
    }
}
