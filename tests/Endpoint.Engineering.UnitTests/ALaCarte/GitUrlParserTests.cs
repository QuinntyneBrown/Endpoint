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
    [InlineData(
        "https://git.company.com/owner/repo/-/tree/develop/path/to/folder",
        "https://git.company.com/owner/repo",
        "develop",
        "path/to/folder")]
    [InlineData(
        "https://code.internal.net/team/project/-/tree/feature/my-feature/src",
        "https://code.internal.net/team/project",
        "feature/my-feature",
        "src")]
    [InlineData(
        "https://gitscm.nike.ca/ahtletes/michael-jordan/-/tree/main/infra/data?ref_type=heads",
        "https://gitscm.nike.ca/ahtletes/michael-jordan",
        "main",
        "infra/data")]
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
    [InlineData(
        "https://github.company.com/owner/repo/tree/main/src/folder",
        "https://github.company.com/owner/repo",
        "main",
        "src/folder")]
    [InlineData(
        "https://git.enterprise.org/team/project/tree/develop/path/to/folder",
        "https://git.enterprise.org/team/project",
        "develop",
        "path/to/folder")]
    [InlineData(
        "https://code.internal.net/owner/repo/tree/feature/branch-name/src",
        "https://code.internal.net/owner/repo",
        "feature/branch-name",
        "src")]
    public void Parse_SelfHostedGitHubEnterpriseUrl_ReturnsCorrectComponents(
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
        "https://bitbucket.org/owner/repo/src/main/src/folder",
        "https://bitbucket.org/owner/repo",
        "main",
        "src/folder")]
    [InlineData(
        "https://bitbucket.org/owner/repo/src/develop/path/to/project",
        "https://bitbucket.org/owner/repo",
        "develop",
        "path/to/project")]
    [InlineData(
        "https://bitbucket.org/owner/repo/src/feature/branch-name/folder",
        "https://bitbucket.org/owner/repo",
        "feature/branch-name",
        "folder")]
    public void Parse_BitbucketCloudUrl_ReturnsCorrectComponents(
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
        "https://bitbucket.company.com/projects/PROJ/repos/myrepo/browse/src/folder?at=refs/heads/main",
        "https://bitbucket.company.com/projects/PROJ/repos/myrepo",
        "main",
        "src/folder")]
    [InlineData(
        "https://bitbucket.company.com/projects/TEAM/repos/app/browse?at=refs/heads/develop",
        "https://bitbucket.company.com/projects/TEAM/repos/app",
        "develop",
        "")]
    [InlineData(
        "https://stash.internal.net/projects/DEV/repos/service/browse/path/to/folder?at=feature/my-feature",
        "https://stash.internal.net/projects/DEV/repos/service",
        "feature/my-feature",
        "path/to/folder")]
    public void Parse_BitbucketServerUrl_ReturnsCorrectComponents(
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
        "https://dev.azure.com/myorg/myproject/_git/myrepo?path=/src/folder&version=GBmain",
        "https://dev.azure.com/myorg/myproject/_git/myrepo",
        "main",
        "src/folder")]
    [InlineData(
        "https://dev.azure.com/company/project/_git/repo?path=/path/to/folder&version=GBdevelop",
        "https://dev.azure.com/company/project/_git/repo",
        "develop",
        "path/to/folder")]
    [InlineData(
        "https://dev.azure.com/org/proj/_git/repo?version=GBfeature/my-branch&path=/src",
        "https://dev.azure.com/org/proj/_git/repo",
        "feature/my-branch",
        "src")]
    [InlineData(
        "https://dev.azure.com/org/proj/_git/repo",
        "https://dev.azure.com/org/proj/_git/repo",
        "main",
        "")]
    public void Parse_AzureDevOpsUrl_ReturnsCorrectComponents(
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
        "https://mycompany.visualstudio.com/myproject/_git/myrepo?path=/src/folder&version=GBmain",
        "https://mycompany.visualstudio.com/myproject/_git/myrepo",
        "main",
        "src/folder")]
    [InlineData(
        "https://company.visualstudio.com/project/_git/repo?version=GBdevelop",
        "https://company.visualstudio.com/project/_git/repo",
        "develop",
        "")]
    public void Parse_LegacyVstsUrl_ReturnsCorrectComponents(
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
        "https://gitea.company.com/owner/repo/src/branch/main/src/folder",
        "https://gitea.company.com/owner/repo",
        "main",
        "src/folder")]
    [InlineData(
        "https://gogs.internal.net/team/project/src/branch/develop/path/to/folder",
        "https://gogs.internal.net/team/project",
        "develop",
        "path/to/folder")]
    [InlineData(
        "https://git.mycompany.io/owner/repo/src/branch/feature/my-feature/src",
        "https://git.mycompany.io/owner/repo",
        "feature/my-feature",
        "src")]
    public void Parse_GiteaGogsUrl_ReturnsCorrectComponents(
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
    [InlineData("https://github.com/owner/repo")]
    [InlineData("https://github.com/owner/repo/blob/main/file.cs")]
    [InlineData("https://example.com/some/path")]
    [InlineData("not a url")]
    public void Parse_InvalidUrl_ReturnsNull(string? url)
    {
        // Act
        var result = GitUrlParser.Parse(url ?? string.Empty);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Parse_NullUrl_ReturnsNull()
    {
        // Act
        var result = GitUrlParser.Parse(null!);

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [InlineData("https://github.com/owner/repo/tree/main/src/folder", true)]
    [InlineData("https://gitlab.com/owner/repo/-/tree/main/src/folder", true)]
    [InlineData("https://bitbucket.org/owner/repo/src/main/src/folder", true)]
    [InlineData("https://dev.azure.com/org/project/_git/repo?path=/src&version=GBmain", true)]
    [InlineData("https://gitea.company.com/owner/repo/src/branch/main/folder", true)]
    [InlineData("https://github.company.com/owner/repo/tree/main/folder", true)]
    [InlineData("https://git.company.com/owner/repo/-/tree/main/folder", true)]
    [InlineData("https://bitbucket.company.com/projects/PROJ/repos/repo/browse/folder?at=main", true)]
    [InlineData("https://github.com/owner/repo", false)]
    [InlineData("", false)]
    public void IsValidGitUrl_ReturnsExpectedResult(string? url, bool expected)
    {
        // Act
        var result = GitUrlParser.IsValidGitUrl(url ?? string.Empty);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void IsValidGitUrl_WithNullUrl_ReturnsFalse()
    {
        // Act
        var result = GitUrlParser.IsValidGitUrl(null!);

        // Assert
        Assert.False(result);
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
