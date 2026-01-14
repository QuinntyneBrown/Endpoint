// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.AcceptanceTests.ImplementationSpec;

/// <summary>
/// Acceptance tests for AC3.1: Syntax generation strategies MUST follow naming pattern {Concept}SyntaxGenerationStrategy.
/// Spec: implementation.spec.md
/// </summary>
public class SyntaxStrategyNamingTests : StaticAnalysisTestBase
{
    [Fact]
    public async Task AC3_1_CorrectNaming_ShouldPass()
    {
        // Arrange
        CreateCSharpFileWithHeader("ClassSyntaxGenerationStrategy.cs", @"
namespace TestNamespace;

public class ClassSyntaxGenerationStrategy : ISyntaxGenerationStrategy<ClassModel>
{
}
");

        // Act
        var result = await RunAnalysisAsync();

        // Assert
        AssertNoViolation(result, "AC3.1");
    }

    [Fact]
    public async Task AC3_1_IncorrectNaming_MissingSuffix_ShouldReportViolation()
    {
        // Arrange - Class implementing ISyntaxGenerationStrategy but wrong name
        CreateCSharpFileWithHeader("ClassGenerator.cs", @"
namespace TestNamespace;

public class ClassGenerator : ISyntaxGenerationStrategy<ClassModel>
{
}
");

        // Act
        var result = await RunAnalysisAsync();

        // Assert
        AssertViolationExists(result, "AC3.1");
        Assert.Contains(result.Violations, v =>
            v.RuleId == "AC3.1" &&
            v.Message.Contains("SyntaxGenerationStrategy") &&
            v.SpecSource == "implementation.spec.md");
    }

    [Fact]
    public async Task AC3_1_IncorrectNaming_WrongSuffix_ShouldReportViolation()
    {
        // Arrange
        CreateCSharpFileWithHeader("ClassSyntaxStrategy.cs", @"
namespace TestNamespace;

public class ClassSyntaxStrategy : ISyntaxGenerationStrategy<ClassModel>
{
}
");

        // Act
        var result = await RunAnalysisAsync();

        // Assert
        AssertViolationExists(result, "AC3.1");
    }

    [Fact]
    public async Task AC3_1_PluralNaming_ShouldPass()
    {
        // Arrange - Plural form for collection generators
        CreateCSharpFileWithHeader("PropertiesSyntaxGenerationStrategy.cs", @"
namespace TestNamespace;

public class PropertiesSyntaxGenerationStrategy : ISyntaxGenerationStrategy<PropertyModel[]>
{
}
");

        // Act
        var result = await RunAnalysisAsync();

        // Assert
        AssertNoViolation(result, "AC3.1");
    }
}
