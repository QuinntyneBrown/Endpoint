// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.AcceptanceTests.ImplementationSpec;

/// <summary>
/// Acceptance tests for AC2.1: Syntax generation strategies MUST NOT perform file I/O.
/// Spec: implementation.spec.md
/// </summary>
public class SyntaxStrategyFileIOTests : StaticAnalysisTestBase
{
    [Fact]
    public async Task AC2_1_SyntaxStrategyWithFileWriteAllText_ShouldReportViolation()
    {
        // Arrange
        CreateCSharpFileWithHeader("BadSyntaxGenerationStrategy.cs", @"
namespace TestNamespace;

public class BadSyntaxGenerationStrategy
{
    public void Generate()
    {
        File.WriteAllText(""test.txt"", ""content"");
    }
}
");

        // Act
        var result = await RunAnalysisAsync();

        // Assert
        AssertViolationExists(result, "AC2.1");
        Assert.Contains(result.Violations, v =>
            v.RuleId == "AC2.1" &&
            v.Message.Contains("file I/O") &&
            v.SpecSource == "implementation.spec.md");
    }

    [Fact]
    public async Task AC2_1_SyntaxStrategyWithFileCreate_ShouldReportViolation()
    {
        // Arrange
        CreateCSharpFileWithHeader("BadSyntaxGenerationStrategy.cs", @"
namespace TestNamespace;

public class BadSyntaxGenerationStrategy
{
    public void Generate()
    {
        using var file = File.Create(""test.txt"");
    }
}
");

        // Act
        var result = await RunAnalysisAsync();

        // Assert
        AssertViolationExists(result, "AC2.1");
    }

    [Fact]
    public async Task AC2_1_SyntaxStrategyWithDirectoryCreate_ShouldReportViolation()
    {
        // Arrange
        CreateCSharpFileWithHeader("BadSyntaxGenerationStrategy.cs", @"
namespace TestNamespace;

public class BadSyntaxGenerationStrategy
{
    public void Generate()
    {
        Directory.CreateDirectory(""testDir"");
    }
}
");

        // Act
        var result = await RunAnalysisAsync();

        // Assert
        AssertViolationExists(result, "AC2.1");
    }

    [Fact]
    public async Task AC2_1_SyntaxStrategyWithIFileSystem_ShouldReportViolation()
    {
        // Arrange
        CreateCSharpFileWithHeader("BadSyntaxGenerationStrategy.cs", @"
namespace TestNamespace;

public class BadSyntaxGenerationStrategy
{
    private readonly IFileSystem _fileSystem;

    public BadSyntaxGenerationStrategy(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }
}
");

        // Act
        var result = await RunAnalysisAsync();

        // Assert
        AssertViolationExists(result, "AC2.1");
    }

    [Fact]
    public async Task AC2_1_SyntaxStrategyWithoutFileIO_ShouldPass()
    {
        // Arrange
        CreateCSharpFileWithHeader("GoodSyntaxGenerationStrategy.cs", @"
namespace TestNamespace;

public class GoodSyntaxGenerationStrategy
{
    public string Generate()
    {
        return ""Generated code"";
    }
}
");

        // Act
        var result = await RunAnalysisAsync();

        // Assert
        AssertNoViolation(result, "AC2.1");
    }

    [Fact]
    public async Task AC2_1_ArtifactStrategyWithFileIO_ShouldNotReportViolation()
    {
        // Arrange - Artifact strategies ARE allowed to do file I/O
        CreateCSharpFileWithHeader("GoodArtifactGenerationStrategy.cs", @"
namespace TestNamespace;

public class GoodArtifactGenerationStrategy
{
    public void Generate()
    {
        File.WriteAllText(""test.txt"", ""content"");
    }
}
");

        // Act
        var result = await RunAnalysisAsync();

        // Assert
        AssertNoViolation(result, "AC2.1");
    }
}
