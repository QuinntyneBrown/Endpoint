// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.StaticAnalysis.CSharp;

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.CSharp;

/// <summary>
/// Acceptance tests for unused code violation detection.
/// </summary>
public class UnusedCodeViolationTests : CSharpStaticAnalysisTestBase
{
    [Fact]
    public async Task Detects_PotentiallyUnusedUsingDirective()
    {
        // Arrange
        var code = @"
using SomeUnused.Namespace;
using AnotherUnused.Library;

namespace TestNamespace
{
    public class Service
    {
        public void Process()
        {
            var x = 1;
        }
    }
}";
        CreateTestFile("Service.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.UnusedCode);

        // Assert
        AssertHasIssue(result, "IDE0005");
        AssertHasIssueMatching(result,
            i => i.Message.Contains("unused using") || i.Message.Contains("Potentially unused"),
            "Unused using directive");
    }

    [Fact]
    public async Task Detects_MultipleUnusedUsings()
    {
        // Arrange
        var code = @"
using Unused.Library.One;
using Unused.Library.Two;
using Unused.Library.Three;

namespace TestNamespace
{
    public class Service
    {
        public void DoWork() { }
    }
}";
        CreateTestFile("Service.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.UnusedCode);

        // Assert
        var unusedUsingIssues = result.Issues.Where(i => i.RuleId == "IDE0005").ToList();
        Assert.True(unusedUsingIssues.Count >= 2,
            $"Expected at least 2 unused using issues, found {unusedUsingIssues.Count}");
    }

    [Fact]
    public async Task NoIssues_WhenAllUsingsAreUsed()
    {
        // Arrange
        var code = @"
using System.Text;

namespace TestNamespace
{
    public class Service
    {
        public string Process()
        {
            var sb = new StringBuilder();
            sb.Append(""Hello"");
            return sb.ToString();
        }
    }
}";
        CreateTestFile("Service.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.UnusedCode);

        // Assert
        // System namespace usings should not be flagged
        Assert.DoesNotContain(result.Issues, i =>
            i.RuleId == "IDE0005" && i.Message.Contains("System.Text"));
    }

    [Fact]
    public async Task NoIssues_ForCommonSystemNamespaces()
    {
        // Arrange
        var code = @"
using System;
using System.Collections.Generic;
using System.Linq;

namespace TestNamespace
{
    public class Service
    {
        public void Process()
        {
            var list = new List<int>();
        }
    }
}";
        CreateTestFile("Service.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.UnusedCode);

        // Assert
        // Common System namespaces should be skipped
        Assert.DoesNotContain(result.Issues, i =>
            i.RuleId == "IDE0005" &&
            (i.Message.Contains("System.Collections") || i.Message.Contains("System.Linq")));
    }

    [Fact]
    public async Task Detects_UnusedCustomNamespace()
    {
        // Arrange
        var code = @"
using MyCompany.CustomLib.Helpers;

namespace TestNamespace
{
    public class Service
    {
        public void Process()
        {
            // Not using anything from MyCompany.CustomLib.Helpers
            var x = 1 + 1;
        }
    }
}";
        CreateTestFile("Service.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.UnusedCode);

        // Assert
        AssertHasIssue(result, "IDE0005");
        AssertHasIssueMatching(result,
            i => i.Message.Contains("MyCompany.CustomLib.Helpers"),
            "Unused custom namespace");
    }
}
