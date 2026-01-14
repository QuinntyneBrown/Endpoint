// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Engineering.UnitTests.StaticAnalysis.AcceptanceTests.ImplementationSpec;

/// <summary>
/// Acceptance tests for AC5.1: All source files MUST include copyright header.
/// Spec: implementation.spec.md
/// </summary>
public class CopyrightHeaderTests : StaticAnalysisTestBase
{
    [Fact]
    public async Task AC5_1_MissingCopyrightHeader_ShouldReportViolation()
    {
        // Arrange - Create a file without copyright header
        CreateCSharpFile("TestClass.cs", @"
namespace TestNamespace;

public class TestClass
{
}
");

        // Act
        var result = await RunAnalysisAsync();

        // Assert
        AssertViolationExists(result, "AC5.1");
        Assert.Contains(result.Violations, v =>
            v.RuleId == "AC5.1" &&
            v.Message.Contains("copyright header") &&
            v.SpecSource == "implementation.spec.md");
    }

    [Fact]
    public async Task AC5_1_IncorrectCopyrightHeader_ShouldReportViolation()
    {
        // Arrange - Create a file with incorrect copyright header
        CreateCSharpFile("TestClass.cs", @"// Copyright (c) Some Other Company. All Rights Reserved.
// Licensed under the Apache License.

namespace TestNamespace;

public class TestClass
{
}
");

        // Act
        var result = await RunAnalysisAsync();

        // Assert
        AssertViolationExists(result, "AC5.1");
    }

    [Fact]
    public async Task AC5_1_CorrectCopyrightHeader_ShouldPass()
    {
        // Arrange - Create a file with correct copyright header
        CreateCSharpFileWithHeader("TestClass.cs", @"
namespace TestNamespace;

public class TestClass
{
}
");

        // Act
        var result = await RunAnalysisAsync();

        // Assert
        AssertNoViolation(result, "AC5.1");
    }

    [Fact]
    public async Task AC5_1_CopyrightHeaderWithExtraWhitespace_ShouldReportViolation()
    {
        // Arrange - Create a file with copyright header that has extra whitespace
        CreateCSharpFile("TestClass.cs", @"  // Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace TestNamespace;

public class TestClass
{
}
");

        // Act
        var result = await RunAnalysisAsync();

        // Assert
        AssertViolationExists(result, "AC5.1");
    }
}
