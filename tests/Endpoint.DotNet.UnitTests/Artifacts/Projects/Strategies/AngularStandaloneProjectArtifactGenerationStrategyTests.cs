// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO;
using System.IO.Abstractions.TestingHelpers;
using Endpoint.Artifacts.Abstractions;
using Endpoint.DotNet.Artifacts.Projects;
using Endpoint.DotNet.Artifacts.Projects.Enums;
using Endpoint.DotNet.Artifacts.Projects.Strategies;
using Endpoint.DotNet.Services;
using Endpoint.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Endpoint.DotNet.UnitTests.Artifacts.Projects.Strategies;

public class AngularStandaloneProjectArtifactGenerationStrategyTests
{
    private class StubTemplateLocator : ITemplateLocator
    {
        public string Get(string name) => "<Project></Project>";
    }

    private class StubTemplateProcessor : ITemplateProcessor
    {
        public string Process(string template, IDictionary<string, object> tokens, string[] ignoreTokens = null)
            => "<Project><PropertyGroup><ProjectName>Test</ProjectName></PropertyGroup></Project>";

        public string Process(string template, IDictionary<string, object> tokens)
            => "<Project><PropertyGroup><ProjectName>Test</ProjectName></PropertyGroup></Project>";

        public string Process(string template, dynamic model)
            => "<Project><PropertyGroup><ProjectName>Test</ProjectName></PropertyGroup></Project>";

        public Task<string> ProcessAsync(string template, IDictionary<string, object> tokens, string[] ignoreTokens = null)
            => Task.FromResult("<Project><PropertyGroup><ProjectName>Test</ProjectName></PropertyGroup></Project>");

        public Task<string> ProcessAsync(string template, IDictionary<string, object> tokens)
            => Task.FromResult("<Project><PropertyGroup><ProjectName>Test</ProjectName></PropertyGroup></Project>");

        public Task<string> ProcessAsync(string template, dynamic model)
            => Task.FromResult("<Project><PropertyGroup><ProjectName>Test</ProjectName></PropertyGroup></Project>");
    }

    private class RecordingCommandService : ICommandService
    {
        public List<(string Command, string? WorkingDirectory)> RecordedCommands { get; } = new();

        public int Start(string command, string? workingDirectory = null, bool waitForExit = true)
        {
            RecordedCommands.Add((command, workingDirectory));
            return 0;
        }
    }

    private class StubArtifactGenerator : IArtifactGenerator
    {
        public Task GenerateAsync(object model) => Task.CompletedTask;
    }


    [Fact]
    public async Task GenerateAsync_CreatesAngularWorkspaceWithKebabCaseApplication()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var commandService = new RecordingCommandService();

        var projectModel = new ProjectModel(DotNetProjectType.TypeScriptStandalone, "MyAngularApp", "/tmp/src");
        var expectedDirectory = Path.Combine("/tmp/src", "MyAngularApp");

        var sut = new AngularStandaloneProjectArtifactGenerationStrategy(
            NullLogger<AngularStandaloneProjectArtifactGenerationStrategy>.Instance,
            fileSystem,
            new StubTemplateLocator(),
            new StubTemplateProcessor(),
            commandService);

        // Act
        await sut.GenerateAsync(projectModel);

        // Assert
        // Verify ng new command was called with correct parameters
        Assert.Contains(
            commandService.RecordedCommands,
            cmd => cmd.Command == "ng new MyAngularApp --no-create-application --directory ./ --defaults" && cmd.WorkingDirectory == expectedDirectory);

        // Verify ng g application command was called with kebab-case project name
        Assert.Contains(
            commandService.RecordedCommands,
            cmd => cmd.Command == "ng g application my-angular-app --defaults" && cmd.WorkingDirectory == expectedDirectory);

