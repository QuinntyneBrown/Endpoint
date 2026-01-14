// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.StaticAnalysis.Html;

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.Html.Security;

/// <summary>
/// Tests for SEC-MIXED-CONTENT rule: HTTP links may cause mixed content issues.
/// </summary>
public class MixedContentTests : HtmlStaticAnalysisTestBase
{
    [Fact]
    public async Task LinkWithHttpUrl_ReportsViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <a href=""http://example.com"">Insecure Link</a>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertHasIssue(result, "SEC-MIXED-CONTENT", HtmlIssueSeverity.Warning);
        AssertHasIssueWithCategory(result, "SEC-MIXED-CONTENT", "Security");
    }

    [Fact]
    public async Task ImageWithHttpSrc_ReportsViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <img src=""http://example.com/image.jpg"" alt=""Image"">
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertHasIssue(result, "SEC-MIXED-CONTENT", HtmlIssueSeverity.Warning);
    }

    [Fact]
    public async Task ScriptWithHttpSrc_ReportsViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <title>Test</title>
    <script src=""http://cdn.example.com/lib.js""></script>
</head>
<body>
    <p>Content</p>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertHasIssue(result, "SEC-MIXED-CONTENT", HtmlIssueSeverity.Warning);
    }

    [Fact]
    public async Task HttpsLinks_NoViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <a href=""https://example.com"">Secure Link</a>
    <img src=""https://example.com/image.jpg"" alt=""Image"">
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "SEC-MIXED-CONTENT");
    }

    [Fact]
    public async Task RelativeUrls_NoViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <a href=""/page"">Relative Link</a>
    <img src=""images/photo.jpg"" alt=""Photo"">
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "SEC-MIXED-CONTENT");
    }

    [Fact]
    public async Task ProtocolRelativeUrls_NoViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <script src=""//cdn.example.com/lib.js""></script>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "SEC-MIXED-CONTENT");
    }

    [Fact]
    public async Task MultipleHttpResources_ReportsMultipleViolations()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <a href=""http://example1.com"">Link 1</a>
    <img src=""http://example2.com/img.jpg"" alt=""Image"">
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        Assert.Equal(2, CountIssues(result, "SEC-MIXED-CONTENT"));
    }
}
