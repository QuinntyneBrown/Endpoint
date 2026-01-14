// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.CSharp;

/// <summary>
/// Acceptance tests for security violation detection.
/// </summary>
public class SecurityViolationTests : CSharpStaticAnalysisTestBase
{
    [Fact]
    public async Task Detects_HardcodedPassword()
    {
        // Arrange
        var code = @"
namespace TestNamespace
{
    public class Config
    {
        private string password = ""supersecret123"";

        public void Connect()
        {
            var db = new Database(password);
        }

        private class Database
        {
            public Database(string pwd) { }
        }
    }
}";
        CreateTestFile("Config.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.Security);

        // Assert
        AssertHasIssue(result, "CA2100");
        AssertHasIssueMatching(result,
            i => i.Message.ToLower().Contains("password"),
            "Hardcoded password");
    }

    [Fact]
    public async Task Detects_HardcodedSecret()
    {
        // Arrange
        var code = @"
namespace TestNamespace
{
    public class ApiClient
    {
        private string secret = ""abc123xyz789"";

        public void Authenticate()
        {
            var x = secret;
        }
    }
}";
        CreateTestFile("ApiClient.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.Security);

        // Assert
        AssertHasIssue(result, "CA2100");
        AssertHasIssueMatching(result,
            i => i.Message.ToLower().Contains("secret"),
            "Hardcoded secret");
    }

    [Fact]
    public async Task Detects_HardcodedApiKey()
    {
        // Arrange
        var code = @"
namespace TestNamespace
{
    public class Service
    {
        private const string apikey = ""sk-1234567890abcdef"";

        public void CallApi()
        {
            var key = apikey;
        }
    }
}";
        CreateTestFile("Service.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.Security);

        // Assert
        AssertHasIssue(result, "CA2100");
        AssertHasIssueMatching(result,
            i => i.Message.ToLower().Contains("api key"),
            "Hardcoded API key");
    }

    [Fact]
    public async Task Detects_HardcodedConnectionString()
    {
        // Arrange
        var code = @"
namespace TestNamespace
{
    public class DatabaseService
    {
        private string connectionstring = ""Server=localhost;Database=mydb;User=admin;Password=secret"";

        public void Connect()
        {
            var conn = connectionstring;
        }
    }
}";
        CreateTestFile("DatabaseService.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.Security);

        // Assert
        AssertHasIssue(result, "CA2100");
        AssertHasIssueMatching(result,
            i => i.Message.ToLower().Contains("connection string"),
            "Hardcoded connection string");
    }

    [Fact]
    public async Task Detects_SqlInjectionWithConcatenation()
    {
        // Arrange
        var code = @"
namespace TestNamespace
{
    public class Repository
    {
        private object _context;

        public void GetUser(string userId)
        {
            var query = ""SELECT * FROM Users WHERE Id = "" + userId;
            ExecuteSqlRaw(query);
        }

        private void ExecuteSqlRaw(string sql) { }
    }
}";
        CreateTestFile("Repository.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.Security);

        // Assert
        AssertHasIssue(result, "CA3001");
        AssertHasIssueMatching(result,
            i => i.Message.Contains("SQL injection"),
            "SQL injection vulnerability");
    }

    [Fact]
    public async Task Detects_SqlInjectionWithInterpolation()
    {
        // Arrange
        var code = @"
namespace TestNamespace
{
    public class Repository
    {
        public void GetUser(string userId)
        {
            ExecuteSqlRaw($""SELECT * FROM Users WHERE Id = {userId}"");
        }

        private void ExecuteSqlRaw(string sql) { }
    }
}";
        CreateTestFile("Repository.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.Security);

        // Assert
        AssertHasIssue(result, "CA3001");
    }

    [Fact]
    public async Task NoIssues_WhenUsingConfiguration()
    {
        // Arrange
        var code = @"
using Microsoft.Extensions.Configuration;

namespace TestNamespace
{
    public class Config
    {
        private readonly IConfiguration _config;

        public Config(IConfiguration config)
        {
            _config = config;
        }

        public string GetPassword()
        {
            return _config[""Database:Password""];
        }
    }
}";
        CreateTestFile("Config.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.Security);

        // Assert
        Assert.DoesNotContain(result.Issues, i =>
            i.RuleId == "CA2100" && i.Message.Contains("password"));
    }

    [Fact]
    public async Task NoIssues_WhenUsingParameterizedQuery()
    {
        // Arrange
        var code = @"
namespace TestNamespace
{
    public class Repository
    {
        public void GetUser(string userId)
        {
            var query = ""SELECT * FROM Users WHERE Id = @userId"";
            ExecuteWithParam(query, userId);
        }

        private void ExecuteWithParam(string sql, string param) { }
    }
}";
        CreateTestFile("Repository.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.Security);

        // Assert
        Assert.DoesNotContain(result.Issues, i => i.RuleId == "CA3001");
    }

    [Fact]
    public async Task Detects_MultipleSecurityViolations()
    {
        // Arrange
        var code = @"
namespace TestNamespace
{
    public class Service
    {
        private string password = ""secret123"";
        private string apikey = ""key-abc123"";

        public void Query(string input)
        {
            ExecuteSqlRaw(""SELECT * FROM Table WHERE x = "" + input);
        }

        private void ExecuteSqlRaw(string sql) { }
    }
}";
        CreateTestFile("Service.cs", code);

        // Act
        var result = await AnalyzeCategoryAsync(IssueCategory.Security);

        // Assert
        Assert.True(result.Issues.Count >= 3, $"Expected at least 3 security issues, found {result.Issues.Count}");
    }
}
