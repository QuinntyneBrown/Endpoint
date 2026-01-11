// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Testing;
using Microsoft.Extensions.Logging.Nulls;

namespace Endpoint.Testing.UnitTests.Syntax;

public class SyntaxFactoryTests
{
    [Fact]
    public void SyntaxFactory_ShouldCreateInstance()
    {
        // Arrange
        var logger = NullLogger<SyntaxFactory>.Instance;

        // Act
        var factory = new SyntaxFactory(logger);

        // Assert
        Assert.NotNull(factory);
    }
}
