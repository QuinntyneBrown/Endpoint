// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.ALaCarte;
using Xunit;

namespace Endpoint.Engineering.UnitTests.ALaCarte;

/// <summary>
/// Unit tests for GitUrlParser.
/// </summary>
public class GitUrlParserTests
{
    [Theory]
    [InlineData(
        "https://github.com/owner/repo/tree/main/src/folder",
        "https://github.com/owner/repo",
        "main",
        "src/folder")]
    [InlineData(
        "https://github.com/owner/repo/tree/develop/path/to/project",
        "https://github.com/owner/repo",
        "develop",
        "path/to/project")]
    [InlineData(
        "https://github.com/owner/repo/tree/feature/branch-name/folder",
        "https://github.com/owner/repo",
        "feature/branch-name",
        "folder")]
    [InlineData(
        "https://github.com/owner/repo/tree/main",
        "https://github.com/owner/repo",
        "main",
        "")]
    public void Parse_GitHubUrl_ReturnsCorrectComponents(
        string url,
        string expectedRepoUrl,
        string expectedBranch,
        string expectedFolderPath)
    {
        // Act
        var result = GitUrlParser.Parse(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedRepoUrl, result.Value.RepositoryUrl);
        Assert.Equal(expectedBranch, result.Value.Branch);
        Assert.Equal(expectedFolderPath, result.Value.FolderPath);
    }

    [Theory]
    [InlineData(
        "https://gitlab.com/owner/repo/-/tree/main/src/folder",
        "https://gitlab.com/owner/repo",
        "main",
        "src/folder")]
    [InlineData(
        "https://gitlab.com/owner/repo/-/tree/develop/path/to/project",
        "https://gitlab.com/owner/repo",
        "develop",
        "path/to/project")]
    [InlineData(
        "https://gitlab.com/owner/repo/-/tree/main",
        "https://gitlab.com/owner/repo",
        "main",
        "")]
    public void Parse_GitLabUrl_ReturnsCorrectComponents(
        string url,
        string expectedRepoUrl,
        string expectedBranch,
        string expectedFolderPath)
    {
        // Act
        var result = GitUrlParser.Parse(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedRepoUrl, result.Value.RepositoryUrl);
        Assert.Equal(expectedBranch, result.Value.Branch);
        Assert.Equal(expectedFolderPath, result.Value.FolderPath);
    }

    [Theory]
    [InlineData(
        "https://gitlab.example.com/owner/repo/-/tree/main/src/folder",
        "https://gitlab.example.com/owner/repo",
        "main",
        "src/folder")]
    public void Parse_SelfHostedGitLabUrl_ReturnsCorrectComponents(
        string url,
        string expectedRepoUrl,
        string expectedBranch,
        string expectedFolderPath)
    {
        // Act
        var result = GitUrlParser.Parse(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedRepoUrl, result.Value.RepositoryUrl);
        Assert.Equal(expectedBranch, result.Value.Branch);
        Assert.Equal(expectedFolderPath, result.Value.FolderPath);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("https://github.com/owner/repo")]
    [InlineData("https://github.com/owner/repo/blob/main/file.cs")]
    [InlineData("https://example.com/some/path")]
    [InlineData("not a url")]
    public void Parse_InvalidUrl_ReturnsNull(string? url)
    {
        // Act
        var result = GitUrlParser.Parse(url!);

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [InlineData("https://github.com/owner/repo/tree/main/src/folder", true)]
    [InlineData("https://gitlab.com/owner/repo/-/tree/main/src/folder", true)]
    [InlineData("https://github.com/owner/repo", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsValidGitUrl_ReturnsExpectedResult(string? url, bool expected)
    {
        // Act
        var result = GitUrlParser.IsValidGitUrl(url!);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Parse_GitHubUrlWithHttpProtocol_ReturnsCorrectComponents()
    {
        // Arrange
        var url = "http://github.com/owner/repo/tree/main/src/folder";

        // Act
        var result = GitUrlParser.Parse(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("http://github.com/owner/repo", result.Value.RepositoryUrl);
        Assert.Equal("main", result.Value.Branch);
        Assert.Equal("src/folder", result.Value.FolderPath);
    }

    [Fact]
    public void Parse_GitLabUrlWithHttpProtocol_ReturnsCorrectComponents()
    {
        // Arrange
        var url = "http://gitlab.com/owner/repo/-/tree/main/src/folder";

        // Act
        var result = GitUrlParser.Parse(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("http://gitlab.com/owner/repo", result.Value.RepositoryUrl);
        Assert.Equal("main", result.Value.Branch);
        Assert.Equal("src/folder", result.Value.FolderPath);
    }
}
