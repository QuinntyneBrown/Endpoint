// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.AcceptanceTests.ImplementationSpec;

/// <summary>
/// Acceptance tests for AC5.3: Strategies MUST implement CanHandle and GetPriority.
/// Spec: implementation.spec.md
/// </summary>
public class StrategyMethodsTests : StaticAnalysisTestBase
{
    [Fact]
    public async Task AC5_3_MissingCanHandle_ShouldReportWarning()
    {
        // Arrange
        CreateCSharpFileWithHeader("IncompleteStrategy.cs", @"
namespace TestNamespace;

public class IncompleteStrategy : ISyntaxGenerationStrategy<ClassModel>
{
    public Task<string> GenerateAsync(ClassModel model)
    {
        return Task.FromResult("""");
    }

    public int GetPriority() => 1;
}
");

        // Act
        var result = await RunAnalysisAsync();

        // Assert
        AssertWarningExists(result, "AC5.3");
        Assert.Contains(result.Warnings, w =>
            w.RuleId == "AC5.3" &&
            w.Message.Contains("CanHandle") &&
            w.SpecSource == "implementation.spec.md");
    }

    [Fact]
    public async Task AC5_3_MissingGetPriority_ShouldReportWarning()
    {
        // Arrange
        CreateCSharpFileWithHeader("IncompleteStrategy.cs", @"
namespace TestNamespace;

public class IncompleteStrategy : ISyntaxGenerationStrategy<ClassModel>
{
    public Task<string> GenerateAsync(ClassModel model)
    {
        return Task.FromResult("""");
    }

    public bool CanHandle(object target) => target is ClassModel;
}
");

        // Act
        var result = await RunAnalysisAsync();

        // Assert
        AssertWarningExists(result, "AC5.3");
        Assert.Contains(result.Warnings, w =>
            w.RuleId == "AC5.3" &&
            w.Message.Contains("GetPriority") &&
            w.SpecSource == "implementation.spec.md");
    }

    [Fact]
    public async Task AC5_3_BothMethodsPresent_ShouldPass()
    {
        // Arrange
        CreateCSharpFileWithHeader("CompleteStrategy.cs", @"
namespace TestNamespace;

public class CompleteStrategy : ISyntaxGenerationStrategy<ClassModel>
{
    public Task<string> GenerateAsync(ClassModel model)
    {
        return Task.FromResult("""");
    }

    public bool CanHandle(object target) => target is ClassModel;

    public int GetPriority() => 1;
}
");

        // Act
        var result = await RunAnalysisAsync();

        // Assert
        AssertNoWarning(result, "AC5.3");
    }

    [Fact]
    public async Task AC5_3_ArtifactStrategy_MissingMethods_ShouldReportWarning()
    {
        // Arrange
        CreateCSharpFileWithHeader("IncompleteArtifactStrategy.cs", @"
namespace TestNamespace;

public class IncompleteArtifactStrategy : IArtifactGenerationStrategy<FileModel>
{
    public Task GenerateAsync(FileModel model)
    {
        return Task.CompletedTask;
    }
}
");

        // Act
        var result = await RunAnalysisAsync();

        // Assert
        // Should have two warnings - one for CanHandle, one for GetPriority
        var warnings = result.Warnings.Where(w => w.RuleId == "AC5.3").ToList();
        Assert.Equal(2, warnings.Count);
    }
}
