// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Endpoint.Artifacts;
using Endpoint.Artifacts.Abstractions;
using Endpoint.DotNet.Artifacts.Files;
using Endpoint.DotNet.Artifacts.Projects;
using Endpoint.DotNet.Artifacts.Projects.Enums;
using Endpoint.DotNet.Artifacts.Projects.Services;
using Endpoint.DotNet.Syntax;
using Endpoint.DotNet.Syntax.Entities;
using Endpoint.DotNet.Syntax.VisualStudio;
using Endpoint.Services;
using FileModel = Endpoint.Artifacts.FileModel;
using IFileFactory = Endpoint.DotNet.Artifacts.Files.Factories.IFileFactory;
using ProjectModel = Endpoint.DotNet.Artifacts.Projects.ProjectModel;

namespace Endpoint.DotNet.UnitTests.Artifacts.Projects.Services;

public class ProjectServiceTests
{
    private class StubFileProvider : IFileProvider
    {
        private readonly string _returnValue;

        public StubFileProvider(string returnValue)
        {
            _returnValue = returnValue;
        }

        public string Get(string pattern, string directory, int depth = 100)
        {
            return _returnValue;
        }
    }

    private class StubArtifactGenerator : IArtifactGenerator
    {
        public Task GenerateAsync(object model) => Task.CompletedTask;
    }

    private class StubFileFactory : IFileFactory
    {
        public TemplatedFileModel CreateTemplate(string template, string name, string directory, string extension = ".cs", string filename = null!, Dictionary<string, object> tokens = null!) => null!;

        public EntityFileModel Create(EntityModel model, string directory) => null!;

        public CSharpTemplatedFileModel CreateCSharp(string template, string @namespace, string name, string directory, Dictionary<string, object> tokens = null!) => null!;

        public TemplatedFileModel LaunchSettingsJson(string projectDirectory, string projectName, int port) => null!;

        public TemplatedFileModel AppSettings(string projectDirectory, string projectName, string dbContextName) => null!;

        public FileModel CreateCSharp<T>(T classModel, string directory)
            where T : TypeDeclarationModel => null!;

        public FileModel CreateResponseBase(string directory) => null!;

        public FileModel CreateLinqExtensions(string directory) => null!;

        public FileModel CreateCoreUsings(string directory) => null!;

        public FileModel CreateDbContextInterface(string directory) => null!;

        public Task<FileModel> CreateUdpClientFactoryInterfaceAsync(string directory) => Task.FromResult<FileModel>(null!);

        public Task<FileModel> CreateUdpMessageSenderInterfaceAsync(string directory) => Task.FromResult<FileModel>(null!);

        public Task<FileModel> CreateUdpMessageReceiverInterfaceAsync(string directory) => Task.FromResult<FileModel>(null!);

        public Task<FileModel> CreateUdpMessageSenderAsync(string directory) => Task.FromResult<FileModel>(null!);

        public Task<FileModel> CreateUdpMessageReceiverAsync(string directory) => Task.FromResult<FileModel>(null!);

        public Task<FileModel> CreateUdpServiceBusConfigureServicesAsync(string directory) => Task.FromResult<FileModel>(null!);

        public Task<FileModel> CreateUdpServiceBusHostExtensionsAsync(string directory) => Task.FromResult<FileModel>(null!);

        public Task<FileModel> CreateUdpServiceBusMessageAsync(string directory) => Task.FromResult<FileModel>(null!);
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

