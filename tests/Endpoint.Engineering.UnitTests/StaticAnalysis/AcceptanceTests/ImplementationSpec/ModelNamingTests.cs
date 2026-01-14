// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.AcceptanceTests.ImplementationSpec;

/// <summary>
/// Acceptance tests for AC3.3: Models MUST follow naming pattern {Concept}Model.
/// Spec: implementation.spec.md
/// </summary>
public class ModelNamingTests : StaticAnalysisTestBase
{
    [Fact]
    public async Task AC3_3_CorrectNaming_ShouldPass()
    {
        // Arrange
        CreateCSharpFileWithHeader("PropertyModel.cs", @"
namespace TestNamespace;

public class PropertyModel : SyntaxModel
{
    public string Name { get; init; }
}
");

        // Act
        var result = await RunAnalysisAsync();

        // Assert
        AssertNoViolation(result, "AC3.3");
    }

    [Fact]
    public async Task AC3_3_IncorrectNaming_MissingSuffix_SyntaxModel_ShouldReportViolation()
    {
        // Arrange
        CreateCSharpFileWithHeader("Property.cs", @"
namespace TestNamespace;

public class Property : SyntaxModel
{
    public string Name { get; init; }
}
");

        // Act
        var result = await RunAnalysisAsync();

        // Assert
        AssertViolationExists(result, "AC3.3");
        Assert.Contains(result.Violations, v =>
            v.RuleId == "AC3.3" &&
            v.Message.Contains("Model") &&
            v.SpecSource == "implementation.spec.md");
    }

    [Fact]
    public async Task AC3_3_IncorrectNaming_MissingSuffix_ArtifactModel_ShouldReportViolation()
    {
        // Arrange
        CreateCSharpFileWithHeader("CodeFile.cs", @"
namespace TestNamespace;

public class CodeFile : ArtifactModel
{
    public string Path { get; init; }
}
");

        // Act
        var result = await RunAnalysisAsync();

        // Assert
        AssertViolationExists(result, "AC3.3");
    }

    [Fact]
    public async Task AC3_3_ArtifactModelDerivative_CorrectNaming_ShouldPass()
    {
        // Arrange
        CreateCSharpFileWithHeader("FileModel.cs", @"
namespace TestNamespace;

public class FileModel : ArtifactModel
{
    public string Path { get; init; }
}
");

        // Act
        var result = await RunAnalysisAsync();

        // Assert
        AssertNoViolation(result, "AC3.3");
    }
}
