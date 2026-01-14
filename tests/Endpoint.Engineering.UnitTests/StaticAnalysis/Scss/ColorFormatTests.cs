// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.Scss;

/// <summary>
/// Acceptance tests for SCSS103 - Color format consistency and SCSS104 - Short hex colors.
/// </summary>
public class ColorFormatTests
{
    private readonly IScssStaticAnalysisService _sut;

    public ColorFormatTests()
    {
        var mockLogger = new Mock<ILogger<ScssStaticAnalysisService>>();
        _sut = new ScssStaticAnalysisService(mockLogger.Object);
    }

    #region SCSS103 - Color Format Consistency Tests

    [Fact]
    public void AnalyzeContent_WithMixedColorFormats_ShouldReportInfo()
    {
        // Arrange
        var scss = @"
.mixed-colors {
    color: #ff0000;
    background: rgb(0, 255, 0);
    border-color: blue;
}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        Assert.Contains(result.Issues, i => i.Code == "SCSS103" && i.Severity == IssueSeverity.Info);
    }

    [Fact]
    public void AnalyzeContent_WithOnlyHexColors_ShouldNotReportInconsistency()
    {
        // Arrange
        var scss = @"
.hex-only {
    color: #ff0000;
    background: #00ff00;
    border-color: #0000ff;
}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        Assert.DoesNotContain(result.Issues, i => i.Code == "SCSS103");
    }

    [Fact]
    public void AnalyzeContent_WithOnlyRgbColors_ShouldNotReportInconsistency()
    {
        // Arrange
        var scss = @"
.rgb-only {
    color: rgb(255, 0, 0);
    background: rgba(0, 255, 0, 0.5);
    border-color: rgb(0, 0, 255);
}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        Assert.DoesNotContain(result.Issues, i => i.Code == "SCSS103");
    }

    [Fact]
    public void AnalyzeContent_WithOnlyNamedColors_ShouldNotReportInconsistency()
    {
        // Arrange
        var scss = @"
.named-only {
    color: red;
    background: green;
    border-color: blue;
}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        Assert.DoesNotContain(result.Issues, i => i.Code == "SCSS103");
    }

    #endregion

    #region SCSS104 - Short Hex Color Tests

    [Fact]
    public void AnalyzeContent_WithShortHexColor_ShouldReportInfo()
    {
        // Arrange
        var scss = @"
.short-hex {
    color: #fff;
    background: #000;
}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        Assert.Contains(result.Issues, i => i.Code == "SCSS104" && i.Severity == IssueSeverity.Info);
    }

    [Fact]
    public void AnalyzeContent_WithMultipleShortHexColors_ShouldReportMultipleInfos()
    {
        // Arrange
        var scss = @"
.multiple-short {
    color: #abc;
    background: #def;
    border: 1px solid #123;
}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        var shortHexIssues = result.Issues.Where(i => i.Code == "SCSS104").ToList();
        Assert.Equal(3, shortHexIssues.Count);
    }

    [Fact]
    public void AnalyzeContent_WithFullHexColor_ShouldNotReportShortHexWarning()
    {
        // Arrange
        var scss = @"
.full-hex {
    color: #ffffff;
    background: #000000;
}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        Assert.DoesNotContain(result.Issues, i => i.Code == "SCSS104");
    }

    #endregion
}