    [Fact]
    public async Task AddToSolution_TypeScriptProject_AddsProjectEntryToSolution()
    {
        // Arrange
        var solutionPath = @"C:\TestSolution\TestSolution.sln";
        var solutionDirectory = @"C:\TestSolution";
        var projectDirectory = @"C:\TestSolution\MyAngularApp";

        var solutionContent = """
            Microsoft Visual Studio Solution File, Format Version 12.00
            # Visual Studio Version 17
            VisualStudioVersion = 17.0.31903.59
            MinimumVisualStudioVersion = 10.0.40219.1
            Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "ExistingProject", "ExistingProject\ExistingProject.csproj", "{12345678-1234-1234-1234-123456789012}"
            EndProject
            Global
            	GlobalSection(SolutionConfigurationPlatforms) = preSolution
            		Debug|Any CPU = Debug|Any CPU
            		Release|Any CPU = Release|Any CPU
            	EndGlobalSection
            	GlobalSection(ProjectConfigurationPlatforms) = postSolution
            		{12345678-1234-1234-1234-123456789012}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
            		{12345678-1234-1234-1234-123456789012}.Debug|Any CPU.Build.0 = Debug|Any CPU
            		{12345678-1234-1234-1234-123456789012}.Release|Any CPU.ActiveCfg = Release|Any CPU
            		{12345678-1234-1234-1234-123456789012}.Release|Any CPU.Build.0 = Release|Any CPU
            	EndGlobalSection
            EndGlobal
            """;

        var mockFileSystem = new MockFileSystem();
        mockFileSystem.AddFile(solutionPath, new MockFileData(solutionContent));
        mockFileSystem.AddDirectory(projectDirectory);

        var sut = new ProjectService(
            new StubArtifactGenerator(),
            new NoOpCommandService(),
            new StubFileProvider(solutionPath),
            mockFileSystem,
            new StubFileFactory());

        var model = new ProjectModel(DotNetProjectType.TypeScriptStandalone, "MyAngularApp", solutionDirectory);

        // Act
        await sut.AddToSolution(model);

        // Assert
        var resultContent = mockFileSystem.File.ReadAllText(solutionPath);

        Assert.Contains($"Project(\"{ProjectTypeGuids.TypeScriptProjectTypeGuid}\")", resultContent);
        Assert.Contains("\"MyAngularApp\"", resultContent);
        Assert.Contains("MyAngularApp\\MyAngularApp.esproj", resultContent);
        Assert.Contains("EndProject", resultContent);
    }

    [Fact]
    public async Task AddToSolution_TypeScriptProject_AddsConfigurationPlatformEntries()
    {
        // Arrange
        var solutionPath = @"C:\TestSolution\TestSolution.sln";
        var solutionDirectory = @"C:\TestSolution";
        var projectDirectory = @"C:\TestSolution\MyAngularApp";

        var solutionContent = """
            Microsoft Visual Studio Solution File, Format Version 12.00
            # Visual Studio Version 17
            VisualStudioVersion = 17.0.31903.59
            MinimumVisualStudioVersion = 10.0.40219.1
            Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "ExistingProject", "ExistingProject\ExistingProject.csproj", "{12345678-1234-1234-1234-123456789012}"
            EndProject
            Global
            	GlobalSection(SolutionConfigurationPlatforms) = preSolution
            		Debug|Any CPU = Debug|Any CPU
            		Release|Any CPU = Release|Any CPU
            	EndGlobalSection
            	GlobalSection(ProjectConfigurationPlatforms) = postSolution
            		{12345678-1234-1234-1234-123456789012}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
            		{12345678-1234-1234-1234-123456789012}.Debug|Any CPU.Build.0 = Debug|Any CPU
            		{12345678-1234-1234-1234-123456789012}.Release|Any CPU.ActiveCfg = Release|Any CPU
            		{12345678-1234-1234-1234-123456789012}.Release|Any CPU.Build.0 = Release|Any CPU
            	EndGlobalSection
            EndGlobal
            """;

        var mockFileSystem = new MockFileSystem();
        mockFileSystem.AddFile(solutionPath, new MockFileData(solutionContent));
        mockFileSystem.AddDirectory(projectDirectory);

        var sut = new ProjectService(
            new StubArtifactGenerator(),
            new NoOpCommandService(),
            new StubFileProvider(solutionPath),
            mockFileSystem,
            new StubFileFactory());

        var model = new ProjectModel(DotNetProjectType.TypeScriptStandalone, "MyAngularApp", solutionDirectory);

        // Act
        await sut.AddToSolution(model);

        // Assert
        var resultContent = mockFileSystem.File.ReadAllText(solutionPath);

        // TypeScript projects should have ActiveCfg, Build.0, and Deploy.0 entries
        Assert.Contains(".Debug|Any CPU.ActiveCfg = Debug|Any CPU", resultContent);
        Assert.Contains(".Debug|Any CPU.Build.0 = Debug|Any CPU", resultContent);
        Assert.Contains(".Debug|Any CPU.Deploy.0 = Debug|Any CPU", resultContent);
        Assert.Contains(".Release|Any CPU.ActiveCfg = Release|Any CPU", resultContent);
        Assert.Contains(".Release|Any CPU.Build.0 = Release|Any CPU", resultContent);
        Assert.Contains(".Release|Any CPU.Deploy.0 = Release|Any CPU", resultContent);
    }

