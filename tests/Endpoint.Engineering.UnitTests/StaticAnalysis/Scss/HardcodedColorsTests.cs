// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.Scss;

/// <summary>
/// Acceptance tests for SCSS111 - Hardcoded colors used multiple times.
/// </summary>
public class HardcodedColorsTests
{
    private readonly IScssStaticAnalysisService _sut;

    public HardcodedColorsTests()
    {
        var mockLogger = new Mock<ILogger<ScssStaticAnalysisService>>();
        _sut = new ScssStaticAnalysisService(mockLogger.Object);
    }

    #region SCSS111 - Hardcoded Colors Violation Tests

    [Fact]
    public void AnalyzeContent_WithRepeatedHexColor_ShouldReportInfo()
    {
        // Arrange - Same color used more than 2 times
        var scss = @"
.button {
    color: #ff0000;
}

.alert {
    background: #ff0000;
}

.error {
    border-color: #ff0000;
}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        Assert.True(result.HasIssues);
        var issue = Assert.Single(result.Issues.Where(i => i.Code == "SCSS111"));
        Assert.Equal(IssueSeverity.Info, issue.Severity);
        Assert.Contains("#ff0000", issue.Message.ToLower());
    }

    [Fact]
    public void AnalyzeContent_WithMultipleRepeatedColors_ShouldReportAllInOneInfo()
    {
        // Arrange
        var scss = @"
.a { color: #ff0000; }
.b { color: #ff0000; }
.c { color: #ff0000; }
.d { background: #00ff00; }
.e { background: #00ff00; }
.f { background: #00ff00; }";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        var issue = Assert.Single(result.Issues.Where(i => i.Code == "SCSS111"));
        Assert.Contains("#ff0000", issue.Message.ToLower());
        Assert.Contains("#00ff00", issue.Message.ToLower());
    }

    [Fact]
    public void AnalyzeContent_WithRepeatedRgbColor_ShouldReportInfo()
    {
        // Arrange
        var scss = @"
.a { color: rgb(255, 0, 0); }
.b { color: rgb(255, 0, 0); }
.c { color: rgb(255, 0, 0); }";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        Assert.Contains(result.Issues, i => i.Code == "SCSS111");
    }

    [Fact]
    public void AnalyzeContent_WithColorUsedOnlyTwice_ShouldNotReportInfo()
    {
        // Arrange - Only 2 uses shouldn't trigger warning
        var scss = @"
.button {
    color: #ff0000;
}

.alert {
    background: #ff0000;
}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        Assert.DoesNotContain(result.Issues, i => i.Code == "SCSS111");
    }

    [Fact]
    public void AnalyzeContent_WithUniqueColors_ShouldNotReportInfo()
    {
        // Arrange
        var scss = @"
.button {
    color: #ff0000;
}

.alert {
    background: #00ff00;
}

.error {
    border-color: #0000ff;
}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        Assert.DoesNotContain(result.Issues, i => i.Code == "SCSS111");
    }

    [Fact]
    public void AnalyzeContent_WithScssVariables_ShouldNotReportRepeatedUsage()
    {
        // Arrange - Using variables is the correct pattern
        var scss = @"
$primary-color: #ff0000;

.button {
    color: $primary-color;
}

.alert {
    background: $primary-color;
}

.error {
    border-color: $primary-color;
}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        // The hex color only appears once (in the variable declaration)
        Assert.DoesNotContain(result.Issues, i => i.Code == "SCSS111");
    }

    #endregion
}
