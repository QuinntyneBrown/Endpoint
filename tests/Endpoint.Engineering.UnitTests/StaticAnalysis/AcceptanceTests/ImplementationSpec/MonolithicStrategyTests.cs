// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text;

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.AcceptanceTests.ImplementationSpec;

/// <summary>
/// Acceptance tests for AC1.4: Strategies MUST NOT contain large monolithic generation logic.
/// Spec: implementation.spec.md
/// </summary>
public class MonolithicStrategyTests : StaticAnalysisTestBase
{
    [Fact]
    public async Task AC1_4_LargeSyntaxStrategy_ShouldReportWarning()
    {
        // Arrange - Create a large strategy file (>300 lines)
        var sb = new StringBuilder();
        sb.AppendLine("// Copyright (c) Quinntyne Brown. All Rights Reserved.");
        sb.AppendLine("// Licensed under the MIT License. See License.txt in the project root for license information.");
        sb.AppendLine();
        sb.AppendLine("namespace TestNamespace;");
        sb.AppendLine();
        sb.AppendLine("public class TestSyntaxGenerationStrategy");
        sb.AppendLine("{");

        // Add enough lines to exceed 300
        for (int i = 0; i < 310; i++)
        {
            sb.AppendLine($"    // Line {i}");
        }

        sb.AppendLine("}");

        CreateCSharpFile("TestSyntaxGenerationStrategy.cs", sb.ToString());

        // Act
        var result = await RunAnalysisAsync();

        // Assert
        AssertWarningExists(result, "AC1.4");
        Assert.Contains(result.Warnings, w =>
            w.RuleId == "AC1.4" &&
            w.Message.Contains("lines") &&
            w.SpecSource == "implementation.spec.md");
    }

    [Fact]
    public async Task AC1_4_LargeArtifactStrategy_ShouldReportWarning()
    {
        // Arrange - Create a large artifact strategy file (>300 lines)
        var sb = new StringBuilder();
        sb.AppendLine("// Copyright (c) Quinntyne Brown. All Rights Reserved.");
        sb.AppendLine("// Licensed under the MIT License. See License.txt in the project root for license information.");
        sb.AppendLine();
        sb.AppendLine("namespace TestNamespace;");
        sb.AppendLine();
        sb.AppendLine("public class TestArtifactGenerationStrategy");
        sb.AppendLine("{");

        for (int i = 0; i < 310; i++)
        {
            sb.AppendLine($"    // Line {i}");
        }

        sb.AppendLine("}");

        CreateCSharpFile("TestArtifactGenerationStrategy.cs", sb.ToString());

        // Act
        var result = await RunAnalysisAsync();

        // Assert
        AssertWarningExists(result, "AC1.4");
    }

    [Fact]
    public async Task AC1_4_SmallStrategy_ShouldNotReportWarning()
    {
        // Arrange - Create a small strategy file (<300 lines)
        CreateCSharpFileWithHeader("SmallSyntaxGenerationStrategy.cs", @"
namespace TestNamespace;

public class SmallSyntaxGenerationStrategy
{
    public void Generate()
    {
        // Small implementation
    }
}
");

        // Act
        var result = await RunAnalysisAsync();

        // Assert
        AssertNoWarning(result, "AC1.4");
    }
}
