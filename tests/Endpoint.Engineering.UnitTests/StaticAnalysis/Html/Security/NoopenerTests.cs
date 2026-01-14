// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.StaticAnalysis.Html;

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.Html.Security;

/// <summary>
/// Tests for SEC-NOOPENER rule: Links with target="_blank" should have rel="noopener".
/// </summary>
public class NoopenerTests : HtmlStaticAnalysisTestBase
{
    [Fact]
    public async Task LinkWithTargetBlankNoRel_ReportsViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <a href=""https://example.com"" target=""_blank"">External Link</a>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertHasIssue(result, "SEC-NOOPENER", HtmlIssueSeverity.Warning);
        AssertHasIssueWithCategory(result, "SEC-NOOPENER", "Security");
    }

    [Fact]
    public async Task LinkWithTargetBlankAndNoopener_NoViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <a href=""https://example.com"" target=""_blank"" rel=""noopener"">External Link</a>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "SEC-NOOPENER");
    }

    [Fact]
    public async Task LinkWithTargetBlankAndNoopenerNoreferrer_NoViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <a href=""https://example.com"" target=""_blank"" rel=""noopener noreferrer"">External Link</a>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "SEC-NOOPENER");
    }

    [Fact]
    public async Task LinkWithoutTargetBlank_NoViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <a href=""https://example.com"">External Link</a>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "SEC-NOOPENER");
    }

    [Fact]
    public async Task LinkWithTargetSelf_NoViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <a href=""https://example.com"" target=""_self"">Same Window Link</a>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "SEC-NOOPENER");
    }

    [Fact]
    public async Task MultipleLinksMissingNoopener_ReportsMultipleViolations()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <a href=""https://example1.com"" target=""_blank"">Link 1</a>
    <a href=""https://example2.com"" target=""_blank"">Link 2</a>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        Assert.Equal(2, CountIssues(result, "SEC-NOOPENER"));
    }

    [Fact]
    public async Task LinkWithTargetBlankAndOtherRel_ReportsViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <a href=""https://example.com"" target=""_blank"" rel=""external"">External Link</a>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertHasIssue(result, "SEC-NOOPENER", HtmlIssueSeverity.Warning);
    }
}
