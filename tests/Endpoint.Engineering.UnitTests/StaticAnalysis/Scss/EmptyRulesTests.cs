// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.Scss;

/// <summary>
/// Acceptance tests for SCSS101 - No empty rules.
/// </summary>
public class EmptyRulesTests
{
    private readonly IScssStaticAnalysisService _sut;

    public EmptyRulesTests()
    {
        var mockLogger = new Mock<ILogger<ScssStaticAnalysisService>>();
        _sut = new ScssStaticAnalysisService(mockLogger.Object);
    }

    #region SCSS101 - Empty Rules Violation Tests

    [Fact]
    public void AnalyzeContent_WithEmptyRuleBlock_ShouldReportWarning()
    {
        // Arrange
        var scss = @"
.empty-class {
}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        Assert.True(result.HasIssues);
        Assert.Contains(result.Issues, i => i.Code == "SCSS101" && i.Severity == IssueSeverity.Warning);
    }

    [Fact]
    public void AnalyzeContent_WithEmptyRuleBlockWithWhitespace_ShouldReportWarning()
    {
        // Arrange
        var scss = @"
.empty-class {

}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        Assert.True(result.HasIssues);
        Assert.Contains(result.Issues, i => i.Code == "SCSS101");
    }

    [Fact]
    public void AnalyzeContent_WithMultipleEmptyRules_ShouldReportMultipleWarnings()
    {
        // Arrange
        var scss = @"
.first-empty { }
.second-empty { }
.third-empty { }";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        Assert.True(result.HasIssues);
        var emptyRuleIssues = result.Issues.Where(i => i.Code == "SCSS101").ToList();
        Assert.Equal(3, emptyRuleIssues.Count);
    }

    [Fact]
    public void AnalyzeContent_WithNonEmptyRuleBlock_ShouldNotReportWarning()
    {
        // Arrange
        var scss = @"
.has-styles {
    color: red;
    background: blue;
}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        Assert.DoesNotContain(result.Issues, i => i.Code == "SCSS101");
    }

    [Fact]
    public void AnalyzeContent_WithNestedEmptyRule_ShouldReportWarning()
    {
        // Arrange
        var scss = @"
.parent {
    color: red;
    .nested-empty { }
}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        Assert.Contains(result.Issues, i => i.Code == "SCSS101");
    }

    #endregion
}
