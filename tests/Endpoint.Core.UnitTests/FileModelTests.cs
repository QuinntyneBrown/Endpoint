// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Core.UnitTests;

public class FileModelTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_InitializesNameProperty()
    {
        var fileModel = new FileModel("TestFile", "/home/user/project", ".cs");

        Assert.Equal("TestFile", fileModel.Name);
    }

    [Fact]
    public void Constructor_InitializesDirectoryProperty()
    {
        var fileModel = new FileModel("TestFile", "/home/user/project", ".cs");

        Assert.Equal("/home/user/project", fileModel.Directory);
    }

    [Fact]
    public void Constructor_InitializesExtensionProperty()
    {
        var fileModel = new FileModel("TestFile", "/home/user/project", ".cs");

        Assert.Equal(".cs", fileModel.Extension);
    }

    [Fact]
    public void Constructor_ComputesPathCorrectly()
    {
        var fileModel = new FileModel("TestFile", "/home/user/project", ".cs");

        var expectedPath = Path.Combine("/home/user/project", "TestFile.cs");
        Assert.Equal(expectedPath, fileModel.Path);
    }

    #endregion

    #region Path Computation Tests

    [Fact]
    public void Path_WithDifferentExtensions_ComputesCorrectly()
    {
        var csFile = new FileModel("MyClass", "/src", ".cs");
        var jsonFile = new FileModel("config", "/config", ".json");
        var txtFile = new FileModel("readme", "/docs", ".txt");

        Assert.Equal(Path.Combine("/src", "MyClass.cs"), csFile.Path);
        Assert.Equal(Path.Combine("/config", "config.json"), jsonFile.Path);
        Assert.Equal(Path.Combine("/docs", "readme.txt"), txtFile.Path);
    }

    [Fact]
    public void Path_WithEmptyExtension_ComputesCorrectly()
    {
        var fileModel = new FileModel("Makefile", "/project", "");

        Assert.Equal(Path.Combine("/project", "Makefile"), fileModel.Path);
    }

    [Fact]
    public void Path_WithNestedDirectory_ComputesCorrectly()
    {
        var fileModel = new FileModel("Service", "/home/user/project/src/services", ".cs");

        var expectedPath = Path.Combine("/home/user/project/src/services", "Service.cs");
        Assert.Equal(expectedPath, fileModel.Path);
    }

    [Fact]
    public void Path_WithWindowsStyleDirectory_ComputesCorrectly()
    {
        var fileModel = new FileModel("Program", "C:\\Users\\Developer\\Project", ".cs");

        var expectedPath = Path.Combine("C:\\Users\\Developer\\Project", "Program.cs");
        Assert.Equal(expectedPath, fileModel.Path);
    }

    [Fact]
    public void Path_WithSpecialCharactersInName_ComputesCorrectly()
    {
        var fileModel = new FileModel("My-Special_File.Test", "/project", ".cs");

        var expectedPath = Path.Combine("/project", "My-Special_File.Test.cs");
        Assert.Equal(expectedPath, fileModel.Path);
    }

    #endregion

    #region Body Property Tests

    [Fact]
    public void Body_CanBeSetAndRetrieved()
    {
        var fileModel = new FileModel("TestFile", "/project", ".cs");
        var body = "public class TestClass { }";

        fileModel.Body = body;

        Assert.Equal(body, fileModel.Body);
    }

    [Fact]
    public void Body_DefaultsToNull()
    {
        var fileModel = new FileModel("TestFile", "/project", ".cs");

        Assert.Null(fileModel.Body);
    }

    [Fact]
    public void Body_CanBeSetToEmpty()
    {
        var fileModel = new FileModel("TestFile", "/project", ".cs");

        fileModel.Body = string.Empty;

        Assert.Equal(string.Empty, fileModel.Body);
    }

    [Fact]
    public void Body_CanContainMultipleLines()
    {
        var fileModel = new FileModel("TestFile", "/project", ".cs");
        var multiLineBody = $"namespace Test{Environment.NewLine}{{{Environment.NewLine}    public class MyClass {{ }}{Environment.NewLine}}}";

        fileModel.Body = multiLineBody;

        Assert.Equal(multiLineBody, fileModel.Body);
        Assert.Contains(Environment.NewLine, fileModel.Body);
    }

    #endregion

    #region Property Mutability Tests

    [Fact]
    public void Name_CanBeModified()
    {
        var fileModel = new FileModel("OriginalName", "/project", ".cs");

        fileModel.Name = "NewName";

        Assert.Equal("NewName", fileModel.Name);
    }

    [Fact]
    public void Directory_CanBeModified()
    {
        var fileModel = new FileModel("TestFile", "/original/path", ".cs");

        fileModel.Directory = "/new/path";

        Assert.Equal("/new/path", fileModel.Directory);
    }

    [Fact]
    public void Extension_CanBeModified()
    {
        var fileModel = new FileModel("TestFile", "/project", ".cs");

        fileModel.Extension = ".txt";

        Assert.Equal(".txt", fileModel.Extension);
    }

    [Fact]
    public void Path_CanBeModifiedDirectly()
    {
        var fileModel = new FileModel("TestFile", "/project", ".cs");

        fileModel.Path = "/custom/path/CustomFile.cs";

        Assert.Equal("/custom/path/CustomFile.cs", fileModel.Path);
    }

    [Fact]
    public void ModifyingProperties_DoesNotAutoUpdatePath()
    {
        var fileModel = new FileModel("TestFile", "/project", ".cs");
        var originalPath = fileModel.Path;

        fileModel.Name = "NewName";

        // Path is set once in constructor and doesn't auto-update
        Assert.Equal(originalPath, fileModel.Path);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Constructor_WithEmptyName_CreatesFileModel()
    {
        var fileModel = new FileModel("", "/project", ".cs");

        Assert.Equal("", fileModel.Name);
        Assert.Equal(Path.Combine("/project", ".cs"), fileModel.Path);
    }

    [Fact]
    public void Constructor_WithEmptyDirectory_CreatesFileModel()
    {
        var fileModel = new FileModel("TestFile", "", ".cs");

        Assert.Equal("", fileModel.Directory);
        Assert.Equal("TestFile.cs", fileModel.Path);
    }

    [Fact]
    public void Constructor_WithExtensionWithoutDot_ComputesPathAsIs()
    {
        var fileModel = new FileModel("TestFile", "/project", "cs");

        Assert.Equal(Path.Combine("/project", "TestFilecs"), fileModel.Path);
    }

    [Fact]
    public void Constructor_WithMultiPartExtension_ComputesCorrectly()
    {
        var fileModel = new FileModel("template", "/project", ".cshtml");

        Assert.Equal(Path.Combine("/project", "template.cshtml"), fileModel.Path);
    }

    [Fact]
    public void Constructor_WithDotsInName_ComputesCorrectly()
    {
        var fileModel = new FileModel("My.Assembly.Name", "/project", ".dll");

        Assert.Equal(Path.Combine("/project", "My.Assembly.Name.dll"), fileModel.Path);
    }

    #endregion

    #region Real-World Scenario Tests

    [Fact]
    public void FileModel_ForCSharpClass_HasCorrectProperties()
    {
        var fileModel = new FileModel("UserService", "/src/Services", ".cs");
        fileModel.Body = @"namespace MyApp.Services
{
    public class UserService
    {
        public void DoSomething() { }
    }
}";

        Assert.Equal("UserService", fileModel.Name);
        Assert.Equal("/src/Services", fileModel.Directory);
        Assert.Equal(".cs", fileModel.Extension);
        Assert.Equal(Path.Combine("/src/Services", "UserService.cs"), fileModel.Path);
        Assert.Contains("public class UserService", fileModel.Body);
    }

    [Fact]
    public void FileModel_ForJsonConfig_HasCorrectProperties()
    {
        var fileModel = new FileModel("appsettings", "/config", ".json");
        fileModel.Body = @"{
    ""ConnectionStrings"": {
        ""Default"": ""Server=localhost;Database=MyDb""
    }
}";

        Assert.Equal("appsettings", fileModel.Name);
        Assert.Equal(".json", fileModel.Extension);
        Assert.Contains("ConnectionStrings", fileModel.Body);
    }

    [Fact]
    public void FileModel_ForProjectFile_HasCorrectProperties()
    {
        var fileModel = new FileModel("MyProject", "/src/MyProject", ".csproj");
        fileModel.Body = @"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>
</Project>";

        Assert.Equal("MyProject", fileModel.Name);
        Assert.Equal(".csproj", fileModel.Extension);
        Assert.Contains("TargetFramework", fileModel.Body);
    }

    #endregion
}
