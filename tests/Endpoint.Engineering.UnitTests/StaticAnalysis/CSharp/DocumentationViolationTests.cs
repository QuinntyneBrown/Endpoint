// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.CSharp;

/// <summary>
/// Acceptance tests for documentation violation detection.
/// </summary>
public class DocumentationViolationTests : CSharpStaticAnalysisTestBase
{
    [Fact]
    public async Task Detects_PublicClassWithoutDocumentation()
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
        var result = await AnalyzeCategoryAsync(IssueCategory.Documentation);

        // Assert
        AssertHasIssue(result, "SA1600");
        AssertHasIssueMatching(result,
            i => i.Message.Contains("OrderService") && i.Message.Contains("XML documentation"),
            "Public class without documentation");
    }

    [Fact]
    public async Task Detects_PublicInterfaceWithoutDocumentation()
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
        var result = await AnalyzeCategoryAsync(IssueCategory.Documentation);

        // Assert
        AssertHasIssue(result, "SA1600");
        AssertHasIssueMatching(result,
            i => i.Message.Contains("IOrderRepository") && i.Message.Contains("interface"),
            "Public interface without documentation");
    }

    [Fact]
    public async Task Detects_PublicMethodWithoutDocumentation()
    {
        // Arrange
        var code = @"
namespace TestNamespace
{
    /// <summary>
    /// Service for processing orders.
    /// </summary>
    public class OrderService
    {
        public void ProcessOrder(int orderId)
        {
        }
    }
}";
        CreateTestFile("OrderService.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.Documentation);

        // Assert
        AssertHasIssue(result, "SA1600");
        AssertHasIssueMatching(result,
            i => i.Message.Contains("ProcessOrder") && i.Message.Contains("method"),
            "Public method without documentation");
    }

    [Fact]
    public async Task NoIssues_WhenPublicClassHasDocumentation()
    {
        // Arrange
        var code = @"
namespace TestNamespace
{
    /// <summary>
    /// Service for managing orders.
    /// </summary>
    public class OrderService
    {
    }
}";
        CreateTestFile("OrderService.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.Documentation);

        // Assert
        Assert.DoesNotContain(result.Issues, i =>
            i.RuleId == "SA1600" && i.Message.Contains("OrderService"));
    }

    [Fact]
    public async Task NoIssues_WhenPublicInterfaceHasDocumentation()
    {
        // Arrange
        var code = @"
namespace TestNamespace
{
    /// <summary>
    /// Repository for order data.
    /// </summary>
    public interface IOrderRepository
    {
        /// <summary>
        /// Saves the order.
        /// </summary>
        void Save();
    }
}";
        CreateTestFile("IOrderRepository.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.Documentation);

        // Assert
        Assert.DoesNotContain(result.Issues, i =>
            i.RuleId == "SA1600" && i.Message.Contains("IOrderRepository"));
    }

    [Fact]
    public async Task NoIssues_WhenPublicMethodHasDocumentation()
    {
        // Arrange
        var code = @"
namespace TestNamespace
{
    /// <summary>
    /// Service for orders.
    /// </summary>
    public class OrderService
    {
        /// <summary>
        /// Processes the specified order.
        /// </summary>
        /// <param name=""orderId"">The order identifier.</param>
        public void ProcessOrder(int orderId)
        {
        }
    }
}";
        CreateTestFile("OrderService.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.Documentation);

        // Assert
        Assert.DoesNotContain(result.Issues, i =>
            i.RuleId == "SA1600" && i.Message.Contains("ProcessOrder"));
    }

    [Fact]
    public async Task NoIssues_ForPrivateClassWithoutDocumentation()
    {
        // Arrange
        var code = @"
namespace TestNamespace
{
    class InternalService
    {
        void DoWork()
        {
        }
    }
}";
        CreateTestFile("InternalService.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.Documentation);

        // Assert
        Assert.DoesNotContain(result.Issues, i =>
            i.RuleId == "SA1600" && i.Message.Contains("InternalService"));
    }

    [Fact]
    public async Task NoIssues_ForPrivateMethodWithoutDocumentation()
    {
        // Arrange
        var code = @"
namespace TestNamespace
{
    /// <summary>
    /// Service class.
    /// </summary>
    public class Service
    {
        private void HelperMethod()
        {
        }
    }
}";
        CreateTestFile("Service.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.Documentation);

        // Assert
        Assert.DoesNotContain(result.Issues, i =>
            i.RuleId == "SA1600" && i.Message.Contains("HelperMethod"));
    }

    [Fact]
    public async Task Detects_MultipleDocumentationViolations()
    {
        // Arrange
        var code = @"
namespace TestNamespace
{
    public interface IService
    {
        void DoWork();
    }

    public class Service : IService
    {
        public void DoWork()
        {
        }

        public void AnotherMethod()
        {
        }
    }
}";
        CreateTestFile("Service.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.Documentation);

        // Assert
        var docIssues = result.Issues.Where(i => i.RuleId == "SA1600").ToList();
        Assert.True(docIssues.Count >= 3,
            $"Expected at least 3 documentation issues, found {docIssues.Count}");
    }
}
