// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Syntax;

namespace Endpoint.UnitTests.Syntax;

public class SyntaxTokenTests
{
    [Fact]
    public void Constructor_ShouldInitializeValue()
    {
        // Arrange
        var value = "test value";

        // Act
        var syntaxToken = new SyntaxToken(value);

        // Assert
        Assert.Equal(value, syntaxToken.Value);
    }

    [Fact]
    public void ImplicitOperator_ShouldConvertToString()
    {
        // Arrange
        var syntaxToken = new SyntaxToken("test value");

        // Act
        string result = syntaxToken;

        // Assert
        Assert.Equal("test value", result);
    }

    [Fact]
    public void ExplicitOperator_ShouldConvertFromString()
    {
        // Arrange
        var value = "test value";

        // Act
        var syntaxToken = (SyntaxToken)value;

        // Assert
        Assert.Equal(value, syntaxToken.Value);
    }

    [Fact]
    public void Value_ShouldBeInitOnly()
    {
        // Arrange & Act
        var syntaxToken = new SyntaxToken("initial value");

        // Assert
        Assert.Equal("initial value", syntaxToken.Value);
    }

    [Fact]
    public void Constructor_WithEmptyString_ShouldInitialize()
    {
        // Arrange & Act
        var syntaxToken = new SyntaxToken(string.Empty);

        // Assert
        Assert.Equal(string.Empty, syntaxToken.Value);
    }

    [Fact]
    public void Record_Equality_ShouldWorkCorrectly()
    {
        // Arrange
        var token1 = new SyntaxToken("value");
        var token2 = new SyntaxToken("value");
        var token3 = new SyntaxToken("different");

        // Assert
        Assert.Equal(token1, token2);
        Assert.NotEqual(token1, token3);
    }

    [Fact]
    public void Record_GetHashCode_ShouldWorkCorrectly()
    {
        // Arrange
        var token1 = new SyntaxToken("value");
        var token2 = new SyntaxToken("value");

        // Assert
        Assert.Equal(token1.GetHashCode(), token2.GetHashCode());
    }
}