    [Fact]
    public async Task AddToSolution_TypeScriptProjectInSrcFolder_AddsNestedProjectEntry()
    {
        // Arrange
        var solutionPath = @"C:\TestSolution\TestSolution.sln";
        var srcDirectory = @"C:\TestSolution\src";
        var projectDirectory = @"C:\TestSolution\src\MyAngularApp";

        var solutionContent = """
            Microsoft Visual Studio Solution File, Format Version 12.00
            # Visual Studio Version 17
            VisualStudioVersion = 17.0.31903.59
            MinimumVisualStudioVersion = 10.0.40219.1
            Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "src", "src", "{AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA}"
            EndProject
            Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "ExistingProject", "src\ExistingProject\ExistingProject.csproj", "{12345678-1234-1234-1234-123456789012}"
            EndProject
            Global
            	GlobalSection(SolutionConfigurationPlatforms) = preSolution
            		Debug|Any CPU = Debug|Any CPU
            		Release|Any CPU = Release|Any CPU
            	EndGlobalSection
            	GlobalSection(ProjectConfigurationPlatforms) = postSolution
            		{12345678-1234-1234-1234-123456789012}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
            		{12345678-1234-1234-1234-123456789012}.Debug|Any CPU.Build.0 = Debug|Any CPU
            		{12345678-1234-1234-1234-123456789012}.Release|Any CPU.ActiveCfg = Release|Any CPU
            		{12345678-1234-1234-1234-123456789012}.Release|Any CPU.Build.0 = Release|Any CPU
            	EndGlobalSection
            	GlobalSection(NestedProjects) = preSolution
            		{12345678-1234-1234-1234-123456789012} = {AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA}
            	EndGlobalSection
            EndGlobal
            """;

        var mockFileSystem = new MockFileSystem();
        mockFileSystem.AddFile(solutionPath, new MockFileData(solutionContent));
        mockFileSystem.AddDirectory(srcDirectory);
        mockFileSystem.AddDirectory(projectDirectory);

        var sut = new ProjectService(
            new StubArtifactGenerator(),
            new NoOpCommandService(),
            new StubFileProvider(solutionPath),
            mockFileSystem,
            new StubFileFactory());

        var model = new ProjectModel(DotNetProjectType.TypeScriptStandalone, "MyAngularApp", srcDirectory);

        // Act
        await sut.AddToSolution(model);

        // Assert
        var resultContent = mockFileSystem.File.ReadAllText(solutionPath);
        var resultLines = resultContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        // Check that project is added to NestedProjects section
        var nestedProjectsSection = false;
        var foundNewNestedEntry = false;
        foreach (var line in resultLines)
        {
            if (line.Contains("GlobalSection(NestedProjects)"))
            {
                nestedProjectsSection = true;
            }
            else if (nestedProjectsSection && line.Contains("EndGlobalSection"))
            {
                break;
            }
            else if (nestedProjectsSection && line.Contains("{AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA}") && !line.Contains("{12345678-1234-1234-1234-123456789012}"))
            {
                foundNewNestedEntry = true;
            }
        }

        Assert.True(foundNewNestedEntry, "TypeScript project should be nested under the 'src' solution folder");
    }

