// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.StaticAnalysis.Html;

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.Html.Accessibility;

/// <summary>
/// Tests for A11Y-INPUT-LABEL rule: Form inputs must have associated labels.
/// </summary>
public class InputLabelTests : HtmlStaticAnalysisTestBase
{
    [Fact]
    public async Task InputWithIdButNoLabel_ReportsViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <input type=""text"" id=""username"">
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertHasIssue(result, "A11Y-INPUT-LABEL", HtmlIssueSeverity.Warning);
        AssertHasIssueWithCategory(result, "A11Y-INPUT-LABEL", "Accessibility");
    }

    [Fact]
    public async Task InputWithMatchingLabel_NoViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <label for=""username"">Username</label>
    <input type=""text"" id=""username"">
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "A11Y-INPUT-LABEL");
    }

    [Fact]
    public async Task InputWithAriaLabel_NoViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <input type=""text"" aria-label=""Search"">
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "A11Y-INPUT-LABEL");
    }

    [Fact]
    public async Task InputWithoutIdOrAriaLabel_ReportsViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <input type=""text"" name=""search"">
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertHasIssue(result, "A11Y-INPUT-LABEL", HtmlIssueSeverity.Warning);
    }

    [Fact]
    public async Task HiddenInput_NoViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <input type=""hidden"" name=""csrf_token"" value=""abc123"">
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "A11Y-INPUT-LABEL");
    }

    [Fact]
    public async Task SubmitButton_NoViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <input type=""submit"" value=""Submit"">
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "A11Y-INPUT-LABEL");
    }

    [Fact]
    public async Task ButtonInput_NoViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <input type=""button"" value=""Click me"">
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "A11Y-INPUT-LABEL");
    }
}
