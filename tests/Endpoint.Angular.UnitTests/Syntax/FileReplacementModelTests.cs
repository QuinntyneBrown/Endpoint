// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Angular.Syntax;

namespace Endpoint.Angular.UnitTests.Syntax;

public class FileReplacementModelTests
{
    [Fact]
    public void FileReplacementModel_ShouldCreateInstance()
    {
        // Arrange & Act
        var model = new FileReplacementModel();

        // Assert
        Assert.NotNull(model);
    }

    [Fact]
    public void FileReplacementModel_ShouldSetProperties()
    {
        // Arrange
        var model = new FileReplacementModel
        {
            Replace = "src/environments/environment.ts",
            With = "src/environments/environment.prod.ts",
        };

        // Act & Assert
        Assert.Equal("src/environments/environment.ts", model.Replace);
        Assert.Equal("src/environments/environment.prod.ts", model.With);
    }
}
