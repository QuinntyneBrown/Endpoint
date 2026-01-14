// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.StaticAnalysis.CSharp;

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.CSharp;

/// <summary>
/// Acceptance tests for naming convention violation detection.
/// </summary>
public class NamingConventionViolationTests : CSharpStaticAnalysisTestBase
{
    [Fact]
    public async Task Detects_ClassNameNotInPascalCase()
    {
        // Arrange
        var code = @"
namespace TestNamespace
{
    public class myClass
    {
    }
}";
        CreateTestFile("MyClass.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.Naming);

        // Assert
        AssertHasIssue(result, "SA1300");
        AssertHasIssueMatching(result,
            i => i.Message.Contains("myClass") && i.Message.Contains("PascalCase"),
            "Class name not in PascalCase");
    }

    [Fact]
    public async Task Detects_ClassNameStartingWithLowercase()
    {
        // Arrange
        var code = @"
namespace TestNamespace
{
    public class orderService
    {
        public void Process() { }
    }
}";
        CreateTestFile("OrderService.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.Naming);

        // Assert
        AssertHasIssue(result, "SA1300");
        Assert.Contains(result.Issues, i =>
            i.RuleId == "SA1300" &&
            i.Message.Contains("orderService"));
    }

    [Fact]
    public async Task Detects_InterfaceNameNotStartingWithI()
    {
        // Arrange
        var code = @"
namespace TestNamespace
{
    public interface OrderRepository
    {
        void Save();
    }
}";
        CreateTestFile("IOrderRepository.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.Naming);

        // Assert
        AssertHasIssue(result, "SA1302");
        AssertHasIssueMatching(result,
            i => i.Message.Contains("OrderRepository") && i.Message.Contains("'I'"),
            "Interface name not starting with I");
    }

    [Fact]
    public async Task Detects_InterfaceNameWithLowercaseAfterI()
    {
        // Arrange
        var code = @"
namespace TestNamespace
{
    public interface IorderService
    {
        void Process();
    }
}";
        CreateTestFile("IOrderService.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.Naming);

        // Assert
        AssertHasIssue(result, "SA1302");
        Assert.Contains(result.Issues, i =>
            i.RuleId == "SA1302" &&
            i.Message.Contains("IorderService"));
    }

    [Fact]
    public async Task Detects_MethodNameNotInPascalCase()
    {
        // Arrange
        var code = @"
namespace TestNamespace
{
    public class Service
    {
        public void processOrder()
        {
        }
    }
}";
        CreateTestFile("Service.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.Naming);

        // Assert
        AssertHasIssue(result, "SA1300");
        AssertHasIssueMatching(result,
            i => i.Message.Contains("processOrder") && i.Message.Contains("PascalCase"),
            "Method name not in PascalCase");
    }

    [Fact]
    public async Task Detects_MethodNameWithUnderscore()
    {
        // Arrange
        var code = @"
namespace TestNamespace
{
    public class Calculator
    {
        public int Calculate_Total()
        {
            return 0;
        }
    }
}";
        CreateTestFile("Calculator.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.Naming);

        // Assert
        AssertHasIssue(result, "SA1300");
        Assert.Contains(result.Issues, i =>
            i.RuleId == "SA1300" &&
            i.Message.Contains("Calculate_Total"));
    }

    [Fact]
    public async Task Detects_PrivateFieldNotUsingUnderscorePrefix()
    {
        // Arrange
        var code = @"
namespace TestNamespace
{
    public class Service
    {
        private string myField;

        public Service()
        {
            myField = ""test"";
        }
    }
}";
        CreateTestFile("Service.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.Naming);

        // Assert
        AssertHasIssue(result, "SA1309");
        AssertHasIssueMatching(result,
            i => i.Message.Contains("myField") && i.Message.Contains("_camelCase"),
            "Private field not using _camelCase");
    }

    [Fact]
    public async Task Detects_PrivateFieldWithUppercaseAfterUnderscore()
    {
        // Arrange
        var code = @"
namespace TestNamespace
{
    public class Repository
    {
        private readonly string _MyConnection;

        public Repository()
        {
            _MyConnection = ""connection"";
        }
    }
}";
        CreateTestFile("Repository.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.Naming);

        // Assert
        AssertHasIssue(result, "SA1309");
        Assert.Contains(result.Issues, i =>
            i.RuleId == "SA1309" &&
            i.Message.Contains("_MyConnection"));
    }

    [Fact]
    public async Task Detects_ConstantNotInPascalCaseOrUpperSnakeCase()
    {
        // Arrange
        var code = @"
namespace TestNamespace
{
    public class Constants
    {
        public const int maxRetries = 5;
    }
}";
        CreateTestFile("Constants.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.Naming);

        // Assert
        AssertHasIssue(result, "SA1303");
        AssertHasIssueMatching(result,
            i => i.Message.Contains("maxRetries"),
            "Constant not in correct case");
    }

    [Fact]
    public async Task NoIssues_WhenClassNameIsCorrectlyPascalCase()
    {
        // Arrange
        var code = @"
namespace TestNamespace
{
    public class OrderService
    {
    }
}";
        CreateTestFile("OrderService.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.Naming);

        // Assert
        Assert.DoesNotContain(result.Issues, i =>
            i.RuleId == "SA1300" && i.Message.Contains("OrderService"));
    }

    [Fact]
    public async Task NoIssues_WhenInterfaceNameStartsWithI()
    {
        // Arrange
        var code = @"
namespace TestNamespace
{
    public interface IOrderRepository
    {
        void Save();
    }
}";
        CreateTestFile("IOrderRepository.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.Naming);

        // Assert
        Assert.DoesNotContain(result.Issues, i =>
            i.RuleId == "SA1302" && i.Message.Contains("IOrderRepository"));
    }

    [Fact]
    public async Task NoIssues_WhenPrivateFieldUsesUnderscorePrefix()
    {
        // Arrange
        var code = @"
namespace TestNamespace
{
    public class Service
    {
        private readonly string _connectionString;

        public Service(string connectionString)
        {
            _connectionString = connectionString;
        }
    }
}";
        CreateTestFile("Service.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.Naming);

        // Assert
        Assert.DoesNotContain(result.Issues, i =>
            i.RuleId == "SA1309" && i.Message.Contains("_connectionString"));
    }

    [Fact]
    public async Task NoIssues_WhenConstantIsInPascalCase()
    {
        // Arrange
        var code = @"
namespace TestNamespace
{
    public class Constants
    {
        public const int MaxRetries = 5;
    }
}";
        CreateTestFile("Constants.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.Naming);

        // Assert
        Assert.DoesNotContain(result.Issues, i =>
            i.RuleId == "SA1303" && i.Message.Contains("MaxRetries"));
    }

    [Fact]
    public async Task NoIssues_WhenConstantIsInUpperSnakeCase()
    {
        // Arrange
        var code = @"
namespace TestNamespace
{
    public class Constants
    {
        public const int MAX_RETRIES = 5;
    }
}";
        CreateTestFile("Constants.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.Naming);

        // Assert
        Assert.DoesNotContain(result.Issues, i =>
            i.RuleId == "SA1303" && i.Message.Contains("MAX_RETRIES"));
    }

    [Fact]
    public async Task Detects_MultipleNamingViolationsInSameFile()
    {
        // Arrange
        var code = @"
namespace TestNamespace
{
    public class myService
    {
        private string connectionString;

        public void processData()
        {
        }
    }
}";
        CreateTestFile("MyService.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.Naming);

        // Assert
        Assert.True(result.Issues.Count >= 3, $"Expected at least 3 naming issues, found {result.Issues.Count}");
        AssertHasIssue(result, "SA1300"); // Class and method naming
        AssertHasIssue(result, "SA1309"); // Field naming
    }
}
