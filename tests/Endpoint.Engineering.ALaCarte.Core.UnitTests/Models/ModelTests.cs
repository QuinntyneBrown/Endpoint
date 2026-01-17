// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.ALaCarte.Core.Models;

namespace Endpoint.Engineering.ALaCarte.Core.UnitTests.Models;

public class ALaCarteRequestTests
{
    [Fact]
    public void ALaCarteRequest_DefaultValues_AreSetCorrectly()
    {
        // Arrange & Act
        var request = new ALaCarteRequest();

        // Assert
        Assert.NotNull(request.Repositories);
        Assert.Empty(request.Repositories);
        Assert.Equal(OutputType.NotSpecified, request.OutputType);
        Assert.Equal(Environment.CurrentDirectory, request.Directory);
        Assert.Equal("ALaCarte.sln", request.SolutionName);
    }

    [Fact]
    public void ALaCarteRequest_CanSetProperties()
    {
        // Arrange
        var request = new ALaCarteRequest
        {
            Directory = "/custom/path",
            SolutionName = "MyApp.sln",
            OutputType = OutputType.DotNetSolution
        };
        request.Repositories.Add(new RepositoryConfiguration());

        // Assert
        Assert.Equal("/custom/path", request.Directory);
        Assert.Equal("MyApp.sln", request.SolutionName);
        Assert.Equal(OutputType.DotNetSolution, request.OutputType);
        Assert.Single(request.Repositories);
    }
}

public class ALaCarteResultTests
{
    [Fact]
    public void ALaCarteResult_DefaultValues_AreSetCorrectly()
    {
        // Arrange & Act
        var result = new ALaCarteResult();

        // Assert
        Assert.False(result.Success);
        Assert.Equal(string.Empty, result.OutputDirectory);
        Assert.Null(result.SolutionPath);
        Assert.NotNull(result.CsprojFiles);
        Assert.Empty(result.CsprojFiles);
        Assert.NotNull(result.AngularWorkspacesCreated);
        Assert.Empty(result.AngularWorkspacesCreated);
        Assert.NotNull(result.AngularRootMappings);
        Assert.Empty(result.AngularRootMappings);
        Assert.NotNull(result.Errors);
        Assert.Empty(result.Errors);
        Assert.NotNull(result.Warnings);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void ALaCarteResult_CanAddItems()
    {
        // Arrange
        var result = new ALaCarteResult
        {
            Success = true,
            OutputDirectory = "/output",
            SolutionPath = "/output/MyApp.sln"
        };
        result.CsprojFiles.Add("/output/Project.csproj");
        result.AngularWorkspacesCreated.Add("/output/angular.json");
        result.AngularRootMappings["key"] = "value";
        result.Errors.Add("Error 1");
        result.Warnings.Add("Warning 1");

        // Assert
        Assert.True(result.Success);
        Assert.Single(result.CsprojFiles);
        Assert.Single(result.AngularWorkspacesCreated);
        Assert.Single(result.AngularRootMappings);
        Assert.Single(result.Errors);
        Assert.Single(result.Warnings);
    }
}

public class ALaCarteTakeRequestTests
{
    [Fact]
    public void ALaCarteTakeRequest_DefaultValues_AreSetCorrectly()
    {
        // Arrange & Act
        var request = new ALaCarteTakeRequest();

        // Assert
        Assert.Equal(string.Empty, request.Url);
        Assert.Equal("main", request.Branch);
        Assert.Equal(string.Empty, request.FromPath);
        Assert.Equal(Environment.CurrentDirectory, request.Directory);
        Assert.Null(request.SolutionName);
        Assert.Null(request.Root);
        Assert.Null(request.FromDirectory);
    }

