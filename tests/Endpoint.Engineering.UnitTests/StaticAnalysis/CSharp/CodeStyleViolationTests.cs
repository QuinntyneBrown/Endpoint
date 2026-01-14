// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.CSharp;

/// <summary>
/// Acceptance tests for code style violation detection.
/// </summary>
public class CodeStyleViolationTests : CSharpStaticAnalysisTestBase
{
    [Fact]
    public async Task Detects_VeryLongLine()
    {
        // Arrange
        var longString = new string('a', 250);
        var code = $@"
namespace TestNamespace
{{
    public class Service
    {{
        public string GetValue() => ""{longString}"";
    }}
}}";
        CreateTestFile("Service.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.Style);

        // Assert
        AssertHasIssue(result, "SA1505");
        AssertHasIssueMatching(result,
            i => i.Message.Contains("characters long"),
            "Line too long");
    }

    [Fact]
    public async Task Detects_TrailingWhitespace()
    {
        // Arrange
        var code = "namespace TestNamespace\n{\n    public class Service    \n    {\n    }\n}\n";
        CreateTestFile("Service.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.Style);

        // Assert
        AssertHasIssue(result, "SA1028");
        AssertHasIssueMatching(result,
            i => i.Message.Contains("Trailing whitespace"),
            "Trailing whitespace detected");
    }

    [Fact]
    public async Task Detects_MultipleStatementsOnSameLine()
    {
        // Arrange
        var code = @"
namespace TestNamespace
{
    public class Calculator
    {
        public void Process()
        {
            int a = 1; int b = 2;
        }
    }
}";
        CreateTestFile("Calculator.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.Style);

        // Assert
        AssertHasIssue(result, "SA1501");
        AssertHasIssueMatching(result,
            i => i.Message.Contains("Multiple statements"),
            "Multiple statements on same line");
    }

    [Fact]
    public async Task NoIssues_WhenLinesAreReasonableLength()
    {
        // Arrange
        var code = @"
namespace TestNamespace
{
    public class Service
    {
        public string GetValue()
        {
            return ""Hello World"";
        }
    }
}";
        CreateTestFile("Service.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.Style);

        // Assert
        Assert.DoesNotContain(result.Issues, i => i.RuleId == "SA1505");
    }

    [Fact]
    public async Task NoIssues_WhenNoTrailingWhitespace()
    {
        // Arrange
        var code = @"namespace TestNamespace
{
    public class Service
    {
    }
}";
        CreateTestFile("Service.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.Style);

        // Assert
        Assert.DoesNotContain(result.Issues, i => i.RuleId == "SA1028");
    }

    [Fact]
    public async Task NoIssues_WhenOneStatementPerLine()
    {
        // Arrange
        var code = @"
namespace TestNamespace
{
    public class Calculator
    {
        public void Process()
        {
            int a = 1;
            int b = 2;
            int c = a + b;
        }
    }
}";
        CreateTestFile("Calculator.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.Style);

        // Assert
        Assert.DoesNotContain(result.Issues, i => i.RuleId == "SA1501");
    }

    [Fact]
    public async Task Detects_MultipleStyleViolationsInSameFile()
    {
        // Arrange
        var longString = new string('x', 220);
        var code = $@"namespace TestNamespace
{{
    public class Service
    {{
        public void Process()
        {{
            int a = 1; int b = 2;
            var s = ""{longString}"";
        }}
    }}
}}";
        CreateTestFile("Service.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.Style);

        // Assert
        Assert.True(result.Issues.Count >= 2, $"Expected at least 2 style issues, found {result.Issues.Count}");
    }
}
