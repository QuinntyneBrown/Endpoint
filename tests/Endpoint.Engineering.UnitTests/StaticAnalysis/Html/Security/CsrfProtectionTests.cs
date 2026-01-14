// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.StaticAnalysis.Html;

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.Html.Security;

/// <summary>
/// Tests for SEC-CSRF rule: POST forms should have CSRF protection.
/// </summary>
public class CsrfProtectionTests : HtmlStaticAnalysisTestBase
{
    [Fact]
    public async Task PostFormWithoutCsrfToken_ReportsViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <form method=""post"" action=""/submit"">
        <input type=""text"" name=""username"">
        <input type=""submit"" value=""Submit"">
    </form>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertHasIssue(result, "SEC-CSRF", HtmlIssueSeverity.Info);
        AssertHasIssueWithCategory(result, "SEC-CSRF", "Security");
    }

    [Fact]
    public async Task PostFormWithCsrfToken_NoViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <form method=""post"" action=""/submit"">
        <input type=""hidden"" name=""csrf_token"" value=""abc123"">
        <input type=""text"" name=""username"">
        <input type=""submit"" value=""Submit"">
    </form>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "SEC-CSRF");
    }

    [Fact]
    public async Task PostFormWithRequestVerificationToken_NoViolation()
    {
        // Arrange - ASP.NET style token
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <form method=""post"" action=""/submit"">
        <input type=""hidden"" name=""__RequestVerificationToken"" value=""xyz789"">
        <input type=""text"" name=""username"">
        <input type=""submit"" value=""Submit"">
    </form>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "SEC-CSRF");
    }

    [Fact]
    public async Task PostFormWithAuthenticityToken_NoViolation()
    {
        // Arrange - Rails style token
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <form method=""post"" action=""/submit"">
        <input type=""hidden"" name=""authenticity_token"" value=""token123"">
        <input type=""text"" name=""username"">
        <input type=""submit"" value=""Submit"">
    </form>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "SEC-CSRF");
    }

    [Fact]
    public async Task GetForm_NoViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <form method=""get"" action=""/search"">
        <input type=""text"" name=""q"">
        <input type=""submit"" value=""Search"">
    </form>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "SEC-CSRF");
    }

    [Fact]
    public async Task FormWithoutMethod_NoViolation()
    {
        // Arrange - Default method is GET
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <form action=""/search"">
        <input type=""text"" name=""q"">
        <input type=""submit"" value=""Search"">
    </form>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "SEC-CSRF");
    }
}