    [Fact]
    public async Task AddToSolution_TypeScriptProject_UsesDefaultConfigurationsWhenNoneExist()
    {
        // Arrange
        var solutionPath = @"C:\TestSolution\TestSolution.sln";
        var solutionDirectory = @"C:\TestSolution";
        var projectDirectory = @"C:\TestSolution\MyAngularApp";

        // Solution without configuration platforms section
        var solutionContent = """
            Microsoft Visual Studio Solution File, Format Version 12.00
            # Visual Studio Version 17
            VisualStudioVersion = 17.0.31903.59
            MinimumVisualStudioVersion = 10.0.40219.1
            Global
            	GlobalSection(ProjectConfigurationPlatforms) = postSolution
            	EndGlobalSection
            EndGlobal
            """;

        var mockFileSystem = new MockFileSystem();
        mockFileSystem.AddFile(solutionPath, new MockFileData(solutionContent));
        mockFileSystem.AddDirectory(projectDirectory);

        var sut = new ProjectService(
            new StubArtifactGenerator(),
            new NoOpCommandService(),
            new StubFileProvider(solutionPath),
            mockFileSystem,
            new StubFileFactory());

        var model = new ProjectModel(DotNetProjectType.TypeScriptStandalone, "MyAngularApp", solutionDirectory);

        // Act
        await sut.AddToSolution(model);

        // Assert
        var resultContent = mockFileSystem.File.ReadAllText(solutionPath);

        // Should use default configurations when SolutionConfigurationPlatforms is missing
        Assert.Contains(".Debug|Any CPU.ActiveCfg = Debug|Any CPU", resultContent);
        Assert.Contains(".Debug|x64.ActiveCfg = Debug|Any CPU", resultContent);
        Assert.Contains(".Debug|x86.ActiveCfg = Debug|Any CPU", resultContent);
        Assert.Contains(".Release|Any CPU.ActiveCfg = Release|Any CPU", resultContent);
        Assert.Contains(".Release|x64.ActiveCfg = Release|Any CPU", resultContent);
        Assert.Contains(".Release|x86.ActiveCfg = Release|Any CPU", resultContent);
    }

    [Fact]
    public async Task AddToSolution_TypeScriptProject_InsertsProjectEntryBeforeGlobal()
    {
        // Arrange
        var solutionPath = @"C:\TestSolution\TestSolution.sln";
        var solutionDirectory = @"C:\TestSolution";
        var projectDirectory = @"C:\TestSolution\MyAngularApp";

        var solutionContent = """
            Microsoft Visual Studio Solution File, Format Version 12.00
            # Visual Studio Version 17
            VisualStudioVersion = 17.0.31903.59
            MinimumVisualStudioVersion = 10.0.40219.1
            Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Project1", "Project1\Project1.csproj", "{11111111-1111-1111-1111-111111111111}"
            EndProject
            Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Project2", "Project2\Project2.csproj", "{22222222-2222-2222-2222-222222222222}"
            EndProject
            Global
            	GlobalSection(SolutionConfigurationPlatforms) = preSolution
            		Debug|Any CPU = Debug|Any CPU
            	EndGlobalSection
            	GlobalSection(ProjectConfigurationPlatforms) = postSolution
            	EndGlobalSection
            EndGlobal
            """;

        var mockFileSystem = new MockFileSystem();
        mockFileSystem.AddFile(solutionPath, new MockFileData(solutionContent));
        mockFileSystem.AddDirectory(projectDirectory);

        var sut = new ProjectService(
            new StubArtifactGenerator(),
            new NoOpCommandService(),
            new StubFileProvider(solutionPath),
            mockFileSystem,
            new StubFileFactory());

        var model = new ProjectModel(DotNetProjectType.TypeScriptStandalone, "MyAngularApp", solutionDirectory);

        // Act
        await sut.AddToSolution(model);

        // Assert
        var resultContent = mockFileSystem.File.ReadAllText(solutionPath);
        var resultLines = resultContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();

        // Find the position of the new project
        var newProjectIndex = resultLines.FindIndex(l => l.Contains("MyAngularApp"));
        var globalIndex = resultLines.FindIndex(l => l.Trim() == "Global");

        Assert.True(newProjectIndex > 0, "New project should be found in solution");
        Assert.True(newProjectIndex < globalIndex, "New project should be before Global section");
    }

