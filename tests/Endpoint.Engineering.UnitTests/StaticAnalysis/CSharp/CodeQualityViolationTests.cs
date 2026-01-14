// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.StaticAnalysis.CSharp;

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.CSharp;

/// <summary>
/// Acceptance tests for code quality violation detection.
/// </summary>
public class CodeQualityViolationTests : CSharpStaticAnalysisTestBase
{
    [Fact]
    public async Task Detects_EmptyCatchBlock()
    {
        // Arrange
        var code = @"
namespace TestNamespace
{
    public class Service
    {
        public void Process()
        {
            try
            {
                DoSomething();
            }
            catch (Exception)
            {
            }
        }

        private void DoSomething() { }
    }
}";
        CreateTestFile("Service.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.CodeQuality);

        // Assert
        AssertHasIssue(result, "CA1031");
        AssertHasIssueMatching(result,
            i => i.Message.Contains("Empty catch block"),
            "Empty catch block");
    }

    [Fact]
    public async Task Detects_CatchAllWithoutLogging()
    {
        // Arrange
        var code = @"
using System;

namespace TestNamespace
{
    public class Service
    {
        public void Process()
        {
            try
            {
                DoWork();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void DoWork() { }
    }
}";
        CreateTestFile("Service.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.CodeQuality);

        // Assert
        AssertHasIssue(result, "CA1031");
        AssertHasIssueMatching(result,
            i => i.Message.Contains("Catching general Exception"),
            "Catching general Exception without logging");
    }

    [Fact]
    public async Task Detects_MagicNumber()
    {
        // Arrange
        var code = @"
namespace TestNamespace
{
    public class Calculator
    {
        public int Calculate(int value)
        {
            return value * 42;
        }
    }
}";
        CreateTestFile("Calculator.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.CodeQuality);

        // Assert
        AssertHasIssue(result, "CA1500");
        AssertHasIssueMatching(result,
            i => i.Message.Contains("Magic number") && i.Message.Contains("42"),
            "Magic number detected");
    }

    [Fact]
    public async Task Detects_TodoComment()
    {
        // Arrange
        var code = @"
namespace TestNamespace
{
    public class Service
    {
        public void Process()
        {
            // TODO: Implement this method
        }
    }
}";
        CreateTestFile("Service.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.CodeQuality);

        // Assert
        AssertHasIssue(result, "TODO");
        AssertHasIssueMatching(result,
            i => i.Message.Contains("TODO"),
            "TODO comment found");
    }

    [Fact]
    public async Task Detects_FixmeComment()
    {
        // Arrange
        var code = @"
namespace TestNamespace
{
    public class Service
    {
        public void Process()
        {
            // FIXME: This is broken
        }
    }
}";
        CreateTestFile("Service.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.CodeQuality);

        // Assert
        AssertHasIssue(result, "TODO");
        AssertHasIssueMatching(result,
            i => i.Message.Contains("TODO") || i.Message.Contains("FIXME"),
            "FIXME comment found");
    }

    [Fact]
    public async Task Detects_HackComment()
    {
        // Arrange
        var code = @"
namespace TestNamespace
{
    public class Service
    {
        public void Process()
        {
            // HACK: Quick workaround
            int result = 0;
        }
    }
}";
        CreateTestFile("Service.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.CodeQuality);

        // Assert
        AssertHasIssue(result, "TODO");
    }

    [Fact]
    public async Task NoIssues_WhenCatchBlockHasLogging()
    {
        // Arrange
        var code = @"
using System;
using Microsoft.Extensions.Logging;

namespace TestNamespace
{
    public class Service
    {
        private readonly ILogger _logger;

        public void Process()
        {
            try
            {
                DoWork();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ""Error occurred"");
                throw;
            }
        }

        private void DoWork() { }
    }
}";
        CreateTestFile("Service.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.CodeQuality);

        // Assert
        Assert.DoesNotContain(result.Issues, i =>
            i.RuleId == "CA1031" && i.Message.Contains("Empty catch block"));
    }

    [Fact]
    public async Task NoIssues_WhenNumberIsInConstant()
    {
        // Arrange
        var code = @"
namespace TestNamespace
{
    public class Calculator
    {
        private const int Multiplier = 42;

        public int Calculate(int value)
        {
            return value * Multiplier;
        }
    }
}";
        CreateTestFile("Calculator.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.CodeQuality);

        // Assert
        // The constant itself should not be flagged as magic number
        Assert.DoesNotContain(result.Issues, i =>
            i.RuleId == "CA1500" && i.Message.Contains("42"));
    }

    [Fact]
    public async Task NoIssues_ForCommonNumbers()
    {
        // Arrange
        var code = @"
namespace TestNamespace
{
    public class Calculator
    {
        public int Increment(int value)
        {
            return value + 1;
        }

        public int Double(int value)
        {
            return value * 2;
        }

        public int Reset()
        {
            return 0;
        }
    }
}";
        CreateTestFile("Calculator.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.CodeQuality);

        // Assert
        // 0, 1, 2 should not be flagged as magic numbers
        Assert.DoesNotContain(result.Issues, i => i.RuleId == "CA1500");
    }

    [Fact]
    public async Task Detects_MultipleCodeQualityIssues()
    {
        // Arrange
        var code = @"
namespace TestNamespace
{
    public class Service
    {
        public void Process()
        {
            // TODO: Fix this
            try
            {
                var result = 100 * 42;
            }
            catch
            {
            }
        }
    }
}";
        CreateTestFile("Service.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.CodeQuality);

        // Assert
        Assert.True(result.Issues.Count >= 3, $"Expected at least 3 code quality issues, found {result.Issues.Count}");
    }
}
