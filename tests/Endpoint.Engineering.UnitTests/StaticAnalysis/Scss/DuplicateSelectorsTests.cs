// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.Scss;

/// <summary>
/// Acceptance tests for SCSS102 - No duplicate selectors.
/// </summary>
public class DuplicateSelectorsTests
{
    private readonly IScssStaticAnalysisService _sut;

    public DuplicateSelectorsTests()
    {
        var mockLogger = new Mock<ILogger<ScssStaticAnalysisService>>();
        _sut = new ScssStaticAnalysisService(mockLogger.Object);
    }

    #region SCSS102 - Duplicate Selectors Violation Tests

    [Fact]
    public void AnalyzeContent_WithDuplicateClassSelector_ShouldReportWarning()
    {
        // Arrange
        var scss = @"
.button {
    color: red;
}

.button {
    background: blue;
}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        Assert.True(result.HasIssues);
        Assert.Contains(result.Issues, i => i.Code == "SCSS102" && i.Severity == IssueSeverity.Warning);
    }

    [Fact]
    public void AnalyzeContent_WithDuplicateElementSelector_ShouldReportWarning()
    {
        // Arrange
        var scss = @"
h1 {
    font-size: 24px;
}

h1 {
    color: black;
}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        Assert.True(result.HasIssues);
        var issue = Assert.Single(result.Issues.Where(i => i.Code == "SCSS102"));
        Assert.Contains("h1", issue.Message);
    }

    [Fact]
    public void AnalyzeContent_WithTriplicateSelector_ShouldReportWarning()
    {
        // Arrange
        var scss = @"
.card {
    padding: 10px;
}

.card {
    margin: 5px;
}

.card {
    border: 1px solid;
}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        Assert.True(result.HasIssues);
        var issue = Assert.Single(result.Issues.Where(i => i.Code == "SCSS102"));
        Assert.Contains("1", issue.Message); // Should mention multiple lines
    }

    [Fact]
    public void AnalyzeContent_WithUniqueSelectors_ShouldNotReportWarning()
    {
        // Arrange
        var scss = @"
.button {
    color: red;
}

.card {
    background: blue;
}

.header {
    font-size: 20px;
}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        Assert.DoesNotContain(result.Issues, i => i.Code == "SCSS102");
    }

    [Fact]
    public void AnalyzeContent_WithDuplicateComplexSelector_ShouldReportWarning()
    {
        // Arrange
        var scss = @"
.container .item {
    color: red;
}

.container .item {
    background: blue;
}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        Assert.Contains(result.Issues, i => i.Code == "SCSS102");
    }

    #endregion
}
