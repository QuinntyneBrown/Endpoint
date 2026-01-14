// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.StaticAnalysis.Html;

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.Html.Security;

/// <summary>
/// Tests for SEC-INLINE-JS rule: Inline JavaScript should be avoided.
/// </summary>
public class InlineJavaScriptTests : HtmlStaticAnalysisTestBase
{
    [Fact]
    public async Task PageWithInlineScript_ReportsViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <script>
        console.log('Hello');
    </script>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertHasIssue(result, "SEC-INLINE-JS", HtmlIssueSeverity.Warning);
        AssertHasIssueWithCategory(result, "SEC-INLINE-JS", "Security");
    }

    [Fact]
    public async Task PageWithExternalScriptOnly_NoViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <title>Test</title>
    <script src=""app.js""></script>
</head>
<body>
    <p>Content</p>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "SEC-INLINE-JS");
    }

    [Fact]
    public async Task PageWithEmptyScriptTag_NoViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <script></script>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "SEC-INLINE-JS");
    }

    [Fact]
    public async Task MultipleInlineScripts_ReportsMultipleViolations()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <script>var x = 1;</script>
    <script>var y = 2;</script>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        Assert.Equal(2, CountIssues(result, "SEC-INLINE-JS"));
    }
}
