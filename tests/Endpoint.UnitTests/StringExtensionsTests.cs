// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.UnitTests;

public class StringExtensionsTests
{
    #region RemoveTrivia Tests

    [Fact]
    public void RemoveTrivia_WithSpaces_RemovesSpaces()
    {
        var input = "Hello World";
        var result = input.RemoveTrivia();
        Assert.Equal("HelloWorld", result);
    }

    [Fact]
    public void RemoveTrivia_WithNewLines_RemovesNewLines()
    {
        var input = $"Hello{Environment.NewLine}World";
        var result = input.RemoveTrivia();
        Assert.Equal("HelloWorld", result);
    }

    [Fact]
    public void RemoveTrivia_WithSpacesAndNewLines_RemovesBoth()
    {
        var input = $"Hello World{Environment.NewLine}Test Value";
        var result = input.RemoveTrivia();
        Assert.Equal("HelloWorldTestValue", result);
    }

    [Fact]
    public void RemoveTrivia_WithNoTrivia_ReturnsOriginal()
    {
        var input = "HelloWorld";
        var result = input.RemoveTrivia();
        Assert.Equal("HelloWorld", result);
    }

    [Fact]
    public void RemoveTrivia_WithEmptyString_ReturnsEmpty()
    {
        var input = string.Empty;
        var result = input.RemoveTrivia();
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void RemoveTrivia_WithMultipleSpaces_RemovesAllSpaces()
    {
        var input = "Hello   World   Test";
        var result = input.RemoveTrivia();
        Assert.Equal("HelloWorldTest", result);
    }

    #endregion

    #region Indent Tests

    [Fact]
    public void Indent_SingleLine_AddsCorrectIndentation()
    {
        var input = "Hello";
        var result = input.Indent(1);
        Assert.Equal("    Hello", result);
    }

    [Fact]
    public void Indent_SingleLine_WithIndentLevel2_AddsCorrectIndentation()
    {
        var input = "Hello";
        var result = input.Indent(2);
        Assert.Equal("        Hello", result);
    }

    [Fact]
    public void Indent_SingleLine_WithCustomSpaces_AddsCorrectIndentation()
    {
        var input = "Hello";
        var result = input.Indent(1, 2);
        Assert.Equal("  Hello", result);
    }

    [Fact]
    public void Indent_MultiLine_IndentsAllLines()
    {
        var input = $"Line1{Environment.NewLine}Line2{Environment.NewLine}Line3";
        var result = input.Indent(1);
        var expected = $"    Line1{Environment.NewLine}    Line2{Environment.NewLine}    Line3";
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Indent_MultiLine_WithEmptyLines_PreservesEmptyLines()
    {
        var input = $"Line1{Environment.NewLine}{Environment.NewLine}Line3";
        var result = input.Indent(1);
        var expected = $"    Line1{Environment.NewLine}{Environment.NewLine}    Line3";
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Indent_EmptyString_ReturnsEmpty()
    {
        var input = string.Empty;
        var result = input.Indent(1);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void Indent_ZeroIndent_ReturnsOriginal()
    {
        var input = "Hello";
        var result = input.Indent(0);
        Assert.Equal("Hello", result);
    }

    [Fact]
    public void Indent_WithLargeIndent_AddsCorrectSpaces()
    {
        var input = "Hello";
        var result = input.Indent(5, 4);
        Assert.Equal("                    Hello", result);
    }

    [Fact]
    public void Indent_MultiLine_WithDifferentSpaces_IndentsCorrectly()
    {
        var input = $"Line1{Environment.NewLine}Line2";
        var result = input.Indent(2, 2);
        var expected = $"    Line1{Environment.NewLine}    Line2";
        Assert.Equal(expected, result);
    }

    #endregion

    #region Remove Tests

    [Fact]
    public void Remove_ExistingSubstring_RemovesIt()
    {
        var input = "Hello World";
        var result = input.Remove("World");
        Assert.Equal("Hello ", result);
    }

    [Fact]
    public void Remove_NonExistingSubstring_ReturnsOriginal()
    {
        var input = "Hello World";
        var result = input.Remove("Test");
        Assert.Equal("Hello World", result);
    }

    [Fact]
    public void Remove_MultipleOccurrences_RemovesAll()
    {
        var input = "Hello Hello Hello";
        var result = input.Remove("Hello");
        Assert.Equal("  ", result);
    }

    [Fact]
    public void Remove_EmptySubstring_ReturnsOriginal()
    {
        var input = "Hello World";
        var result = input.Remove(string.Empty);
        Assert.Equal("Hello World", result);
    }

    [Fact]
    public void Remove_EntireString_ReturnsEmpty()
    {
        var input = "Hello";
        var result = input.Remove("Hello");
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void Remove_SingleCharacter_RemovesAllOccurrences()
    {
        var input = "Hello";
        var result = input.Remove("l");
        Assert.Equal("Heo", result);
    }

    #endregion

    #region GetResourceName Tests

    [Fact]
    public void GetResourceName_ExactMatch_ReturnsMatch()
    {
        var collection = new[] { "Resource.Test.txt", "Resource.Other.txt" };
        var result = collection.GetResourceName("Test.txt");
        Assert.Equal("Resource.Test.txt", result);
    }

    [Fact]
    public void GetResourceName_WithTxtExtensionFallback_ReturnsMatch()
    {
        var collection = new[] { "Resource.Test.txt", "Resource.Other.txt" };
        var result = collection.GetResourceName("Test");
        Assert.Equal("Resource.Test.txt", result);
    }

    [Fact]
    public void GetResourceName_NoMatch_ReturnsNull()
    {
        var collection = new[] { "Resource.Test.txt", "Resource.Other.txt" };
        var result = collection.GetResourceName("NotFound");
        Assert.Null(result);
    }

    [Fact]
    public void GetResourceName_EmptyCollection_ReturnsNull()
    {
        var collection = Array.Empty<string>();
        var result = collection.GetResourceName("Test");
        Assert.Null(result);
    }

    [Fact]
    public void GetResourceName_PartialMatch_ReturnsCorrectResource()
    {
        var collection = new[]
        {
            "Namespace.Resources.Template.txt",
            "Namespace.Resources.Other.Template.txt"
        };
        var result = collection.GetResourceName("Template.txt");
        Assert.Equal("Namespace.Resources.Template.txt", result);
    }

    [Fact]
    public void GetResourceName_WithDotTxtExtension_FindsResource()
    {
        var collection = new[] { "Assembly.Resource.MyTemplate.txt" };
        var result = collection.GetResourceName("MyTemplate");
        Assert.Equal("Assembly.Resource.MyTemplate.txt", result);
    }

    #endregion

    #region GetNameAndType Tests

    [Fact]
    public void GetNameAndType_WithType_ReturnsNameAndType()
    {
        var input = "propertyName:string";
        var result = input.GetNameAndType();
        Assert.Equal("propertyName", result.Item1);
        Assert.Equal("string", result.Item2);
    }

    [Fact]
    public void GetNameAndType_WithoutType_ReturnsNameAndEmptyType()
    {
        var input = "propertyName";
        var result = input.GetNameAndType();
        Assert.Equal("propertyName", result.Item1);
        Assert.Equal(string.Empty, result.Item2);
    }

    [Fact]
    public void GetNameAndType_WithComplexType_ReturnsCorrectly()
    {
        var input = "items:List<string>";
        var result = input.GetNameAndType();
        Assert.Equal("items", result.Item1);
        Assert.Equal("List<string>", result.Item2);
    }

    [Fact]
    public void GetNameAndType_WithMultipleColons_SplitsAtFirst()
    {
        var input = "name:Type:Extra";
        var result = input.GetNameAndType();
        Assert.Equal("name", result.Item1);
        Assert.Equal("Type", result.Item2);
    }

    [Fact]
    public void GetNameAndType_EmptyString_ReturnsEmptyNameAndType()
    {
        var input = string.Empty;
        var result = input.GetNameAndType();
        Assert.Equal(string.Empty, result.Item1);
        Assert.Equal(string.Empty, result.Item2);
    }

    [Fact]
    public void GetNameAndType_WithSpaces_PreservesSpaces()
    {
        var input = "my property:my type";
        var result = input.GetNameAndType();
        Assert.Equal("my property", result.Item1);
        Assert.Equal("my type", result.Item2);
    }

    [Fact]
    public void GetNameAndType_OnlyColon_ReturnsTwoEmptyStrings()
    {
        var input = ":";
        var result = input.GetNameAndType();
        Assert.Equal(string.Empty, result.Item1);
        Assert.Equal(string.Empty, result.Item2);
    }

    [Fact]
    public void GetNameAndType_ColonAtStart_ReturnsEmptyNameWithType()
    {
        var input = ":string";
        var result = input.GetNameAndType();
        Assert.Equal(string.Empty, result.Item1);
        Assert.Equal("string", result.Item2);
    }

    [Fact]
    public void GetNameAndType_ColonAtEnd_ReturnsNameWithEmptyType()
    {
        var input = "name:";
        var result = input.GetNameAndType();
        Assert.Equal("name", result.Item1);
        Assert.Equal(string.Empty, result.Item2);
    }

    #endregion
}