    [Fact]
    public async Task AddToSolution_TypeScriptProject_GeneratesValidProjectGuid()
    {
        // Arrange
        var solutionPath = @"C:\TestSolution\TestSolution.sln";
        var solutionDirectory = @"C:\TestSolution";
        var projectDirectory = @"C:\TestSolution\MyAngularApp";

        var solutionContent = """
            Microsoft Visual Studio Solution File, Format Version 12.00
            # Visual Studio Version 17
            VisualStudioVersion = 17.0.31903.59
            MinimumVisualStudioVersion = 10.0.40219.1
            Global
            	GlobalSection(SolutionConfigurationPlatforms) = preSolution
            		Debug|Any CPU = Debug|Any CPU
            	EndGlobalSection
            	GlobalSection(ProjectConfigurationPlatforms) = postSolution
            	EndGlobalSection
            EndGlobal
            """;

        var mockFileSystem = new MockFileSystem();
        mockFileSystem.AddFile(solutionPath, new MockFileData(solutionContent));
        mockFileSystem.AddDirectory(projectDirectory);

        var sut = new ProjectService(
            new StubArtifactGenerator(),
            new NoOpCommandService(),
            new StubFileProvider(solutionPath),
            mockFileSystem,
            new StubFileFactory());

        var model = new ProjectModel(DotNetProjectType.TypeScriptStandalone, "MyAngularApp", solutionDirectory);

        // Act
        await sut.AddToSolution(model);

        // Assert
        var resultContent = mockFileSystem.File.ReadAllText(solutionPath);

        // Extract the project GUID from the result
        var guidPattern = new System.Text.RegularExpressions.Regex(@"\{[A-F0-9]{8}-[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{12}\}");
        var matches = guidPattern.Matches(resultContent);

        // Should find at least the TypeScript project type GUID and the generated project GUID
        Assert.True(matches.Count >= 2, "Should contain valid GUIDs");

        // Verify that the same project GUID is used in both the project entry and configuration entries
        var projectLine = resultContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault(l => l.Contains("MyAngularApp") && l.StartsWith("Project"));

        Assert.NotNull(projectLine);

        // Extract the project GUID from the project line (the last GUID in the line)
        var projectLineMatches = guidPattern.Matches(projectLine);
        Assert.True(projectLineMatches.Count >= 2, "Project line should contain type GUID and project GUID");

        var projectGuid = projectLineMatches[projectLineMatches.Count - 1].Value; // Last GUID is the project GUID
        Assert.Contains($"{projectGuid}.Debug|Any CPU.ActiveCfg", resultContent);
    }

    [Fact]
    public async Task AddToSolution_TypeScriptProjectWithNoExistingProjects_FallsBackToMinimumVisualStudioVersion()
    {
        // Arrange
        var solutionPath = @"C:\TestSolution\TestSolution.sln";
        var solutionDirectory = @"C:\TestSolution";
        var projectDirectory = @"C:\TestSolution\MyAngularApp";

        var solutionContent = """
            Microsoft Visual Studio Solution File, Format Version 12.00
            # Visual Studio Version 17
            VisualStudioVersion = 17.0.31903.59
            MinimumVisualStudioVersion = 10.0.40219.1
            Global
            	GlobalSection(SolutionConfigurationPlatforms) = preSolution
            		Debug|Any CPU = Debug|Any CPU
            	EndGlobalSection
            	GlobalSection(ProjectConfigurationPlatforms) = postSolution
            	EndGlobalSection
            EndGlobal
            """;

        var mockFileSystem = new MockFileSystem();
        mockFileSystem.AddFile(solutionPath, new MockFileData(solutionContent));
        mockFileSystem.AddDirectory(projectDirectory);

        var sut = new ProjectService(
            new StubArtifactGenerator(),
            new NoOpCommandService(),
            new StubFileProvider(solutionPath),
            mockFileSystem,
            new StubFileFactory());

        var model = new ProjectModel(DotNetProjectType.TypeScriptStandalone, "MyAngularApp", solutionDirectory);

        // Act
        await sut.AddToSolution(model);

        // Assert
        var resultContent = mockFileSystem.File.ReadAllText(solutionPath);
        var resultLines = resultContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();

        var projectIndex = resultLines.FindIndex(l => l.Contains("MyAngularApp"));
        var globalIndex = resultLines.FindIndex(l => l.Trim() == "Global");

        Assert.True(projectIndex > 0, "Project should be added to solution");
        Assert.True(projectIndex < globalIndex, "Project should be before Global section");
    }

