// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.StaticAnalysis.Html;

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.Html.Accessibility;

/// <summary>
/// Tests for A11Y-TABLE-HEADERS rule: Tables should have header cells.
/// </summary>
public class TableHeaderTests : HtmlStaticAnalysisTestBase
{
    [Fact]
    public async Task TableWithoutHeaders_ReportsViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <table>
        <tr>
            <td>Name</td>
            <td>Age</td>
        </tr>
        <tr>
            <td>John</td>
            <td>25</td>
        </tr>
    </table>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertHasIssue(result, "A11Y-TABLE-HEADERS", HtmlIssueSeverity.Warning);
        AssertHasIssueWithCategory(result, "A11Y-TABLE-HEADERS", "Accessibility");
    }

    [Fact]
    public async Task TableWithThElements_NoViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <table>
        <tr>
            <th>Name</th>
            <th>Age</th>
        </tr>
        <tr>
            <td>John</td>
            <td>25</td>
        </tr>
    </table>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "A11Y-TABLE-HEADERS");
    }

    [Fact]
    public async Task TableWithScopeAttribute_NoViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <table>
        <tr>
            <td scope=""col"">Name</td>
            <td scope=""col"">Age</td>
        </tr>
        <tr>
            <td>John</td>
            <td>25</td>
        </tr>
    </table>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "A11Y-TABLE-HEADERS");
    }

    [Fact]
    public async Task MultipleTablesWithoutHeaders_ReportsMultipleViolations()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <table>
        <tr><td>Data 1</td></tr>
    </table>
    <table>
        <tr><td>Data 2</td></tr>
    </table>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        Assert.Equal(2, CountIssues(result, "A11Y-TABLE-HEADERS"));
    }

    [Fact]
    public async Task TableWithTheadAndTh_NoViolation()
    {
        // Arrange
        var html = @"<!DOCTYPE html>
<html lang=""en"">
<head><title>Test</title></head>
<body>
    <table>
        <thead>
            <tr>
                <th>Header 1</th>
                <th>Header 2</th>
            </tr>
        </thead>
        <tbody>
            <tr>
                <td>Data 1</td>
                <td>Data 2</td>
            </tr>
        </tbody>
    </table>
</body>
</html>";

        // Act
        var result = await AnalyzeHtmlAsync(html);

        // Assert
        AssertNoIssue(result, "A11Y-TABLE-HEADERS");
    }
}
