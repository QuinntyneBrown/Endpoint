// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.Scss;

/// <summary>
/// Acceptance tests for SCSS105 - No @import usage (deprecated).
/// </summary>
public class ImportUsageTests
{
    private readonly IScssStaticAnalysisService _sut;

    public ImportUsageTests()
    {
        var mockLogger = new Mock<ILogger<ScssStaticAnalysisService>>();
        _sut = new ScssStaticAnalysisService(mockLogger.Object);
    }

    #region SCSS105 - Import Usage Violation Tests

    [Fact]
    public void AnalyzeContent_WithImportStatement_ShouldReportWarning()
    {
        // Arrange
        var scss = @"
@import 'variables';
@import 'mixins';

.button {
    color: red;
}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        Assert.True(result.HasIssues);
        var importIssues = result.Issues.Where(i => i.Code == "SCSS105").ToList();
        Assert.Equal(2, importIssues.Count);
        Assert.All(importIssues, i => Assert.Equal(IssueSeverity.Warning, i.Severity));
    }

    [Fact]
    public void AnalyzeContent_WithSingleImport_ShouldReportWarning()
    {
        // Arrange
        var scss = @"@import 'base';";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        var issue = Assert.Single(result.Issues.Where(i => i.Code == "SCSS105"));
        Assert.Contains("@use", issue.Message);
        Assert.Contains("@forward", issue.Message);
    }

    [Fact]
    public void AnalyzeContent_WithImportPath_ShouldReportWarning()
    {
        // Arrange
        var scss = @"@import 'components/button';";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        Assert.Contains(result.Issues, i => i.Code == "SCSS105");
    }

    [Fact]
    public void AnalyzeContent_WithUseStatement_ShouldNotReportWarning()
    {
        // Arrange
        var scss = @"
@use 'variables';
@use 'mixins' as m;

.button {
    color: red;
}";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        Assert.DoesNotContain(result.Issues, i => i.Code == "SCSS105");
    }

    [Fact]
    public void AnalyzeContent_WithForwardStatement_ShouldNotReportWarning()
    {
        // Arrange
        var scss = @"
@forward 'variables';
@forward 'mixins';";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        Assert.DoesNotContain(result.Issues, i => i.Code == "SCSS105");
    }

    [Fact]
    public void AnalyzeContent_WithCssImportUrl_ShouldNotReportWarning()
    {
        // Arrange - CSS @import with url() is valid CSS, not SCSS import
        var scss = @"@import url('https://fonts.googleapis.com/css?family=Roboto');";

        // Act
        var result = _sut.AnalyzeContent(scss);

        // Assert
        Assert.DoesNotContain(result.Issues, i => i.Code == "SCSS105");
    }

    #endregion
}
