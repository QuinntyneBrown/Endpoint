// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.Scss;

/// <summary>
/// Acceptance tests for SCSS100 - Max nesting depth rule.
/// </summary>
public class NestingDepthTests
{
    private readonly IScssStaticAnalysisService _sut;

    public NestingDepthTests()
    {
        var mockLogger = new Mock<ILogger<ScssStaticAnalysisService>>();
        _sut = new ScssStaticAnalysisService(mockLogger.Object);
    }

    #region SCSS100 - Nesting Depth Violation Tests

    [Fact]
    public void AnalyzeContent_WithNestingDepthOf4_ShouldReportWarning()
    {
        // Arrange
        var scss = @"
.level1 {
    .level2 {
        .level3 {
            .level4 {
                color: red;
            }
        }
    }
}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        Assert.True(result.HasIssues);
        Assert.Contains(result.Issues, i => i.Code == "SCSS100" && i.Severity == IssueSeverity.Warning);
        Assert.Equal(4, result.MaxNestingDepth);
    }

    [Fact]
    public void AnalyzeContent_WithNestingDepthOf5_ShouldReportWarning()
    {
        // Arrange
        var scss = @"
.a {
    .b {
        .c {
            .d {
                .e {
                    color: blue;
                }
            }
        }
    }
}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        Assert.True(result.HasIssues);
        var issue = Assert.Single(result.Issues.Where(i => i.Code == "SCSS100"));
        Assert.Equal(IssueSeverity.Warning, issue.Severity);
        Assert.Contains("5", issue.Message);
        Assert.Equal(5, result.MaxNestingDepth);
    }

    [Fact]
    public void AnalyzeContent_WithNestingDepthOf3_ShouldNotReportWarning()
    {
        // Arrange
        var scss = @"
.level1 {
    .level2 {
        .level3 {
            color: red;
        }
    }
}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        Assert.DoesNotContain(result.Issues, i => i.Code == "SCSS100");
        Assert.Equal(3, result.MaxNestingDepth);
    }

    [Fact]
    public void AnalyzeContent_WithFlatStructure_ShouldNotReportWarning()
    {
        // Arrange
        var scss = @"
.button {
    color: blue;
}

.header {
    background: white;
}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        Assert.DoesNotContain(result.Issues, i => i.Code == "SCSS100");
        Assert.Equal(1, result.MaxNestingDepth);
    }

    [Fact]
    public void AnalyzeContent_WithMediaQueryNesting_ShouldCountNestingCorrectly()
    {
        // Arrange
        var scss = @"
.container {
    .wrapper {
        .content {
            .item {
                @media (max-width: 768px) {
                    color: red;
                }
            }
        }
    }
}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        Assert.True(result.HasIssues);
        Assert.Contains(result.Issues, i => i.Code == "SCSS100");
    }

    #endregion
}