    [Fact]
    public async Task AddToSolution_TypeScriptProject_UsesCorrectProjectTypeGuid()
    {
        // Arrange
        var solutionPath = @"C:\TestSolution\TestSolution.sln";
        var solutionDirectory = @"C:\TestSolution";
        var projectDirectory = @"C:\TestSolution\MyAngularApp";

        var solutionContent = """
            Microsoft Visual Studio Solution File, Format Version 12.00
            # Visual Studio Version 17
            VisualStudioVersion = 17.0.31903.59
            MinimumVisualStudioVersion = 10.0.40219.1
            Global
            	GlobalSection(SolutionConfigurationPlatforms) = preSolution
            		Debug|Any CPU = Debug|Any CPU
            	EndGlobalSection
            	GlobalSection(ProjectConfigurationPlatforms) = postSolution
            	EndGlobalSection
            EndGlobal
            """;

        var mockFileSystem = new MockFileSystem();
        mockFileSystem.AddFile(solutionPath, new MockFileData(solutionContent));
        mockFileSystem.AddDirectory(projectDirectory);

        var sut = new ProjectService(
            new StubArtifactGenerator(),
            new NoOpCommandService(),
            new StubFileProvider(solutionPath),
            mockFileSystem,
            new StubFileFactory());

        var model = new ProjectModel(DotNetProjectType.TypeScriptStandalone, "MyAngularApp", solutionDirectory);

        // Act
        await sut.AddToSolution(model);

        // Assert
        var resultContent = mockFileSystem.File.ReadAllText(solutionPath);

        // Verify the TypeScript project type GUID is used
        Assert.Contains($"Project(\"{ProjectTypeGuids.TypeScriptProjectTypeGuid}\")", resultContent);
        Assert.Contains("{54A90642-561A-4BB1-A94E-469ADEE60C69}", resultContent);
    }

    [Fact]
    public async Task AddToSolution_TypeScriptProject_HandlesMultipleConfigurationPlatforms()
    {
        // Arrange
        var solutionPath = @"C:\TestSolution\TestSolution.sln";
        var solutionDirectory = @"C:\TestSolution";
        var projectDirectory = @"C:\TestSolution\MyAngularApp";

        var solutionContent = """
            Microsoft Visual Studio Solution File, Format Version 12.00
            # Visual Studio Version 17
            VisualStudioVersion = 17.0.31903.59
            MinimumVisualStudioVersion = 10.0.40219.1
            Global
            	GlobalSection(SolutionConfigurationPlatforms) = preSolution
            		Debug|Any CPU = Debug|Any CPU
            		Debug|ARM = Debug|ARM
            		Debug|x64 = Debug|x64
            		Release|Any CPU = Release|Any CPU
            		Release|ARM = Release|ARM
            		Release|x64 = Release|x64
            	EndGlobalSection
            	GlobalSection(ProjectConfigurationPlatforms) = postSolution
            	EndGlobalSection
            EndGlobal
            """;

        var mockFileSystem = new MockFileSystem();
        mockFileSystem.AddFile(solutionPath, new MockFileData(solutionContent));
        mockFileSystem.AddDirectory(projectDirectory);

        var sut = new ProjectService(
            new StubArtifactGenerator(),
            new NoOpCommandService(),
            new StubFileProvider(solutionPath),
            mockFileSystem,
            new StubFileFactory());

        var model = new ProjectModel(DotNetProjectType.TypeScriptStandalone, "MyAngularApp", solutionDirectory);

        // Act
        await sut.AddToSolution(model);

        // Assert
        var resultContent = mockFileSystem.File.ReadAllText(solutionPath);

        // Verify all configurations are handled
        Assert.Contains(".Debug|Any CPU.ActiveCfg = Debug|Any CPU", resultContent);
        Assert.Contains(".Debug|ARM.ActiveCfg = Debug|Any CPU", resultContent);
        Assert.Contains(".Debug|x64.ActiveCfg = Debug|Any CPU", resultContent);
        Assert.Contains(".Release|Any CPU.ActiveCfg = Release|Any CPU", resultContent);
        Assert.Contains(".Release|ARM.ActiveCfg = Release|Any CPU", resultContent);
        Assert.Contains(".Release|x64.ActiveCfg = Release|Any CPU", resultContent);

        // TypeScript projects map to Any CPU
        Assert.Contains(".Debug|ARM.Build.0 = Debug|Any CPU", resultContent);
        Assert.Contains(".Debug|ARM.Deploy.0 = Debug|Any CPU", resultContent);
    }

