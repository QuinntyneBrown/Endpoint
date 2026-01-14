// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.CSharp;

/// <summary>
/// Acceptance tests for design violation detection.
/// </summary>
public class DesignViolationTests : CSharpStaticAnalysisTestBase
{
    [Fact]
    public async Task Detects_ClassWithTooManyMethods()
    {
        // Arrange
        var methods = string.Join("\n        ", Enumerable.Range(1, 25)
            .Select(i => $"public void Method{i}() {{ }}"));

        var code = $@"
namespace TestNamespace
{{
    public class LargeService
    {{
        {methods}
    }}
}}";
        CreateTestFile("LargeService.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.Design);

        // Assert
        AssertHasIssue(result, "CA1502");
        AssertHasIssueMatching(result,
            i => i.Message.Contains("LargeService") && i.Message.Contains("methods"),
            "Class with too many methods");
    }

    [Fact]
    public async Task Detects_MethodWithTooManyParameters()
    {
        // Arrange
        var code = @"
namespace TestNamespace
{
    public class Service
    {
        public void ProcessOrder(
            int orderId,
            string customerName,
            string address,
            string city,
            string state,
            string zip,
            string country,
            string phone,
            decimal amount)
        {
        }
    }
}";
        CreateTestFile("Service.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.Design);

        // Assert
        AssertHasIssue(result, "CA1026");
        AssertHasIssueMatching(result,
            i => i.Message.Contains("ProcessOrder") && i.Message.Contains("parameters"),
            "Method with too many parameters");
    }

    [Fact]
    public async Task Detects_DeeplyNestedCode()
    {
        // Arrange
        var code = @"
namespace TestNamespace
{
    public class Processor
    {
        public void Process(int x)
        {
            if (x > 0)
            {
                if (x > 10)
                {
                    for (int i = 0; i < x; i++)
                    {
                        if (i % 2 == 0)
                        {
                            while (i > 0)
                            {
                                x--;
                            }
                        }
                    }
                }
            }
        }
    }
}";
        CreateTestFile("Processor.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.Design);

        // Assert
        AssertHasIssue(result, "CA1502");
        AssertHasIssueMatching(result,
            i => i.Message.Contains("nesting depth"),
            "Deeply nested code");
    }

    [Fact]
    public async Task NoIssues_WhenClassHasReasonableMethodCount()
    {
        // Arrange
        var methods = string.Join("\n        ", Enumerable.Range(1, 10)
            .Select(i => $"public void Method{i}() {{ }}"));

        var code = $@"
namespace TestNamespace
{{
    public class Service
    {{
        {methods}
    }}
}}";
        CreateTestFile("Service.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.Design);

        // Assert
        Assert.DoesNotContain(result.Issues, i =>
            i.RuleId == "CA1502" && i.Message.Contains("methods"));
    }

    [Fact]
    public async Task NoIssues_WhenMethodHasReasonableParameters()
    {
        // Arrange
        var code = @"
namespace TestNamespace
{
    public class Service
    {
        public void ProcessOrder(int orderId, string customerName, decimal amount)
        {
        }
    }
}";
        CreateTestFile("Service.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.Design);

        // Assert
        Assert.DoesNotContain(result.Issues, i =>
            i.RuleId == "CA1026" && i.Message.Contains("ProcessOrder"));
    }

    [Fact]
    public async Task NoIssues_WhenCodeHasReasonableNesting()
    {
        // Arrange
        var code = @"
namespace TestNamespace
{
    public class Processor
    {
        public void Process(int x)
        {
            if (x > 0)
            {
                for (int i = 0; i < x; i++)
                {
                    Console.WriteLine(i);
                }
            }
        }
    }
}";
        CreateTestFile("Processor.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.Design);

        // Assert
        Assert.DoesNotContain(result.Issues, i =>
            i.RuleId == "CA1502" && i.Message.Contains("nesting"));
    }

    [Fact]
    public async Task Detects_MultipleDesignViolations()
    {
        // Arrange
        var methods = string.Join("\n        ", Enumerable.Range(1, 22)
            .Select(i => $"public void Method{i}() {{ }}"));

        var code = $@"
namespace TestNamespace
{{
    public class LargeService
    {{
        public void TooManyParams(int a, int b, int c, int d, int e, int f, int g, int h, int i)
        {{
        }}

        {methods}
    }}
}}";
        CreateTestFile("LargeService.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.Design);

        // Assert
        Assert.True(result.Issues.Count >= 2, $"Expected at least 2 design issues, found {result.Issues.Count}");
    }
}
