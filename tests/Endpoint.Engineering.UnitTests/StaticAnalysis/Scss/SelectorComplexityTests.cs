// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.Scss;

/// <summary>
/// Acceptance tests for SCSS108 - Selector complexity.
/// </summary>
public class SelectorComplexityTests
{
    private readonly IScssStaticAnalysisService _sut;

    public SelectorComplexityTests()
    {
        var mockLogger = new Mock<ILogger<ScssStaticAnalysisService>>();
        _sut = new ScssStaticAnalysisService(mockLogger.Object);
    }

    #region SCSS108 - Selector Complexity Violation Tests

    [Fact]
    public void AnalyzeContent_WithComplexSelector_ShouldReportWarning()
    {
        // Arrange - 5 parts exceeds the recommended max of 4
        var scss = @"
.container .wrapper .content .item .text {
    color: red;
}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        Assert.True(result.HasIssues);
        var issue = Assert.Single(result.Issues.Where(i => i.Code == "SCSS108"));
        Assert.Equal(IssueSeverity.Warning, issue.Severity);
        Assert.Contains("5", issue.Message);
    }

    [Fact]
    public void AnalyzeContent_WithVeryComplexSelector_ShouldReportWarning()
    {
        // Arrange - 7 parts
        var scss = @"
body .main .section .container .row .col .item {
    padding: 10px;
}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        var issue = Assert.Single(result.Issues.Where(i => i.Code == "SCSS108"));
        Assert.Contains("7", issue.Message);
    }

    [Fact]
    public void AnalyzeContent_WithChildCombinatorSelector_ShouldCountCorrectly()
    {
        // Arrange - 5 parts with child combinators
        var scss = @"
.nav > .menu > .item > .link > .icon {
    display: inline;
}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        Assert.Contains(result.Issues, i => i.Code == "SCSS108");
    }

    [Fact]
    public void AnalyzeContent_WithSimpleSelector_ShouldNotReportWarning()
    {
        // Arrange - Only 1 part
        var scss = @"
.button {
    color: blue;
}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        Assert.DoesNotContain(result.Issues, i => i.Code == "SCSS108");
    }

    [Fact]
    public void AnalyzeContent_WithFourPartSelector_ShouldNotReportWarning()
    {
        // Arrange - Exactly 4 parts (at the limit)
        var scss = @"
.container .wrapper .content .item {
    color: red;
}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        Assert.DoesNotContain(result.Issues, i => i.Code == "SCSS108");
    }

    [Fact]
    public void AnalyzeContent_WithAdjacentSiblingSelector_ShouldCountCorrectly()
    {
        // Arrange - 5 parts with adjacent sibling combinators
        var scss = @"
.a + .b + .c + .d + .e {
    margin: 0;
}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        Assert.Contains(result.Issues, i => i.Code == "SCSS108");
    }

    [Fact]
    public void AnalyzeContent_WithGeneralSiblingSelector_ShouldCountCorrectly()
    {
        // Arrange - 5 parts with general sibling combinators
        var scss = @"
.a ~ .b ~ .c ~ .d ~ .e {
    display: block;
}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        Assert.Contains(result.Issues, i => i.Code == "SCSS108");
    }

    #endregion
}
