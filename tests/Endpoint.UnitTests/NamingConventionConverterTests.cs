// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.UnitTests;

public class NamingConventionConverterTests
{
    private readonly NamingConventionConverter _converter;

    public NamingConventionConverterTests()
    {
        _converter = new NamingConventionConverter();
    }

    #region Convert(to, value) Tests

    [Theory]
    [InlineData("myVariable", "myVariable")]
    [InlineData("MyVariable", "myVariable")]
    [InlineData("my-variable", "myVariable")]
    [InlineData("my variable", "myVariable")]
    [InlineData("MY_VARIABLE", "myVariable")]
    public void Convert_ToCamelCase_ReturnsCorrectResult(string input, string expected)
    {
        var result = _converter.Convert(NamingConvention.CamelCase, input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("myVariable", "MyVariable")]
    [InlineData("MyVariable", "MyVariable")]
    [InlineData("my-variable", "MyVariable")]
    [InlineData("my variable", "MyVariable")]
    public void Convert_ToPascalCase_ReturnsCorrectResult(string input, string expected)
    {
        var result = _converter.Convert(NamingConvention.PascalCase, input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("myVariable", "my-variable")]
    [InlineData("MyVariable", "my-variable")]
    [InlineData("my variable", "my-variable")]
    public void Convert_ToSnakeCase_ReturnsCorrectResult(string input, string expected)
    {
        var result = _converter.Convert(NamingConvention.SnakeCase, input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("myVariable", "My Variable")]
    [InlineData("MyVariable", "My Variable")]
    [InlineData("my-variable", "My Variable")]
    public void Convert_ToTitleCase_ReturnsCorrectResult(string input, string expected)
    {
        var result = _converter.Convert(NamingConvention.TitleCase, input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("myVariable", "MY_VARIABLE")]
    [InlineData("MyVariable", "MY_VARIABLE")]
    public void Convert_ToAllCaps_ReturnsCorrectResult(string input, string expected)
    {
        var result = _converter.Convert(NamingConvention.AllCaps, input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("myVariable", "my_variable")]
    [InlineData("MyVariable", "my_variable")]
    public void Convert_ToKebobCase_ReturnsCorrectResult(string input, string expected)
    {
        var result = _converter.Convert(NamingConvention.KebobCase, input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Convert_WithEmptyOrNullValue_ReturnsEmpty(string? input)
    {
        var result = _converter.Convert(NamingConvention.PascalCase, input!);
        Assert.Equal(string.Empty, result);
    }

    #endregion

    #region Convert(from, to, value) Tests

    [Theory]
    [InlineData(NamingConvention.PascalCase, NamingConvention.CamelCase, "MyVariable", "myVariable")]
    [InlineData(NamingConvention.CamelCase, NamingConvention.PascalCase, "myVariable", "MyVariable")]
    [InlineData(NamingConvention.PascalCase, NamingConvention.SnakeCase, "MyVariable", "my-variable")]
    [InlineData(NamingConvention.CamelCase, NamingConvention.TitleCase, "myVariable", "My Variable")]
    public void Convert_FromToValue_ReturnsCorrectResult(
        NamingConvention from,
        NamingConvention to,
        string input,
        string expected)
    {
        var result = _converter.Convert(from, to, input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Convert_FromToValue_WithEmptyString_ReturnsEmpty()
    {
        var result = _converter.Convert(NamingConvention.PascalCase, NamingConvention.CamelCase, string.Empty);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void Convert_FromToValue_WithNull_ReturnsEmpty()
    {
        var result = _converter.Convert(NamingConvention.PascalCase, NamingConvention.CamelCase, null!);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void Convert_WithUnknownConvention_ReturnsOriginalValue()
    {
        var result = _converter.Convert(NamingConvention.None, NamingConvention.None, "TestValue");
        Assert.Equal("TestValue", result);
    }

    #endregion

    #region GetNamingConvention Tests

    [Theory]
    [InlineData("myVariable", NamingConvention.CamelCase)]
    [InlineData("myVariableName", NamingConvention.CamelCase)]
    public void GetNamingConvention_CamelCase_ReturnsCorrect(string input, NamingConvention expected)
    {
        var result = _converter.GetNamingConvention(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("MyVariable", NamingConvention.PascalCase)]
    [InlineData("MyVariableName", NamingConvention.PascalCase)]
    public void GetNamingConvention_PascalCase_ReturnsCorrect(string input, NamingConvention expected)
    {
        var result = _converter.GetNamingConvention(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("my-variable", NamingConvention.SnakeCase)]
    [InlineData("my-variable-name", NamingConvention.SnakeCase)]
    public void GetNamingConvention_SnakeCase_ReturnsCorrect(string input, NamingConvention expected)
    {
        var result = _converter.GetNamingConvention(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("My Variable", NamingConvention.TitleCase)]
    [InlineData("My Variable Name", NamingConvention.TitleCase)]
    public void GetNamingConvention_TitleCase_ReturnsCorrect(string input, NamingConvention expected)
    {
        var result = _converter.GetNamingConvention(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void GetNamingConvention_WithEmptyOrNull_ReturnsNone(string? input)
    {
        var result = _converter.GetNamingConvention(input!);
        Assert.Equal(NamingConvention.None, result);
    }

    #endregion

    #region IsNamingConventionType Tests

    [Theory]
    [InlineData("myVariable", true)]
    [InlineData("myVariableName", true)]
    [InlineData("MyVariable", false)]
    [InlineData("my-variable", false)]
    [InlineData("my variable", false)]
    public void IsNamingConventionType_CamelCase_ReturnsCorrect(string input, bool expected)
    {
        var result = _converter.IsNamingConventionType(NamingConvention.CamelCase, input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("MyVariable", true)]
    [InlineData("MyVariableName", true)]
    [InlineData("myVariable", false)]
    [InlineData("my-variable", false)]
    [InlineData("my variable", false)]
    public void IsNamingConventionType_PascalCase_ReturnsCorrect(string input, bool expected)
    {
        var result = _converter.IsNamingConventionType(NamingConvention.PascalCase, input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("my-variable", true)]
    [InlineData("my-variable-name", true)]
    [InlineData("MyVariable", false)]
    [InlineData("my variable", false)]
    public void IsNamingConventionType_SnakeCase_ReturnsCorrect(string input, bool expected)
    {
        var result = _converter.IsNamingConventionType(NamingConvention.SnakeCase, input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("My Variable", true)]
    [InlineData("My Variable Name", true)]
    [InlineData("myVariable", false)]
    [InlineData("my-variable", false)]
    public void IsNamingConventionType_TitleCase_ReturnsCorrect(string input, bool expected)
    {
        var result = _converter.IsNamingConventionType(NamingConvention.TitleCase, input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void IsNamingConventionType_UnsupportedConvention_ThrowsNotImplementedException()
    {
        Assert.Throws<NotImplementedException>(() =>
            _converter.IsNamingConventionType(NamingConvention.AllCaps, "TEST"));
    }

    #endregion

    #region Static Method Tests - CamelCase

    [Theory]
    [InlineData("MyVariable", "myVariable")]
    [InlineData("TestString", "testString")]
    [InlineData("A", "a")]
    [InlineData("ABC", "aBC")]
    public void CamelCase_Static_ReturnsCorrectResult(string input, string expected)
    {
        var result = NamingConventionConverter.CamelCase(input);
        Assert.Equal(expected, result);
    }

    #endregion

    #region Static Method Tests - PascalCaseToTitleCase

    [Theory]
    [InlineData("MyVariable", "My Variable")]
    [InlineData("TestStringValue", "Test String Value")]
    [InlineData("ABC", "A B C")]
    [InlineData("A", "A")]
    public void PascalCaseToTitleCase_Static_ReturnsCorrectResult(string input, string expected)
    {
        var result = NamingConventionConverter.PascalCaseToTitleCase(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void PascalCaseToTitleCase_WithEmptyOrWhitespace_ReturnsEmpty(string? input)
    {
        var result = NamingConventionConverter.PascalCaseToTitleCase(input!);
        Assert.Equal(string.Empty, result);
    }

    #endregion

    #region Static Method Tests - SnakeCaseToPascalCase

    [Theory]
    [InlineData("my_variable", "MyVariable")]
    [InlineData("my-variable", "MyVariable")]
    [InlineData("my_variable_name", "MyVariableName")]
    [InlineData("test", "Test")]
    public void SnakeCaseToPascalCase_Static_ReturnsCorrectResult(string input, string expected)
    {
        var result = NamingConventionConverter.SnakeCaseToPascalCase(input);
        Assert.Equal(expected, result);
    }

    #endregion

    #region Helper Method Tests

    [Theory]
    [InlineData("my-variable", "my-Variable")]
    [InlineData("test-value-here", "test-Value-Here")]
    [InlineData("no-change", "no-Change")]
    public void FirstCharacterUpperAfterADash_ReturnsCorrectResult(string input, string expected)
    {
        var result = _converter.FirstCharacterUpperAfterADash(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("my variable", "my Variable")]
    [InlineData("test value here", "test Value Here")]
    public void FirstCharacterUpperAfterASpace_ReturnsCorrectResult(string input, string expected)
    {
        var result = _converter.FirstCharacterUpperAfterASpace(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("MyVariable", "My Variable")]
    [InlineData("TestValueHere", "Test Value Here")]
    [InlineData("ABC", "A B C")]
    public void InsertSpaceBeforeUpperCase_ReturnsCorrectResult(string input, string expected)
    {
        var result = _converter.InsertSpaceBeforeUpperCase(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void InsertSpaceBeforeUpperCase_WithEmptyOrWhitespace_ReturnsEmpty(string? input)
    {
        var result = _converter.InsertSpaceBeforeUpperCase(input!);
        Assert.Equal(string.Empty, result);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Convert_SingleCharacterLower_ToPascalCase()
    {
        var result = _converter.Convert(NamingConvention.PascalCase, "a");
        Assert.Equal("A", result);
    }

    [Fact]
    public void Convert_SingleCharacterUpper_ToCamelCase()
    {
        var result = _converter.Convert(NamingConvention.CamelCase, "A");
        Assert.Equal("a", result);
    }

    [Fact]
    public void Convert_WithMultipleDashes_ToPascalCase()
    {
        var result = _converter.Convert(NamingConvention.PascalCase, "my-complex-variable-name");
        Assert.Equal("MyComplexVariableName", result);
    }

    [Fact]
    public void Convert_WithMultipleSpaces_ToPascalCase()
    {
        var result = _converter.Convert(NamingConvention.PascalCase, "my complex variable name");
        Assert.Equal("MyComplexVariableName", result);
    }

    [Fact]
    public void Convert_ToKebobCase_WithLeadingUnderscore()
    {
        var result = _converter.Convert(NamingConvention.KebobCase, "_myVariable");
        Assert.Equal("_my_variable", result);
    }

    [Fact]
    public void Convert_ToKebobCase_WithEmptyString_ReturnsEmpty()
    {
        var result = _converter.Convert(NamingConvention.PascalCase, NamingConvention.KebobCase, string.Empty);
        Assert.Equal(string.Empty, result);
    }

    #endregion
}
