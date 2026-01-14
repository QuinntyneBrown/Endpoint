// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.Scss;

/// <summary>
/// Acceptance tests for SCSS112 - Vendor prefixes.
/// </summary>
public class VendorPrefixTests
{
    private readonly IScssStaticAnalysisService _sut;

    public VendorPrefixTests()
    {
        var mockLogger = new Mock<ILogger<ScssStaticAnalysisService>>();
        _sut = new ScssStaticAnalysisService(mockLogger.Object);
    }

    #region SCSS112 - Vendor Prefix Violation Tests

    [Fact]
    public void AnalyzeContent_WithWebkitPrefix_ShouldReportInfo()
    {
        // Arrange
        var scss = @"
.element {
    -webkit-transform: rotate(45deg);
}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        Assert.True(result.HasIssues);
        var issue = Assert.Single(result.Issues.Where(i => i.Code == "SCSS112"));
        Assert.Equal(IssueSeverity.Info, issue.Severity);
    }

    [Fact]
    public void AnalyzeContent_WithMozPrefix_ShouldReportInfo()
    {
        // Arrange
        var scss = @"
.element {
    -moz-transform: rotate(45deg);
}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        Assert.Contains(result.Issues, i => i.Code == "SCSS112");
    }

    [Fact]
    public void AnalyzeContent_WithMsPrefix_ShouldReportInfo()
    {
        // Arrange
        var scss = @"
.element {
    -ms-transform: rotate(45deg);
}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        Assert.Contains(result.Issues, i => i.Code == "SCSS112");
    }

    [Fact]
    public void AnalyzeContent_WithOPrefix_ShouldReportInfo()
    {
        // Arrange
        var scss = @"
.element {
    -o-transform: rotate(45deg);
}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        Assert.Contains(result.Issues, i => i.Code == "SCSS112");
    }

    [Fact]
    public void AnalyzeContent_WithMultipleVendorPrefixes_ShouldReportMultipleInfos()
    {
        // Arrange
        var scss = @"
.element {
    -webkit-transform: rotate(45deg);
    -moz-transform: rotate(45deg);
    -ms-transform: rotate(45deg);
    -o-transform: rotate(45deg);
    transform: rotate(45deg);
}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        var vendorPrefixIssues = result.Issues.Where(i => i.Code == "SCSS112").ToList();
        Assert.Equal(4, vendorPrefixIssues.Count);
    }

    [Fact]
    public void AnalyzeContent_WithoutVendorPrefixes_ShouldNotReportInfo()
    {
        // Arrange
        var scss = @"
.element {
    transform: rotate(45deg);
    display: flex;
    grid-template-columns: 1fr 1fr;
}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        Assert.DoesNotContain(result.Issues, i => i.Code == "SCSS112");
    }

    [Fact]
    public void AnalyzeContent_WithWebkitAnimation_ShouldReportInfo()
    {
        // Arrange
        var scss = @"
.animated {
    -webkit-animation: fadeIn 1s ease-in;
}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        Assert.Contains(result.Issues, i => i.Code == "SCSS112");
    }

    [Fact]
    public void AnalyzeContent_WithWebkitKeyframes_ShouldReportInfo()
    {
        // Arrange
        var scss = @"
@-webkit-keyframes fadeIn {
    from { opacity: 0; }
    to { opacity: 1; }
}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        Assert.Contains(result.Issues, i => i.Code == "SCSS112");
    }

    [Theory]
    [InlineData("-webkit-box-shadow")]
    [InlineData("-moz-box-shadow")]
    [InlineData("-webkit-transition")]
    [InlineData("-moz-transition")]
    [InlineData("-webkit-border-radius")]
    [InlineData("-moz-border-radius")]
    public void AnalyzeContent_WithCommonVendorPrefixedProperties_ShouldReportInfo(string property)
    {
        // Arrange
        var scss = $@"
.element {{
    {property}: 5px;
}}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        Assert.Contains(result.Issues, i => i.Code == "SCSS112");
    }

    #endregion
}
