// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.AI.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Endpoint.Engineering.UnitTests.AI.Services;

/// <summary>
/// Unit tests for HtmlParserService stripping levels.
/// </summary>
public class HtmlParserServiceTests
{
    private readonly Mock<ILogger<HtmlParserService>> _mockLogger;
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly HtmlParserService _sut;

    public HtmlParserServiceTests()
    {
        _mockLogger = new Mock<ILogger<HtmlParserService>>();
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _sut = new HtmlParserService(_mockLogger.Object, _mockHttpClientFactory.Object);
    }

    [Fact]
    public void ParseHtml_WithStripLevel0_PerformsSemanticExtraction()
    {
        // Arrange
        var html = @"
            <!DOCTYPE html>
            <html>
            <head>
                <title>Test Page</title>
                <style>.test { color: blue; }</style>
                <script>alert('test');</script>
            </head>
            <body>
                <h1>Welcome</h1>
                <p class='intro' style='color: red;'>This is a test paragraph.</p>
            </body>
            </html>";

        // Act
        var result = _sut.ParseHtml(html, stripLevel: 0);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("Welcome", result);
        Assert.Contains("test paragraph", result);
        Assert.DoesNotContain("<script>", result);
        Assert.DoesNotContain("<style>", result);
    }

    [Fact]
    public void ParseHtml_WithStripLevel10_ReturnsBasicHtmlOnly()
    {
        // Arrange
        var html = @"
            <!DOCTYPE html>
            <html>
            <head>
                <title>Test Page</title>
                <style>.test { color: blue; }</style>
            </head>
            <body>
                <div id='container' class='main' style='background: white;'>
                    <h1 onclick='test()'>Welcome</h1>
                    <p class='intro' style='color: red;'>This is a <strong>test</strong> paragraph.</p>
                </div>
            </body>
            </html>";

        // Act
        var result = _sut.ParseHtml(html, stripLevel: 10);

        // Assert
        Assert.NotNull(result);
        // Should have tags but no attributes
        Assert.Contains("<h1>", result);
        Assert.Contains("<p>", result);
        Assert.Contains("<strong>", result);
        // Should not have any attributes
        Assert.DoesNotContain("class=", result);
        Assert.DoesNotContain("id=", result);
        Assert.DoesNotContain("style=", result);
        Assert.DoesNotContain("onclick=", result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(10)]
    public void ParseHtml_WithValidStripLevel_DoesNotThrow(int stripLevel)
    {
        // Arrange
        var html = "<html><body><h1>Test</h1></body></html>";

        // Act
        var exception = Record.Exception(() => _sut.ParseHtml(html, stripLevel));

        // Assert
        Assert.Null(exception);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(11)]
    [InlineData(100)]
    public void ParseHtml_WithInvalidStripLevel_ThrowsArgumentOutOfRangeException(int stripLevel)
    {
        // Arrange
        var html = "<html><body><h1>Test</h1></body></html>";

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => _sut.ParseHtml(html, stripLevel));
    }

    [Fact]
    public void ParseHtml_WithStripLevel10_RemovesNonBasicTags()
    {
        // Arrange
        var html = @"
            <html>
            <body>
                <header>Header Content</header>
                <nav>Navigation</nav>
                <section>
                    <h1>Title</h1>
                    <p>Paragraph</p>
                </section>
                <footer>Footer</footer>
            </body>
            </html>";

        // Act
        var result = _sut.ParseHtml(html, stripLevel: 10);

        // Assert
        Assert.NotNull(result);
        // Basic tags should remain
        Assert.Contains("<h1>", result);
        Assert.Contains("<p>", result);
        // Non-basic tags should be removed but content preserved
        Assert.DoesNotContain("<header>", result);
        Assert.DoesNotContain("<nav>", result);
        Assert.DoesNotContain("<section>", result);
        Assert.DoesNotContain("<footer>", result);
    }

    [Fact]
    public void ParseHtml_WithStripLevel10_PreservesBasicTextFormatting()
    {
        // Arrange
        var html = @"
            <html>
            <body>
                <p>This is <strong>bold</strong> and <em>italic</em> text.</p>
            </body>
            </html>";

        // Act
        var result = _sut.ParseHtml(html, stripLevel: 10);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("<strong>", result);
        Assert.Contains("<em>", result);
        Assert.Contains("bold", result);
        Assert.Contains("italic", result);
    }

    [Fact]
    public void ParseHtml_WithStripLevel10_PreservesListsAndTables()
    {
        // Arrange
        var html = @"
            <html>
            <body>
                <ul>
                    <li>Item 1</li>
                    <li>Item 2</li>
                </ul>
                <table>
                    <tr>
                        <td>Cell</td>
                    </tr>
                </table>
            </body>
            </html>";

        // Act
        var result = _sut.ParseHtml(html, stripLevel: 10);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("<ul>", result);
        Assert.Contains("<li>", result);
        Assert.Contains("<table>", result);
        Assert.Contains("<tr>", result);
        Assert.Contains("<td>", result);
    }

    [Fact]
    public void ParseHtml_WithNullHtml_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _sut.ParseHtml(null!, stripLevel: 0));
    }

    [Fact]
    public void ParseHtml_WithEmptyHtml_ReturnsEmptyString()
    {
        // Arrange
        var html = "";

        // Act
        var result = _sut.ParseHtml(html, stripLevel: 0);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Trim());
    }
}
