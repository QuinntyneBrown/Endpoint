// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.Scss;

/// <summary>
/// Acceptance tests for SCSS109 - Missing semicolons.
/// </summary>
public class MissingSemicolonTests
{
    private readonly IScssStaticAnalysisService _sut;

    public MissingSemicolonTests()
    {
        var mockLogger = new Mock<ILogger<ScssStaticAnalysisService>>();
        _sut = new ScssStaticAnalysisService(mockLogger.Object);
    }

    #region SCSS109 - Missing Semicolon Violation Tests

    [Fact]
    public void AnalyzeContent_WithMissingSemicolonBeforeClosingBrace_ShouldReportError()
    {
        // Arrange
        var scss = @"
.button {
    color: red
}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        Assert.True(result.HasIssues);
        var issue = Assert.Single(result.Issues.Where(i => i.Code == "SCSS109"));
        Assert.Equal(IssueSeverity.Error, issue.Severity);
    }

    [Fact]
    public void AnalyzeContent_WithProperSemicolons_ShouldNotReportError()
    {
        // Arrange
        var scss = @"
.button {
    color: red;
    background: blue;
}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        Assert.DoesNotContain(result.Issues, i => i.Code == "SCSS109");
    }

    [Fact]
    public void AnalyzeContent_WithMultipleMissingSemicolons_ShouldReportMultipleErrors()
    {
        // Arrange
        var scss = @"
.first {
    color: red
}

.second {
    background: blue
}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        var semicolonIssues = result.Issues.Where(i => i.Code == "SCSS109").ToList();
        Assert.Equal(2, semicolonIssues.Count);
        Assert.All(semicolonIssues, i => Assert.Equal(IssueSeverity.Error, i.Severity));
    }

    [Fact]
    public void AnalyzeContent_WithSemicolonOnLastDeclaration_ShouldNotReportError()
    {
        // Arrange
        var scss = @"
.button {
    color: red;
}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        Assert.DoesNotContain(result.Issues, i => i.Code == "SCSS109");
    }

    [Fact]
    public void AnalyzeContent_WithNestedRuleMissingSemicolon_ShouldReportError()
    {
        // Arrange
        var scss = @"
.parent {
    .child {
        color: red
    }
}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        Assert.Contains(result.Issues, i => i.Code == "SCSS109");
    }

    #endregion
}
