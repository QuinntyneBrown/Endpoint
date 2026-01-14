// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.Scss;

/// <summary>
/// Acceptance tests for SCSS106 - No !important usage.
/// </summary>
public class ImportantUsageTests
{
    private readonly IScssStaticAnalysisService _sut;

    public ImportantUsageTests()
    {
        var mockLogger = new Mock<ILogger<ScssStaticAnalysisService>>();
        _sut = new ScssStaticAnalysisService(mockLogger.Object);
    }

    #region SCSS106 - !important Usage Violation Tests

    [Fact]
    public void AnalyzeContent_WithImportantDeclaration_ShouldReportWarning()
    {
        // Arrange
        var scss = @"
.override {
    color: red !important;
}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        Assert.True(result.HasIssues);
        var issue = Assert.Single(result.Issues.Where(i => i.Code == "SCSS106"));
        Assert.Equal(IssueSeverity.Warning, issue.Severity);
    }

    [Fact]
    public void AnalyzeContent_WithMultipleImportantDeclarations_ShouldReportMultipleWarnings()
    {
        // Arrange
        var scss = @"
.override {
    color: red !important;
    background: blue !important;
    font-size: 16px !important;
}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        var importantIssues = result.Issues.Where(i => i.Code == "SCSS106").ToList();
        Assert.Equal(3, importantIssues.Count);
    }

    [Fact]
    public void AnalyzeContent_WithImportantInNestedRule_ShouldReportWarning()
    {
        // Arrange
        var scss = @"
.parent {
    .child {
        display: none !important;
    }
}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        Assert.Contains(result.Issues, i => i.Code == "SCSS106");
    }

    [Fact]
    public void AnalyzeContent_WithoutImportant_ShouldNotReportWarning()
    {
        // Arrange
        var scss = @"
.normal {
    color: red;
    background: blue;
    font-size: 16px;
}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        Assert.DoesNotContain(result.Issues, i => i.Code == "SCSS106");
    }

    [Fact]
    public void AnalyzeContent_WithImportantOnSingleLine_ShouldReportCorrectLine()
    {
        // Arrange
        var scss = @".hidden { display: none !important; }";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        var issue = Assert.Single(result.Issues.Where(i => i.Code == "SCSS106"));
        Assert.Equal(1, issue.Line);
    }

    #endregion
}
