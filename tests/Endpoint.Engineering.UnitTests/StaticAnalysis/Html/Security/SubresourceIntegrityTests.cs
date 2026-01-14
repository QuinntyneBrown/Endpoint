// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.StaticAnalysis.Html;

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.Html.Security;

/// <summary>
/// Tests for SEC-SRI rule: External scripts should have integrity attribute.
/// </summary>
public class SubresourceIntegrityTests : HtmlStaticAnalysisTestBase
{
    [Fact]
    public async Task ExternalScriptWithoutIntegrity_ReportsViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <title>Test</title>
    <script src=""https://cdn.example.com/lib.js""></script>
</head>
<body>
    <p>Content</p>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertHasIssue(result, "SEC-SRI", HtmlIssueSeverity.Warning);
        AssertHasIssueWithCategory(result, "SEC-SRI", "Security");
    }

    [Fact]
    public async Task ExternalScriptWithIntegrity_NoViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <title>Test</title>
    <script src=""https://cdn.example.com/lib.js"" integrity=""sha384-abc123"" crossorigin=""anonymous""></script>
</head>
<body>
    <p>Content</p>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "SEC-SRI");
    }

    [Fact]
    public async Task LocalScript_NoViolation()
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
        AssertNoIssue(result, "SEC-SRI");
    }

    [Fact]
    public async Task ProtocolRelativeScriptWithoutIntegrity_ReportsViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <title>Test</title>
    <script src=""//cdn.example.com/lib.js""></script>
</head>
<body>
    <p>Content</p>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertHasIssue(result, "SEC-SRI", HtmlIssueSeverity.Warning);
    }

    [Fact]
    public async Task RelativePathScript_NoViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <title>Test</title>
    <script src=""/js/app.js""></script>
    <script src=""../scripts/utils.js""></script>
</head>
<body>
    <p>Content</p>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "SEC-SRI");
    }

    [Fact]
    public async Task MultipleCdnScriptsWithoutIntegrity_ReportsMultipleViolations()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <title>Test</title>
    <script src=""https://cdn.example.com/lib1.js""></script>
    <script src=""https://cdn.example.com/lib2.js""></script>
</head>
<body>
    <p>Content</p>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        Assert.Equal(2, CountIssues(result, "SEC-SRI"));
    }
}
