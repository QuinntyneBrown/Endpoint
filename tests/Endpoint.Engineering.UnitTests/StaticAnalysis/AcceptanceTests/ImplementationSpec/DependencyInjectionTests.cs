// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.AcceptanceTests.ImplementationSpec;

/// <summary>
/// Acceptance tests for AC5.2: Strategies MUST use dependency injection for dependencies.
/// Spec: implementation.spec.md
/// </summary>
public class DependencyInjectionTests : StaticAnalysisTestBase
{
    [Fact]
    public async Task AC5_2_DirectInstantiation_ShouldReportViolation()
    {
        // Arrange
        CreateCSharpFileWithHeader("BadSyntaxGenerationStrategy.cs", @"
namespace TestNamespace;

public class BadSyntaxGenerationStrategy
{
    public void Generate()
    {
        var generator = new SyntaxGenerator();
        generator.Generate();
    }
}
");

        // Act
        var result = await RunAnalysisAsync();

        // Assert
        AssertViolationExists(result, "AC5.2");
        Assert.Contains(result.Violations, v =>
            v.RuleId == "AC5.2" &&
            v.Message.Contains("dependency injection") &&
            v.SpecSource == "implementation.spec.md");
    }

    [Fact]
    public async Task AC5_2_ConstructorInjection_ShouldPass()
    {
        // Arrange
        CreateCSharpFileWithHeader("GoodSyntaxGenerationStrategy.cs", @"
namespace TestNamespace;

public class GoodSyntaxGenerationStrategy
{
    private readonly ISyntaxGenerator _syntaxGenerator;

    public GoodSyntaxGenerationStrategy(ISyntaxGenerator syntaxGenerator)
    {
        _syntaxGenerator = syntaxGenerator;
    }

    public void Generate()
    {
        _syntaxGenerator.Generate();
    }
}
");

        // Act
        var result = await RunAnalysisAsync();

        // Assert
        AssertNoViolation(result, "AC5.2");
    }

    [Fact]
    public async Task AC5_2_DirectInstantiationInArtifactStrategy_ShouldReportViolation()
    {
        // Arrange
        CreateCSharpFileWithHeader("BadArtifactGenerationStrategy.cs", @"
namespace TestNamespace;

public class BadArtifactGenerationStrategy
{
    public void Generate()
    {
        var generator = new SyntaxGenerator();
    }
}
");

        // Act
        var result = await RunAnalysisAsync();

        // Assert
        AssertViolationExists(result, "AC5.2");
    }
}
