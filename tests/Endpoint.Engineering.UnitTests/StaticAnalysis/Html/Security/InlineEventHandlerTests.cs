// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.StaticAnalysis.Html;

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.Html.Security;

/// <summary>
/// Tests for SEC-INLINE-HANDLER rule: Inline event handlers should be avoided.
/// </summary>
public class InlineEventHandlerTests : HtmlStaticAnalysisTestBase
{
    [Fact]
    public async Task ElementWithOnClick_ReportsViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <button onclick=""alert('clicked')"">Click me</button>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertHasIssue(result, "SEC-INLINE-HANDLER", HtmlIssueSeverity.Warning);
        AssertHasIssueWithCategory(result, "SEC-INLINE-HANDLER", "Security");
    }

    [Fact]
    public async Task ElementWithOnMouseOver_ReportsViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <div onmouseover=""highlight(this)"">Hover me</div>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertHasIssue(result, "SEC-INLINE-HANDLER", HtmlIssueSeverity.Warning);
    }

    [Fact]
    public async Task ElementWithOnSubmit_ReportsViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <form onsubmit=""validate()"">
        <input type=""submit"" value=""Submit"">
    </form>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertHasIssue(result, "SEC-INLINE-HANDLER", HtmlIssueSeverity.Warning);
    }

    [Fact]
    public async Task ElementWithOnLoad_ReportsViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body onload=""init()"">
    <p>Content</p>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertHasIssue(result, "SEC-INLINE-HANDLER", HtmlIssueSeverity.Warning);
    }

    [Fact]
    public async Task PageWithoutEventHandlers_NoViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <button id=""myButton"">Click me</button>
    <script src=""app.js""></script>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "SEC-INLINE-HANDLER");
    }

    [Fact]
    public async Task MultipleInlineHandlers_ReportsMultipleViolations()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <button onclick=""action1()"">Button 1</button>
    <button onclick=""action2()"">Button 2</button>
    <input onchange=""handleChange()"">
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        Assert.Equal(3, CountIssues(result, "SEC-INLINE-HANDLER"));
    }
}
