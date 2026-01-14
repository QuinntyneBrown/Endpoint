// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.StaticAnalysis.CSharp;

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.CSharp;

/// <summary>
/// Acceptance tests for performance violation detection.
/// </summary>
public class PerformanceViolationTests : CSharpStaticAnalysisTestBase
{
    [Fact]
    public async Task Detects_StringConcatenationInLoop()
    {
        // Arrange
        var code = @"
namespace TestNamespace
{
    public class Builder
    {
        public string BuildString(int count)
        {
            string result = """";
            for (int i = 0; i < count; i++)
            {
                result += i.ToString();
            }
            return result;
        }
    }
}";
        CreateTestFile("Builder.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.Performance);

        // Assert
        AssertHasIssue(result, "CA1850");
        AssertHasIssueMatching(result,
            i => i.Message.Contains("String") && i.Message.Contains("loop"),
            "String concatenation in loop");
    }

    [Fact]
    public async Task Detects_LinqCountInLoop()
    {
        // Arrange
        var code = @"
using System.Collections.Generic;
using System.Linq;

namespace TestNamespace
{
    public class Processor
    {
        public void Process(List<int> items)
        {
            for (int i = 0; i < 10; i++)
            {
                var count = items.Count();
            }
        }
    }
}";
        CreateTestFile("Processor.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.Performance);

        // Assert
        AssertHasIssue(result, "CA1851");
        AssertHasIssueMatching(result,
            i => i.Message.Contains("LINQ") && i.Message.Contains("loop"),
            "LINQ operation in loop");
    }

    [Fact]
    public async Task Detects_LinqAnyInLoop()
    {
        // Arrange
        var code = @"
using System.Collections.Generic;
using System.Linq;

namespace TestNamespace
{
    public class Checker
    {
        public void CheckItems(List<string> items)
        {
            for (int i = 0; i < 100; i++)
            {
                if (items.Any())
                {
                    Process(i);
                }
            }
        }

        private void Process(int x) { }
    }
}";
        CreateTestFile("Checker.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.Performance);

        // Assert
        AssertHasIssue(result, "CA1851");
        AssertHasIssueMatching(result,
            i => i.Message.Contains("LINQ"),
            "LINQ Any() in loop");
    }

    [Fact]
    public async Task Detects_LinqFirstInLoop()
    {
        // Arrange
        var code = @"
using System.Collections.Generic;
using System.Linq;

namespace TestNamespace
{
    public class Selector
    {
        public void SelectItems(List<int> items)
        {
            for (int i = 0; i < 10; i++)
            {
                var first = items.First();
            }
        }
    }
}";
        CreateTestFile("Selector.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.Performance);

        // Assert
        AssertHasIssue(result, "CA1851");
    }

    [Fact]
    public async Task Detects_LinqToListInLoop()
    {
        // Arrange
        var code = @"
using System.Collections.Generic;
using System.Linq;

namespace TestNamespace
{
    public class Converter
    {
        public void Convert(List<int> items)
        {
            for (int i = 0; i < 10; i++)
            {
                var list = items.ToList();
            }
        }
    }
}";
        CreateTestFile("Converter.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.Performance);

        // Assert
        AssertHasIssue(result, "CA1851");
    }

    [Fact]
    public async Task NoIssues_WhenUsingStringBuilder()
    {
        // Arrange
        var code = @"
using System.Text;

namespace TestNamespace
{
    public class Builder
    {
        public string BuildString(int count)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < count; i++)
            {
                sb.Append(i);
            }
            return sb.ToString();
        }
    }
}";
        CreateTestFile("Builder.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.Performance);

        // Assert
        Assert.DoesNotContain(result.Issues, i =>
            i.RuleId == "CA1850" && i.Message.Contains("StringBuilder"));
    }

    [Fact]
    public async Task NoIssues_WhenLinqIsOutsideLoop()
    {
        // Arrange
        var code = @"
using System.Collections.Generic;
using System.Linq;

namespace TestNamespace
{
    public class Processor
    {
        public void Process(List<int> items)
        {
            var count = items.Count();
            for (int i = 0; i < count; i++)
            {
                DoWork(i);
            }
        }

        private void DoWork(int x) { }
    }
}";
        CreateTestFile("Processor.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.Performance);

        // Assert
        Assert.DoesNotContain(result.Issues, i => i.RuleId == "CA1851");
    }

    [Fact]
    public async Task Detects_MultiplePerformanceViolations()
    {
        // Arrange
        var code = @"
using System.Collections.Generic;
using System.Linq;

namespace TestNamespace
{
    public class Processor
    {
        public void Process(List<int> items)
        {
            string result = """";
            for (int i = 0; i < items.Count(); i++)
            {
                result += items.First().ToString();
            }
        }
    }
}";
        CreateTestFile("Processor.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.Performance);

        // Assert
        Assert.True(result.Issues.Count >= 2, $"Expected at least 2 performance issues, found {result.Issues.Count}");
    }
}