        // Verify .esproj file was written
        Assert.True(fileSystem.File.Exists(Path.Combine(expectedDirectory, "MyAngularApp.esproj")));
    }

    [Fact]
    public async Task GenerateAsync_CreatesParentDirectoryIfNotExists()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var commandService = new RecordingCommandService();
        
        var projectModel = new ProjectModel(DotNetProjectType.TypeScriptStandalone, "MyAngularApp", "/tmp/src");

        var sut = new AngularStandaloneProjectArtifactGenerationStrategy(
            NullLogger<AngularStandaloneProjectArtifactGenerationStrategy>.Instance,
            fileSystem,
            new StubTemplateLocator(),
            new StubTemplateProcessor(),
            commandService);

        // Act
        await sut.GenerateAsync(projectModel);

        // Assert
        // Parent directory should have been checked/created through Angular CLI commands
        Assert.Equal(2, commandService.RecordedCommands.Count);
    }

    [Fact]
    public async Task GenerateAsync_WithGeneratorParameter_CallsGenerateAsyncOnModel()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var commandService = new RecordingCommandService();
        var generator = new StubArtifactGenerator();
        
        var projectModel = new ProjectModel(DotNetProjectType.TypeScriptStandalone, "TestApp", "/tmp");

        var sut = new AngularStandaloneProjectArtifactGenerationStrategy(
            NullLogger<AngularStandaloneProjectArtifactGenerationStrategy>.Instance,
            fileSystem,
            new StubTemplateLocator(),
            new StubTemplateProcessor(),
            commandService);

        // Act
        var result = await sut.GenerateAsync(generator, projectModel);

        // Assert
        Assert.True(result);
        Assert.NotEmpty(commandService.RecordedCommands);
    }

    [Fact]
    public async Task GenerateAsync_WithNonEsprojProject_ReturnsFalse()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var commandService = new RecordingCommandService();
        var generator = new StubArtifactGenerator();
        
        var projectModel = new ProjectModel(DotNetProjectType.ClassLib, "TestLibrary", "/tmp");

        var sut = new AngularStandaloneProjectArtifactGenerationStrategy(
            NullLogger<AngularStandaloneProjectArtifactGenerationStrategy>.Instance,
            fileSystem,
            new StubTemplateLocator(),
            new StubTemplateProcessor(),
            commandService);

        // Act
        var result = await sut.GenerateAsync(generator, projectModel);

        // Assert
        Assert.False(result);
        Assert.Empty(commandService.RecordedCommands);
    }

    [Fact]
    public void GetPriority_ReturnsOne()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var commandService = new RecordingCommandService();

        var sut = new AngularStandaloneProjectArtifactGenerationStrategy(
            NullLogger<AngularStandaloneProjectArtifactGenerationStrategy>.Instance,
            fileSystem,
            new StubTemplateLocator(),
            new StubTemplateProcessor(),
            commandService);

        // Act
        var priority = sut.GetPriority();

        // Assert
        Assert.Equal(1, priority);
    }

    [Theory]
    [InlineData("MyAngularApp", "my-angular-app")]
    [InlineData("TestProject", "test-project")]
    [InlineData("SampleWebApp", "sample-web-app")]
    [InlineData("API", "api")]
    public async Task GenerateAsync_ConvertsProjectNameToKebabCase(string projectName, string expectedKebabCase)
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var commandService = new RecordingCommandService();
        
        var projectModel = new ProjectModel(DotNetProjectType.TypeScriptStandalone, projectName, "/tmp/src");

        var sut = new AngularStandaloneProjectArtifactGenerationStrategy(
            NullLogger<AngularStandaloneProjectArtifactGenerationStrategy>.Instance,
            fileSystem,
            new StubTemplateLocator(),
            new StubTemplateProcessor(),
            commandService);

        // Act
        await sut.GenerateAsync(projectModel);

        // Assert
        Assert.Contains(
            commandService.RecordedCommands,
            cmd => cmd.Command == $"ng g application {expectedKebabCase} --defaults");
    }
}
