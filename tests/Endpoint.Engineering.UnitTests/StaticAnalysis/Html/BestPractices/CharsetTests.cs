// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.StaticAnalysis.Html;

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.Html.BestPractices;

/// <summary>
/// Tests for BP-CHARSET rule: Documents should have charset declaration.
/// </summary>
public class CharsetTests : HtmlStaticAnalysisTestBase
{
    [Fact]
    public async Task PageWithoutCharset_ReportsViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <p>Content without charset</p>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertHasIssue(result, "BP-CHARSET", HtmlIssueSeverity.Warning);
        AssertHasIssueWithCategory(result, "BP-CHARSET", "BestPractices");
    }

    [Fact]
    public async Task PageWithCharset_NoViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <title>Test</title>
</head>
<body>
    <p>Content</p>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "BP-CHARSET");
    }

    [Fact]
    public async Task PageWithCharsetLowercase_NoViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""utf-8"">
    <title>Test</title>
</head>
<body>
    <p>Content</p>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "BP-CHARSET");
    }

    [Fact]
    public async Task PageWithHttpEquivCharset_NoViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"">
    <title>Test</title>
</head>
<body>
    <p>Content</p>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "BP-CHARSET");
    }

    [Fact]
    public async Task PageWithCharsetSingleQuotes_NoViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset='UTF-8'>
    <title>Test</title>
</head>
<body>
    <p>Content</p>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "BP-CHARSET");
    }
}
