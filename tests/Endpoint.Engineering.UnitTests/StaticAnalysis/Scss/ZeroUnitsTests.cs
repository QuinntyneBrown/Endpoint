// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.Scss;

/// <summary>
/// Acceptance tests for SCSS110 - Unnecessary units on zero values.
/// </summary>
public class ZeroUnitsTests
{
    private readonly IScssStaticAnalysisService _sut;

    public ZeroUnitsTests()
    {
        var mockLogger = new Mock<ILogger<ScssStaticAnalysisService>>();
        _sut = new ScssStaticAnalysisService(mockLogger.Object);
    }

    #region SCSS110 - Zero Units Violation Tests

    [Fact]
    public void AnalyzeContent_WithZeroPx_ShouldReportInfo()
    {
        // Arrange
        var scss = @"
.element {
    margin: 0px;
}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        Assert.True(result.HasIssues);
        var issue = Assert.Single(result.Issues.Where(i => i.Code == "SCSS110"));
        Assert.Equal(IssueSeverity.Info, issue.Severity);
        Assert.Contains("0px", issue.SourceSnippet ?? issue.Message);
    }

    [Fact]
    public void AnalyzeContent_WithZeroEm_ShouldReportInfo()
    {
        // Arrange
        var scss = @"
.element {
    padding: 0em;
}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        Assert.Contains(result.Issues, i => i.Code == "SCSS110");
    }

    [Fact]
    public void AnalyzeContent_WithZeroRem_ShouldReportInfo()
    {
        // Arrange
        var scss = @"
.element {
    font-size: 0rem;
}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        Assert.Contains(result.Issues, i => i.Code == "SCSS110");
    }

    [Fact]
    public void AnalyzeContent_WithZeroPercent_ShouldReportInfo()
    {
        // Arrange
        var scss = @"
.element {
    width: 0%;
}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        Assert.Contains(result.Issues, i => i.Code == "SCSS110");
    }

    [Fact]
    public void AnalyzeContent_WithMultipleZeroUnits_ShouldReportMultipleInfos()
    {
        // Arrange
        var scss = @"
.element {
    margin: 0px;
    padding: 0em;
    border-width: 0rem;
}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        var zeroUnitIssues = result.Issues.Where(i => i.Code == "SCSS110").ToList();
        Assert.Equal(3, zeroUnitIssues.Count);
    }

    [Fact]
    public void AnalyzeContent_WithPlainZero_ShouldNotReportInfo()
    {
        // Arrange
        var scss = @"
.element {
    margin: 0;
    padding: 0;
}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        Assert.DoesNotContain(result.Issues, i => i.Code == "SCSS110");
    }

    [Fact]
    public void AnalyzeContent_WithNonZeroValues_ShouldNotReportInfo()
    {
        // Arrange
        var scss = @"
.element {
    margin: 10px;
    padding: 1em;
    width: 100%;
}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        Assert.DoesNotContain(result.Issues, i => i.Code == "SCSS110");
    }

    [Theory]
    [InlineData("0px")]
    [InlineData("0em")]
    [InlineData("0rem")]
    [InlineData("0%")]
    [InlineData("0pt")]
    [InlineData("0cm")]
    [InlineData("0mm")]
    [InlineData("0in")]
    [InlineData("0vw")]
    [InlineData("0vh")]
    public void AnalyzeContent_WithVariousZeroUnits_ShouldReportInfo(string zeroValue)
    {
        // Arrange
        var scss = $@"
.element {{
    margin: {zeroValue};
}}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        Assert.Contains(result.Issues, i => i.Code == "SCSS110");
    }

    #endregion
}
