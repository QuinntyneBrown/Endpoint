// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.AcceptanceTests.ImplementationSpec;

/// <summary>
/// Acceptance tests for AC3.2: Artifact generation strategies MUST follow naming pattern
/// {Concept}ArtifactGenerationStrategy or {Concept}GenerationStrategy.
/// Spec: implementation.spec.md
/// </summary>
public class ArtifactStrategyNamingTests : StaticAnalysisTestBase
{
    [Fact]
    public async Task AC3_2_CorrectNaming_WithArtifactSuffix_ShouldPass()
    {
        // Arrange
        CreateCSharpFileWithHeader("ClassCodeFileArtifactGenerationStrategy.cs", @"
namespace TestNamespace;

public class ClassCodeFileArtifactGenerationStrategy : IArtifactGenerationStrategy<CodeFileModel>
{
}
");

        // Act
        var result = await RunAnalysisAsync();

        // Assert
        AssertNoViolation(result, "AC3.2");
    }

    [Fact]
    public async Task AC3_2_CorrectNaming_WithGenerationStrategySuffix_ShouldPass()
    {
        // Arrange - For established concepts like File, Project, Solution
        CreateCSharpFileWithHeader("ProjectGenerationStrategy.cs", @"
namespace TestNamespace;

public class ProjectGenerationStrategy : IArtifactGenerationStrategy<ProjectModel>
{
}
");

        // Act
        var result = await RunAnalysisAsync();

        // Assert
        AssertNoViolation(result, "AC3.2");
    }

    [Fact]
    public async Task AC3_2_IncorrectNaming_MissingSuffix_ShouldReportViolation()
    {
        // Arrange
        CreateCSharpFileWithHeader("ClassFileCreator.cs", @"
namespace TestNamespace;

public class ClassFileCreator : IArtifactGenerationStrategy<CodeFileModel>
{
}
");

        // Act
        var result = await RunAnalysisAsync();

        // Assert
        AssertViolationExists(result, "AC3.2");
        Assert.Contains(result.Violations, v =>
            v.RuleId == "AC3.2" &&
            v.Message.Contains("ArtifactGenerationStrategy") &&
            v.SpecSource == "implementation.spec.md");
    }

    [Fact]
    public async Task AC3_2_IncorrectNaming_WrongSuffix_ShouldReportViolation()
    {
        // Arrange
        CreateCSharpFileWithHeader("ClassArtifactStrategy.cs", @"
namespace TestNamespace;

public class ClassArtifactStrategy : IArtifactGenerationStrategy<CodeFileModel>
{
}
");

        // Act
        var result = await RunAnalysisAsync();

        // Assert
        AssertViolationExists(result, "AC3.2");
    }
}
