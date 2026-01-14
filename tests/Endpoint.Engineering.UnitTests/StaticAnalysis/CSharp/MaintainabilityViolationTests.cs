// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.CSharp;

/// <summary>
/// Acceptance tests for maintainability violation detection.
/// </summary>
public class MaintainabilityViolationTests : CSharpStaticAnalysisTestBase
{
    [Fact]
    public async Task Detects_LongMethod()
    {
        // Arrange
        var statements = string.Join("\n            ", Enumerable.Range(1, 60)
            .Select(i => $"var x{i} = {i};"));

        var code = $@"
namespace TestNamespace
{{
    public class Service
    {{
        public void VeryLongMethod()
        {{
            {statements}
        }}
    }}
}}";
        CreateTestFile("Service.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.Maintainability);

        // Assert
        AssertHasIssue(result, "CA1502");
        AssertHasIssueMatching(result,
            i => i.Message.Contains("VeryLongMethod") && i.Message.Contains("lines"),
            "Long method");
    }

    [Fact]
    public async Task Detects_HighCyclomaticComplexity()
    {
        // Arrange
        var code = @"
namespace TestNamespace
{
    public class Validator
    {
        public bool Validate(int a, int b, int c, int d, int e)
        {
            if (a > 0)
            {
                if (b > 0 && c > 0)
                {
                    if (d > 0 || e > 0)
                    {
                        return true;
                    }
                }
                else if (b < 0 && c < 0)
                {
                    if (d < 0 || e < 0)
                    {
                        return false;
                    }
                }
            }

            switch (a)
            {
                case 1:
                    return b > 0;
                case 2:
                    return c > 0;
                case 3:
                    return d > 0;
                case 4:
                    return e > 0;
                case 5:
                    return a == b;
                case 6:
                    return b == c;
                case 7:
                    return c == d;
                case 8:
                    return d == e;
                default:
                    return false;
            }
        }
    }
}";
        CreateTestFile("Validator.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.Maintainability);

        // Assert
        AssertHasIssue(result, "CA1502");
        AssertHasIssueMatching(result,
            i => i.Message.Contains("cyclomatic complexity"),
            "High cyclomatic complexity");
    }

    [Fact]
    public async Task Detects_ComplexMethodWithManyBranches()
    {
        // Arrange
        var code = @"
namespace TestNamespace
{
    public class Processor
    {
        public int Process(int input)
        {
            int result = 0;

            if (input > 0)
                result += 1;
            if (input > 10)
                result += 2;
            if (input > 20)
                result += 3;
            if (input > 30)
                result += 4;
            if (input > 40)
                result += 5;
            if (input > 50)
                result += 6;
            if (input > 60)
                result += 7;
            if (input > 70)
                result += 8;
            if (input > 80)
                result += 9;
            if (input > 90)
                result += 10;
            if (input > 100)
                result += 11;
            if (input > 110)
                result += 12;
            if (input > 120)
                result += 13;
            if (input > 130)
                result += 14;
            if (input > 140)
                result += 15;
            if (input > 150)
                result += 16;

            return result;
        }
    }
}";
        CreateTestFile("Processor.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.Maintainability);

        // Assert
        AssertHasIssue(result, "CA1502");
        AssertHasIssueMatching(result,
            i => i.Message.Contains("complexity"),
            "Complex method with many branches");
    }

    [Fact]
    public async Task NoIssues_WhenMethodIsReasonableLength()
    {
        // Arrange
        var code = @"
namespace TestNamespace
{
    public class Service
    {
        public int Calculate(int a, int b)
        {
            var sum = a + b;
            var product = a * b;
            var difference = a - b;

            if (sum > 100)
            {
                return product;
            }

            return difference;
        }
    }
}";
        CreateTestFile("Service.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.Maintainability);

        // Assert
        Assert.DoesNotContain(result.Issues, i =>
            i.RuleId == "CA1502" && i.Message.Contains("lines"));
    }

    [Fact]
    public async Task NoIssues_WhenCyclomaticComplexityIsLow()
    {
        // Arrange
        var code = @"
namespace TestNamespace
{
    public class Validator
    {
        public bool IsValid(int value)
        {
            if (value < 0)
            {
                return false;
            }

            if (value > 100)
            {
                return false;
            }

            return true;
        }
    }
}";
        CreateTestFile("Validator.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.Maintainability);

        // Assert
        Assert.DoesNotContain(result.Issues, i =>
            i.RuleId == "CA1502" && i.Message.Contains("complexity"));
    }

    [Fact]
    public async Task Detects_MultipleMaintainabilityViolations()
    {
        // Arrange
        var statements = string.Join("\n            ", Enumerable.Range(1, 55)
            .Select(i => $"if (x > {i}) result += {i};"));

        var code = $@"
namespace TestNamespace
{{
    public class Service
    {{
        public int ComplexAndLongMethod(int x)
        {{
            int result = 0;
            {statements}
            return result;
        }}
    }}
}}";
        CreateTestFile("Service.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.Maintainability);

        // Assert
        // Should have both long method and high complexity issues
        var maintainabilityIssues = result.Issues.Where(i =>
            i.Category == IssueCategory.Maintainability && i.RuleId == "CA1502").ToList();
        Assert.True(maintainabilityIssues.Count >= 2,
            $"Expected at least 2 maintainability issues, found {maintainabilityIssues.Count}");
    }

    [Fact]
    public async Task Detects_MethodWithComplexConditions()
    {
        // Arrange
        var code = @"
namespace TestNamespace
{
    public class Filter
    {
        public bool ShouldProcess(int a, int b, int c, int d, int e, int f)
        {
            return (a > 0 && b > 0) ||
                   (c > 0 && d > 0) ||
                   (e > 0 && f > 0) ||
                   (a > 10 && b > 10) ||
                   (c > 10 && d > 10) ||
                   (e > 10 && f > 10) ||
                   (a + b > 100 && c + d > 100) ||
                   (e + f > 100 && a + c > 100) ||
                   (b + d > 100 && e + a > 100);
        }
    }
}";
        CreateTestFile("Filter.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.Maintainability);

        // Assert
        AssertHasIssue(result, "CA1502");
    }
}