    [Fact]
    public void ALaCarteTakeRequest_CanSetAllProperties()
    {
        // Arrange & Act
        var request = new ALaCarteTakeRequest
        {
            Url = "https://github.com/user/repo",
            Branch = "develop",
            FromPath = "src/lib",
            Directory = "/output",
            SolutionName = "MyLib.sln",
            Root = "packages/lib",
            FromDirectory = "/local/source"
        };

        // Assert
        Assert.Equal("https://github.com/user/repo", request.Url);
        Assert.Equal("develop", request.Branch);
        Assert.Equal("src/lib", request.FromPath);
        Assert.Equal("/output", request.Directory);
        Assert.Equal("MyLib.sln", request.SolutionName);
        Assert.Equal("packages/lib", request.Root);
        Assert.Equal("/local/source", request.FromDirectory);
    }
}

public class ALaCarteTakeResultTests
{
    [Fact]
    public void ALaCarteTakeResult_DefaultValues_AreSetCorrectly()
    {
        // Arrange & Act
        var result = new ALaCarteTakeResult();

        // Assert
        Assert.False(result.Success);
        Assert.Equal(string.Empty, result.OutputDirectory);
        Assert.Equal(string.Empty, result.CopiedFolderPath);
        Assert.Null(result.SolutionPath);
        Assert.Null(result.AngularWorkspacePath);
        Assert.NotNull(result.CsprojFiles);
        Assert.Empty(result.CsprojFiles);
        Assert.False(result.IsDotNetProject);
        Assert.False(result.IsAngularProject);
        Assert.NotNull(result.Errors);
        Assert.Empty(result.Errors);
        Assert.NotNull(result.Warnings);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void ALaCarteTakeResult_CanSetAllProperties()
    {
        // Arrange & Act
        var result = new ALaCarteTakeResult
        {
            Success = true,
            OutputDirectory = "/output",
            CopiedFolderPath = "/output/lib",
            SolutionPath = "/output/lib.sln",
            AngularWorkspacePath = "/output/angular.json",
            IsDotNetProject = true,
            IsAngularProject = true
        };
        result.CsprojFiles.Add("/output/lib/lib.csproj");
        result.Errors.Add("Error");
        result.Warnings.Add("Warning");

        // Assert
        Assert.True(result.Success);
        Assert.Equal("/output", result.OutputDirectory);
        Assert.Equal("/output/lib", result.CopiedFolderPath);
        Assert.Equal("/output/lib.sln", result.SolutionPath);
        Assert.Equal("/output/angular.json", result.AngularWorkspacePath);
        Assert.True(result.IsDotNetProject);
        Assert.True(result.IsAngularProject);
        Assert.Single(result.CsprojFiles);
        Assert.Single(result.Errors);
        Assert.Single(result.Warnings);
    }
}

public class RepositoryConfigurationTests
{
    [Fact]
    public void RepositoryConfiguration_DefaultValues_AreSetCorrectly()
    {
        // Arrange & Act
        var config = new RepositoryConfiguration();

        // Assert
        Assert.Equal(Guid.Empty, config.RepositoryConfigurationId);
        Assert.Equal(string.Empty, config.Url);
        Assert.Equal("main", config.Branch);
        Assert.Null(config.LocalDirectory);
        Assert.NotNull(config.Folders);
        Assert.Empty(config.Folders);
    }

    [Fact]
    public void RepositoryConfiguration_CanSetAllProperties()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var config = new RepositoryConfiguration
        {
            RepositoryConfigurationId = id,
            Url = "https://github.com/user/repo",
            Branch = "develop",
            LocalDirectory = "/local/path"
        };
        config.Folders.Add(new FolderConfiguration { From = "src", To = "dest" });

        // Assert
        Assert.Equal(id, config.RepositoryConfigurationId);
        Assert.Equal("https://github.com/user/repo", config.Url);
        Assert.Equal("develop", config.Branch);
        Assert.Equal("/local/path", config.LocalDirectory);
        Assert.Single(config.Folders);
    }
}

public class FolderConfigurationTests
{
    [Fact]
    public void FolderConfiguration_DefaultValues_AreSetCorrectly()
    {
        // Arrange & Act
        var config = new FolderConfiguration();

        // Assert
        Assert.Equal(string.Empty, config.From);
        Assert.Equal(string.Empty, config.To);
        Assert.Null(config.Root);
    }

    [Fact]
    public void FolderConfiguration_CanSetAllProperties()
    {
        // Arrange & Act
        var config = new FolderConfiguration
        {
            From = "source/path",
            To = "destination/path",
            Root = "custom/root"
        };

        // Assert
        Assert.Equal("source/path", config.From);
        Assert.Equal("destination/path", config.To);
        Assert.Equal("custom/root", config.Root);
    }
}

public class OutputTypeTests
{
    [Fact]
    public void OutputType_HasExpectedValues()
    {
        // Assert
        Assert.Equal(0, (int)OutputType.NotSpecified);
        Assert.Equal(1, (int)OutputType.DotNetSolution);
        Assert.Equal(2, (int)OutputType.MixDotNetSolutionWithOtherFolders);
    }

    [Theory]
    [InlineData(OutputType.NotSpecified)]
    [InlineData(OutputType.DotNetSolution)]
    [InlineData(OutputType.MixDotNetSolutionWithOtherFolders)]
    public void OutputType_CanBeAssigned(OutputType type)
    {
        // Arrange
        var request = new ALaCarteRequest();

        // Act
        request.OutputType = type;

        // Assert
        Assert.Equal(type, request.OutputType);
    }
}
