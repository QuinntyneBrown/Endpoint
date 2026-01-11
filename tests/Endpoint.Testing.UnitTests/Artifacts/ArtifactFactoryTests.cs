// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Testing;
using Microsoft.Extensions.Logging.Abstractions;

namespace Endpoint.Testing.UnitTests.Artifacts;

public class ArtifactFactoryTests
{
    [Fact]
    public void ArtifactFactory_ShouldCreateInstance()
    {
        // Arrange
        var logger = NullLogger<ArtifactFactory>.Instance;

        // Act
        var factory = new ArtifactFactory(logger);

        // Assert
        Assert.NotNull(factory);
    }
}