    [Fact]
    public async Task AddToSolution_CSharpProject_UsesDotnetSlnCommand()
    {
        // Arrange
        var solutionPath = @"C:\TestSolution\TestSolution.sln";
        var solutionDirectory = @"C:\TestSolution";
        var projectDirectory = @"C:\TestSolution\MyProject";

        var solutionContent = """
            Microsoft Visual Studio Solution File, Format Version 12.00
            Global
            EndGlobal
            """;

        var mockFileSystem = new MockFileSystem();
        mockFileSystem.AddFile(solutionPath, new MockFileData(solutionContent));
        mockFileSystem.AddDirectory(projectDirectory);

        var recordingCommandService = new RecordingCommandService();

        var sut = new ProjectService(
            new StubArtifactGenerator(),
            recordingCommandService,
            new StubFileProvider(solutionPath),
            mockFileSystem,
            new StubFileFactory());

        var model = new ProjectModel(DotNetProjectType.ClassLib, "MyProject", solutionDirectory);

        // Act
        await sut.AddToSolution(model);

        // Assert
        Assert.Single(recordingCommandService.RecordedCommands);
        var (command, workingDirectory) = recordingCommandService.RecordedCommands[0];
        Assert.Contains("dotnet sln", command);
        Assert.Contains("add", command);
        Assert.Equal(solutionDirectory, workingDirectory);
    }

    [Fact]
    public async Task AddToSolution_TypeScriptProjectNotInSubfolder_DoesNotAddNestedProjectEntry()
    {
        // Arrange
        var solutionPath = @"C:\TestSolution\TestSolution.sln";
        var solutionDirectory = @"C:\TestSolution";
        var projectDirectory = @"C:\TestSolution\MyAngularApp";

        var solutionContent = """
            Microsoft Visual Studio Solution File, Format Version 12.00
            # Visual Studio Version 17
            VisualStudioVersion = 17.0.31903.59
            MinimumVisualStudioVersion = 10.0.40219.1
            Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "ExistingProject", "ExistingProject\ExistingProject.csproj", "{12345678-1234-1234-1234-123456789012}"
            EndProject
            Global
            	GlobalSection(SolutionConfigurationPlatforms) = preSolution
            		Debug|Any CPU = Debug|Any CPU
            	EndGlobalSection
            	GlobalSection(ProjectConfigurationPlatforms) = postSolution
            	EndGlobalSection
            	GlobalSection(NestedProjects) = preSolution
            	EndGlobalSection
            EndGlobal
            """;

        var mockFileSystem = new MockFileSystem();
        mockFileSystem.AddFile(solutionPath, new MockFileData(solutionContent));
        mockFileSystem.AddDirectory(projectDirectory);

        var sut = new ProjectService(
            new StubArtifactGenerator(),
            new NoOpCommandService(),
            new StubFileProvider(solutionPath),
            mockFileSystem,
            new StubFileFactory());

        var model = new ProjectModel(DotNetProjectType.TypeScriptStandalone, "MyAngularApp", solutionDirectory);

        // Act
        await sut.AddToSolution(model);

        // Assert
        var resultContent = mockFileSystem.File.ReadAllText(solutionPath);

        // Extract new project GUID
        var guidPattern = new System.Text.RegularExpressions.Regex(@"\{[A-F0-9]{8}-[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{12}\}");
        var projectLine = resultContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault(l => l.Contains("MyAngularApp") && l.StartsWith("Project"));

        Assert.NotNull(projectLine);

        var projectLineMatches = guidPattern.Matches(projectLine);
        var projectGuid = projectLineMatches[projectLineMatches.Count - 1].Value;

        // Check NestedProjects section doesn't contain the new project
        var lines = resultContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var inNestedSection = false;
        var foundInNestedProjects = false;

        foreach (var line in lines)
        {
            if (line.Contains("GlobalSection(NestedProjects)"))
            {
                inNestedSection = true;
            }
            else if (inNestedSection && line.Contains("EndGlobalSection"))
            {
                break;
            }
            else if (inNestedSection && line.Contains(projectGuid))
            {
                foundInNestedProjects = true;
            }
        }

        Assert.False(foundInNestedProjects, "Project at root should not be in NestedProjects section");
    }
}
