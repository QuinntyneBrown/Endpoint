// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.ALaCarte.Core.Helpers;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.ALaCarte.Core.UnitTests.Helpers;

public class CsprojSanitizerTests : IDisposable
{
    private readonly Mock<ILogger> _mockLogger;
    private readonly CsprojSanitizer _sanitizer;
    private readonly string _testDirectory;

    public CsprojSanitizerTests()
    {
        _mockLogger = new Mock<ILogger>();
        _sanitizer = new CsprojSanitizer(_mockLogger.Object);
        _testDirectory = Path.Combine(Path.GetTempPath(), $"csproj_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CsprojSanitizer(null!));
    }

    [Fact]
    public void Constructor_WithValidLogger_CreatesInstance()
    {
        // Act
        var sanitizer = new CsprojSanitizer(_mockLogger.Object);

        // Assert
        Assert.NotNull(sanitizer);
    }

    #endregion

    #region Sanitize - File Not Found Tests

    [Fact]
    public void Sanitize_WithNonExistentFile_ReturnsFalse()
    {
        // Arrange
        var nonExistentPath = Path.Combine(_testDirectory, "nonexistent.csproj");

        // Act
        var result = _sanitizer.Sanitize(nonExistentPath);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Sanitize - ProjectReference Tests

    [Fact]
    public void Sanitize_WithRelativeProjectReferences_RemovesThem()
    {
        // Arrange
        var csprojContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include=""../OtherProject/OtherProject.csproj"" />
    <ProjectReference Include=""..\..\AnotherProject\Another.csproj"" />
  </ItemGroup>
</Project>";

        var csprojPath = Path.Combine(_testDirectory, "Test.csproj");
        File.WriteAllText(csprojPath, csprojContent);

        // Act
        var result = _sanitizer.Sanitize(csprojPath);

        // Assert
        Assert.True(result);
        var sanitizedContent = File.ReadAllText(csprojPath);
        Assert.DoesNotContain("ProjectReference", sanitizedContent);
    }

    [Fact]
    public void Sanitize_WithLocalProjectReferences_KeepsThem()
    {
        // Arrange
        var csprojContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include=""SubFolder/LocalProject.csproj"" />
  </ItemGroup>
</Project>";

        var csprojPath = Path.Combine(_testDirectory, "Test.csproj");
        File.WriteAllText(csprojPath, csprojContent);

        // Act
        var result = _sanitizer.Sanitize(csprojPath);

        // Assert
        Assert.False(result);
        var sanitizedContent = File.ReadAllText(csprojPath);
        Assert.Contains("ProjectReference", sanitizedContent);
    }

    #endregion

    #region Sanitize - Import Tests

    [Fact]
    public void Sanitize_WithRelativeImports_RemovesThem()
    {
        // Arrange
        var csprojContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Project Sdk=""Microsoft.NET.Sdk"">
  <Import Project=""../shared/common.props"" />
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>
</Project>";

        var csprojPath = Path.Combine(_testDirectory, "Test.csproj");
        File.WriteAllText(csprojPath, csprojContent);

        // Act
        var result = _sanitizer.Sanitize(csprojPath);

        // Assert
        Assert.True(result);
        var sanitizedContent = File.ReadAllText(csprojPath);
        Assert.DoesNotContain("common.props", sanitizedContent);
    }

    [Fact]
    public void Sanitize_WithDirectoryBuildImports_RemovesThem()
    {
        // Arrange
        var csprojContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Project Sdk=""Microsoft.NET.Sdk"">
  <Import Project=""../Directory.Build.props"" />
  <Import Project=""../../Directory.Build.targets"" />
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>
</Project>";

        var csprojPath = Path.Combine(_testDirectory, "Test.csproj");
        File.WriteAllText(csprojPath, csprojContent);

        // Act
        var result = _sanitizer.Sanitize(csprojPath);

        // Assert
        Assert.True(result);
        var sanitizedContent = File.ReadAllText(csprojPath);
        Assert.DoesNotContain("Directory.Build.props", sanitizedContent);
        Assert.DoesNotContain("Directory.Build.targets", sanitizedContent);
    }

    [Fact]
    public void Sanitize_WithLocalImports_KeepsThem()
    {
        // Arrange
        var csprojContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Project Sdk=""Microsoft.NET.Sdk"">
  <Import Project=""local.props"" />
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>
</Project>";

        var csprojPath = Path.Combine(_testDirectory, "Test.csproj");
        File.WriteAllText(csprojPath, csprojContent);

        // Act
        var result = _sanitizer.Sanitize(csprojPath);

        // Assert
        Assert.False(result);
        var sanitizedContent = File.ReadAllText(csprojPath);
        Assert.Contains("local.props", sanitizedContent);
    }

    #endregion

    #region Sanitize - Linked Items Tests

    [Fact]
    public void Sanitize_WithLinkedCompileItems_RemovesThem()
    {
        // Arrange
        var csprojContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include=""../shared/SharedCode.cs"" Link=""Shared\SharedCode.cs"" />
    <Content Include=""../shared/config.json"" Link=""config.json"" />
  </ItemGroup>
</Project>";

        var csprojPath = Path.Combine(_testDirectory, "Test.csproj");
        File.WriteAllText(csprojPath, csprojContent);

        // Act
        var result = _sanitizer.Sanitize(csprojPath);

        // Assert
        Assert.True(result);
        var sanitizedContent = File.ReadAllText(csprojPath);
        Assert.DoesNotContain("SharedCode.cs", sanitizedContent);
        Assert.DoesNotContain("config.json", sanitizedContent);
    }

    [Fact]
    public void Sanitize_WithLinkedNoneItems_RemovesThem()
    {
        // Arrange
        var csprojContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <None Include=""../shared/readme.txt"" Link=""readme.txt"" />
  </ItemGroup>
</Project>";

        var csprojPath = Path.Combine(_testDirectory, "Test.csproj");
        File.WriteAllText(csprojPath, csprojContent);

        // Act
        var result = _sanitizer.Sanitize(csprojPath);

        // Assert
        Assert.True(result);
        var sanitizedContent = File.ReadAllText(csprojPath);
        Assert.DoesNotContain("readme.txt", sanitizedContent);
    }

    [Fact]
    public void Sanitize_WithLinkedEmbeddedResources_RemovesThem()
    {
        // Arrange
        var csprojContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <EmbeddedResource Include=""../shared/Resources.resx"" Link=""Resources.resx"" />
  </ItemGroup>
</Project>";

        var csprojPath = Path.Combine(_testDirectory, "Test.csproj");
        File.WriteAllText(csprojPath, csprojContent);

        // Act
        var result = _sanitizer.Sanitize(csprojPath);

        // Assert
        Assert.True(result);
        var sanitizedContent = File.ReadAllText(csprojPath);
        Assert.DoesNotContain("Resources.resx", sanitizedContent);
    }

    #endregion

    #region Sanitize - Empty ItemGroup Tests

    [Fact]
    public void Sanitize_RemovesEmptyItemGroups()
    {
        // Arrange
        var csprojContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include=""../Other/Other.csproj"" />
  </ItemGroup>
</Project>";

        var csprojPath = Path.Combine(_testDirectory, "Test.csproj");
        File.WriteAllText(csprojPath, csprojContent);

        // Act
        var result = _sanitizer.Sanitize(csprojPath);

        // Assert
        Assert.True(result);
        var sanitizedContent = File.ReadAllText(csprojPath);
        Assert.DoesNotContain("<ItemGroup>", sanitizedContent);
    }

    #endregion

    #region Sanitize - No Changes Tests

    [Fact]
    public void Sanitize_WithNoChangesNeeded_ReturnsFalse()
    {
        // Arrange
        var csprojContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include=""Newtonsoft.Json"" Version=""13.0.1"" />
  </ItemGroup>
</Project>";

        var csprojPath = Path.Combine(_testDirectory, "Test.csproj");
        File.WriteAllText(csprojPath, csprojContent);

        // Act
        var result = _sanitizer.Sanitize(csprojPath);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Sanitize - Invalid XML Tests

    [Fact]
    public void Sanitize_WithInvalidXml_ReturnsFalse()
    {
        // Arrange
        var invalidContent = "This is not valid XML content";
        var csprojPath = Path.Combine(_testDirectory, "Invalid.csproj");
        File.WriteAllText(csprojPath, invalidContent);

        // Act
        var result = _sanitizer.Sanitize(csprojPath);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Sanitize - Complex Scenario Tests

    [Fact]
    public void Sanitize_WithMixedContent_RemovesOnlyRelativeReferences()
    {
        // Arrange
        var csprojContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include=""xunit"" Version=""2.4.2"" />
    <ProjectReference Include=""../External/External.csproj"" />
    <ProjectReference Include=""Local/Local.csproj"" />
  </ItemGroup>
  <ItemGroup>
    <Compile Include=""Program.cs"" />
    <Compile Include=""../shared/Shared.cs"" Link=""Shared.cs"" />
  </ItemGroup>
</Project>";

        var csprojPath = Path.Combine(_testDirectory, "Test.csproj");
        File.WriteAllText(csprojPath, csprojContent);

        // Act
        var result = _sanitizer.Sanitize(csprojPath);

        // Assert
        Assert.True(result);
        var sanitizedContent = File.ReadAllText(csprojPath);
        Assert.Contains("xunit", sanitizedContent); // Package reference kept
        Assert.DoesNotContain("External.csproj", sanitizedContent); // Relative project ref removed
        Assert.Contains("Local/Local.csproj", sanitizedContent); // Local project ref kept
        Assert.Contains("Program.cs", sanitizedContent); // Local compile kept
        Assert.DoesNotContain("Shared.cs", sanitizedContent); // Linked compile removed
    }

    #endregion
}
