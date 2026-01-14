// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.StaticAnalysis.Html;

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.Html.Accessibility;

/// <summary>
/// Tests for A11Y-SKIP-NAV rule: Pages should have skip navigation links.
/// </summary>
public class SkipNavigationTests : HtmlStaticAnalysisTestBase
{
    [Fact]
    public async Task PageWithMainContentButNoSkipNav_ReportsViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <nav>Navigation here</nav>
    <main>Main content</main>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertHasIssue(result, "A11Y-SKIP-NAV", HtmlIssueSeverity.Info);
        AssertHasIssueWithCategory(result, "A11Y-SKIP-NAV", "Accessibility");
    }

    [Fact]
    public async Task PageWithSkipToMainLink_NoViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <a href=""#main"">Skip to main content</a>
    <nav>Navigation here</nav>
    <main id=""main"">Main content</main>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "A11Y-SKIP-NAV");
    }

    [Fact]
    public async Task PageWithSkipNavLink_NoViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <a href=""#content"">Skip nav</a>
    <nav>Navigation here</nav>
    <main id=""content"">Main content</main>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "A11Y-SKIP-NAV");
    }

    [Fact]
    public async Task PageWithMainId_NoSkipLink_ReportsViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <nav>Navigation here</nav>
    <div id=""main"">Main content</div>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertHasIssue(result, "A11Y-SKIP-NAV", HtmlIssueSeverity.Info);
    }

    [Fact]
    public async Task SimplePageWithoutMainContent_NoViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <p>Simple page content</p>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "A11Y-SKIP-NAV");
    }
}
