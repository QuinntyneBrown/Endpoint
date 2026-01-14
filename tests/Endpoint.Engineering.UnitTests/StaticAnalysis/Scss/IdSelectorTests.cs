// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.Scss;

/// <summary>
/// Acceptance tests for SCSS107 - No ID selectors.
/// </summary>
public class IdSelectorTests
{
    private readonly IScssStaticAnalysisService _sut;

    public IdSelectorTests()
    {
        var mockLogger = new Mock<ILogger<ScssStaticAnalysisService>>();
        _sut = new ScssStaticAnalysisService(mockLogger.Object);
    }

    #region SCSS107 - ID Selector Violation Tests

    [Fact]
    public void AnalyzeContent_WithIdSelector_ShouldReportInfo()
    {
        // Arrange
        var scss = @"
#header {
    background: blue;
}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        Assert.True(result.HasIssues);
        var issue = Assert.Single(result.Issues.Where(i => i.Code == "SCSS107"));
        Assert.Equal(IssueSeverity.Info, issue.Severity);
        Assert.Contains("#header", issue.Message);
    }

    [Fact]
    public void AnalyzeContent_WithMultipleIdSelectors_ShouldReportMultipleInfos()
    {
        // Arrange
        var scss = @"
#header {
    background: blue;
}

#footer {
    background: gray;
}

#sidebar {
    width: 200px;
}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        var idIssues = result.Issues.Where(i => i.Code == "SCSS107").ToList();
        Assert.Equal(3, idIssues.Count);
    }

    [Fact]
    public void AnalyzeContent_WithIdInCombinedSelector_ShouldReportInfo()
    {
        // Arrange
        var scss = @"
.container #main-content {
    padding: 20px;
}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        Assert.Contains(result.Issues, i => i.Code == "SCSS107");
    }

    [Fact]
    public void AnalyzeContent_WithClassSelectorsOnly_ShouldNotReportInfo()
    {
        // Arrange
        var scss = @"
.header {
    background: blue;
}

.footer {
    background: gray;
}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        Assert.DoesNotContain(result.Issues, i => i.Code == "SCSS107");
    }

    [Fact]
    public void AnalyzeContent_WithScssInterpolation_ShouldNotReportFalsePositive()
    {
        // Arrange - #{$var} is SCSS interpolation, not an ID selector
        var scss = @"
$prefix: 'btn';

.#{$prefix}-primary {
    color: blue;
}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        Assert.DoesNotContain(result.Issues, i => i.Code == "SCSS107" && i.SourceSnippet?.Contains("#{") == true);
    }

    [Fact]
    public void AnalyzeContent_WithHexColorInProperty_ShouldNotReportAsIdSelector()
    {
        // Arrange
        var scss = @"
.button {
    color: #ff0000;
    background: #abc123;
}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        // Hex colors should not be flagged as ID selectors
        var idIssues = result.Issues.Where(i => i.Code == "SCSS107").ToList();
        Assert.Empty(idIssues);
    }

    #endregion
}
