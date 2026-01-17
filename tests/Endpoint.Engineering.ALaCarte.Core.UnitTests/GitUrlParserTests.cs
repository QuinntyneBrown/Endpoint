// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.ALaCarte.Core;

namespace Endpoint.Engineering.ALaCarte.Core.UnitTests;

public class GitUrlParserTests
{
    #region Parse - Null and Empty Input Tests

    [Fact]
    public void Parse_WithNullUrl_ReturnsNull()
    {
        // Act
        var result = GitUrlParser.Parse(null!);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Parse_WithEmptyUrl_ReturnsNull()
    {
        // Act
        var result = GitUrlParser.Parse(string.Empty);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Parse_WithWhitespaceUrl_ReturnsNull()
    {
        // Act
        var result = GitUrlParser.Parse("   ");

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region Parse - GitHub URL Tests

    [Fact]
    public void Parse_GitHubUrl_WithBranchAndPath_ReturnsCorrectValues()
    {
        // Arrange
        var url = "https://github.com/owner/repo/tree/main/path/to/folder";

        // Act
        var result = GitUrlParser.Parse(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("https://github.com/owner/repo", result.Value.RepositoryUrl);
        Assert.Equal("main", result.Value.Branch);
        Assert.Equal("path/to/folder", result.Value.FolderPath);
    }

    [Fact]
    public void Parse_GitHubUrl_WithBranchOnly_ReturnsEmptyPath()
    {
        // Arrange
        var url = "https://github.com/owner/repo/tree/main";

        // Act
        var result = GitUrlParser.Parse(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("https://github.com/owner/repo", result.Value.RepositoryUrl);
        Assert.Equal("main", result.Value.Branch);
        Assert.Equal(string.Empty, result.Value.FolderPath);
    }

    [Fact]
    public void Parse_GitHubUrl_WithFeatureBranch_ReturnsCorrectBranch()
    {
        // Arrange
        var url = "https://github.com/owner/repo/tree/feature/my-feature/src/folder";

        // Act
        var result = GitUrlParser.Parse(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("https://github.com/owner/repo", result.Value.RepositoryUrl);
        Assert.Equal("feature/my-feature", result.Value.Branch);
        Assert.Equal("src/folder", result.Value.FolderPath);
    }

    [Fact]
    public void Parse_GitHubUrl_WithBugfixBranch_ReturnsCorrectBranch()
    {
        // Arrange
        var url = "https://github.com/owner/repo/tree/bugfix/fix-issue/src";

        // Act
        var result = GitUrlParser.Parse(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("bugfix/fix-issue", result.Value.Branch);
        Assert.Equal("src", result.Value.FolderPath);
    }

    [Fact]
    public void Parse_GitHubUrl_WithHotfixBranch_ReturnsCorrectBranch()
    {
        // Arrange
        var url = "https://github.com/owner/repo/tree/hotfix/critical-fix/lib";

        // Act
        var result = GitUrlParser.Parse(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("hotfix/critical-fix", result.Value.Branch);
        Assert.Equal("lib", result.Value.FolderPath);
    }

    [Fact]
    public void Parse_GitHubUrl_WithReleaseBranch_ReturnsCorrectBranch()
    {
        // Arrange
        var url = "https://github.com/owner/repo/tree/release/v1.0/docs";

        // Act
        var result = GitUrlParser.Parse(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("release/v1.0", result.Value.Branch);
        Assert.Equal("docs", result.Value.FolderPath);
    }

    [Fact]
    public void Parse_GitHubUrl_WithHttpProtocol_ReturnsCorrectValues()
    {
        // Arrange
        var url = "http://github.com/owner/repo/tree/main/src";

        // Act
        var result = GitUrlParser.Parse(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("http://github.com/owner/repo", result.Value.RepositoryUrl);
    }

    #endregion

    #region Parse - GitLab URL Tests

    [Fact]
    public void Parse_GitLabUrl_WithBranchAndPath_ReturnsCorrectValues()
    {
        // Arrange
        var url = "https://gitlab.com/owner/repo/-/tree/main/src/components";

        // Act
        var result = GitUrlParser.Parse(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("https://gitlab.com/owner/repo", result.Value.RepositoryUrl);
        Assert.Equal("main", result.Value.Branch);
        Assert.Equal("src/components", result.Value.FolderPath);
    }

    [Fact]
    public void Parse_GitLabUrl_WithSubowner_ReturnsCorrectValues()
    {
        // Arrange
        var url = "https://gitlab.com/owner/subowner/repo/-/tree/develop/lib";

        // Act
        var result = GitUrlParser.Parse(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("https://gitlab.com/owner/subowner/repo", result.Value.RepositoryUrl);
        Assert.Equal("develop", result.Value.Branch);
        Assert.Equal("lib", result.Value.FolderPath);
    }

    [Fact]
    public void Parse_GitLabUrl_WithQueryString_StripsQueryString()
    {
        // Arrange
        var url = "https://gitlab.com/owner/repo/-/tree/main/src?ref_type=heads";

        // Act
        var result = GitUrlParser.Parse(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("main", result.Value.Branch);
        Assert.Equal("src", result.Value.FolderPath);
    }

    #endregion

    #region Parse - Bitbucket Cloud URL Tests

    [Fact]
    public void Parse_BitbucketCloudUrl_WithBranchAndPath_ReturnsCorrectValues()
    {
        // Arrange
        var url = "https://bitbucket.org/owner/repo/src/main/src/app";

        // Act
        var result = GitUrlParser.Parse(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("https://bitbucket.org/owner/repo", result.Value.RepositoryUrl);
        Assert.Equal("main", result.Value.Branch);
        Assert.Equal("src/app", result.Value.FolderPath);
    }

    [Fact]
    public void Parse_BitbucketCloudUrl_WithFeatureBranch_ReturnsCorrectValues()
    {
        // Arrange
        var url = "https://bitbucket.org/owner/repo/src/feature/new-feature/lib";

        // Act
        var result = GitUrlParser.Parse(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("feature/new-feature", result.Value.Branch);
        Assert.Equal("lib", result.Value.FolderPath);
    }

    #endregion

    #region Parse - Bitbucket Server URL Tests

    [Fact]
    public void Parse_BitbucketServerUrl_WithBrowseAndBranch_ReturnsCorrectValues()
    {
        // Arrange
        var url = "https://bitbucket.company.com/projects/PROJECT/repos/repo/browse/src/main?at=refs/heads/develop";

        // Act
        var result = GitUrlParser.Parse(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("https://bitbucket.company.com/projects/PROJECT/repos/repo", result.Value.RepositoryUrl);
        Assert.Equal("develop", result.Value.Branch);
        Assert.Equal("src/main", result.Value.FolderPath);
    }

    [Fact]
    public void Parse_BitbucketServerUrl_WithoutBranch_DefaultsToMain()
    {
        // Arrange
        var url = "https://bitbucket.company.com/projects/PROJECT/repos/repo/browse/src";

        // Act
        var result = GitUrlParser.Parse(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("main", result.Value.Branch);
        Assert.Equal("src", result.Value.FolderPath);
    }

    [Fact]
    public void Parse_BitbucketServerUrl_WithBranchWithoutRefsHeads_ReturnsCorrectBranch()
    {
        // Arrange
        var url = "https://bitbucket.company.com/projects/PROJECT/repos/repo/browse?at=feature-branch";

        // Act
        var result = GitUrlParser.Parse(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("feature-branch", result.Value.Branch);
    }

    #endregion

    #region Parse - Azure DevOps URL Tests

    [Fact]
    public void Parse_AzureDevOpsUrl_WithPathAndVersion_ReturnsCorrectValues()
    {
        // Arrange
        var url = "https://dev.azure.com/org/project/_git/repo?path=/src/app&version=GBmain";

        // Act
        var result = GitUrlParser.Parse(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("https://dev.azure.com/org/project/_git/repo", result.Value.RepositoryUrl);
        Assert.Equal("main", result.Value.Branch);
        Assert.Equal("src/app", result.Value.FolderPath);
    }

    [Fact]
    public void Parse_AzureDevOpsUrl_WithoutQueryParams_DefaultsToMain()
    {
        // Arrange
        var url = "https://dev.azure.com/org/project/_git/repo";

        // Act
        var result = GitUrlParser.Parse(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("main", result.Value.Branch);
        Assert.Equal(string.Empty, result.Value.FolderPath);
    }

    [Fact]
    public void Parse_VSTSUrl_ReturnsCorrectValues()
    {
        // Arrange
        var url = "https://org.visualstudio.com/project/_git/repo?path=/lib&version=GBdevelop";

        // Act
        var result = GitUrlParser.Parse(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("https://org.visualstudio.com/project/_git/repo", result.Value.RepositoryUrl);
        Assert.Equal("develop", result.Value.Branch);
        Assert.Equal("lib", result.Value.FolderPath);
    }

    [Fact]
    public void Parse_AzureDevOpsUrl_WithVersionWithoutGBPrefix_UsesVersionAsIs()
    {
        // Arrange
        var url = "https://dev.azure.com/org/project/_git/repo?version=customBranch";

        // Act
        var result = GitUrlParser.Parse(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("customBranch", result.Value.Branch);
    }

    #endregion

    #region Parse - Gitea/Gogs URL Tests

    [Fact]
    public void Parse_GiteaUrl_WithBranchAndPath_ReturnsCorrectValues()
    {
        // Arrange
        var url = "https://gitea.company.com/owner/repo/src/branch/main/src/components";

        // Act
        var result = GitUrlParser.Parse(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("https://gitea.company.com/owner/repo", result.Value.RepositoryUrl);
        Assert.Equal("main", result.Value.Branch);
        Assert.Equal("src/components", result.Value.FolderPath);
    }

    [Fact]
    public void Parse_GiteaUrl_WithFeatureBranch_ReturnsCorrectValues()
    {
        // Arrange
        var url = "https://gogs.internal.com/team/project/src/branch/feature/cool-feature/lib";

        // Act
        var result = GitUrlParser.Parse(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("feature/cool-feature", result.Value.Branch);
        Assert.Equal("lib", result.Value.FolderPath);
    }

    #endregion

    #region Parse - Self-Hosted GitLab URL Tests

    [Fact]
    public void Parse_SelfHostedGitLabUrl_WithBranchAndPath_ReturnsCorrectValues()
    {
        // Arrange
        var url = "https://git.company.com/team/project/-/tree/develop/src/utils";

        // Act
        var result = GitUrlParser.Parse(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("https://git.company.com/team/project", result.Value.RepositoryUrl);
        Assert.Equal("develop", result.Value.Branch);
        Assert.Equal("src/utils", result.Value.FolderPath);
    }

    [Fact]
    public void Parse_SelfHostedGitLabUrl_WithNestedPath_ReturnsCorrectValues()
    {
        // Arrange
        var url = "https://gitlab.internal.org/group/subgroup/project/-/tree/master/packages/core";

        // Act
        var result = GitUrlParser.Parse(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("https://gitlab.internal.org/group/subgroup/project", result.Value.RepositoryUrl);
        Assert.Equal("master", result.Value.Branch);
        Assert.Equal("packages/core", result.Value.FolderPath);
    }

    #endregion

    #region Parse - Self-Hosted GitHub Enterprise URL Tests

    [Fact]
    public void Parse_GitHubEnterpriseUrl_WithBranchAndPath_ReturnsCorrectValues()
    {
        // Arrange
        var url = "https://github.company.com/engineering/api-library/tree/main/src/core";

        // Act
        var result = GitUrlParser.Parse(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("https://github.company.com/engineering/api-library", result.Value.RepositoryUrl);
        Assert.Equal("main", result.Value.Branch);
        Assert.Equal("src/core", result.Value.FolderPath);
    }

    #endregion

    #region IsValidGitUrl Tests

    [Fact]
    public void IsValidGitUrl_WithValidGitHubUrl_ReturnsTrue()
    {
        // Arrange
        var url = "https://github.com/owner/repo/tree/main/src";

        // Act
        var result = GitUrlParser.IsValidGitUrl(url);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValidGitUrl_WithValidGitLabUrl_ReturnsTrue()
    {
        // Arrange
        var url = "https://gitlab.com/owner/repo/-/tree/main/src";

        // Act
        var result = GitUrlParser.IsValidGitUrl(url);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValidGitUrl_WithInvalidUrl_ReturnsFalse()
    {
        // Arrange
        var url = "https://example.com/not-a-git-url";

        // Act
        var result = GitUrlParser.IsValidGitUrl(url);

        // Assert
        Assert.False(result);
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
    public void IsValidGitUrl_WithEmptyUrl_ReturnsFalse()
    {
        // Act
        var result = GitUrlParser.IsValidGitUrl(string.Empty);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Parse_UrlWithTrailingWhitespace_TrimsAndParses()
    {
        // Arrange
        var url = "  https://github.com/owner/repo/tree/main/src  ";

        // Act
        var result = GitUrlParser.Parse(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("https://github.com/owner/repo", result.Value.RepositoryUrl);
    }

    [Fact]
    public void Parse_UrlWithDeepNestedPath_ReturnsFullPath()
    {
        // Arrange
        var url = "https://github.com/owner/repo/tree/main/a/b/c/d/e/f";

        // Act
        var result = GitUrlParser.Parse(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("a/b/c/d/e/f", result.Value.FolderPath);
    }

    #endregion
}
