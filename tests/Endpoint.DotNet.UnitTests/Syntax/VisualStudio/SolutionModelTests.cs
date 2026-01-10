// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Endpoint.DotNet.Syntax;
using Endpoint.DotNet.Syntax.VisualStudio;
using Microsoft.Extensions.DependencyInjection;

namespace Endpoint.DotNet.UnitTests.Syntax.VisualStudio;

public class SolutionModelTests
{
    [Fact]
    public async Task SyntaxGenerator_generates_solution_file_syntax_given_valid_solution_model()
    {
        // ARRANGE
        var expectedSyntax = $$"""

            """;

        var services = new ServiceCollection();

        services.AddLogging();

        services.AddDotNetServices();

        services.AddSingleton<IFileSystem, MockFileSystem>();

        var serviceProvider = services.BuildServiceProvider();

        var sut = serviceProvider.GetRequiredService<ISyntaxGenerator>();

        // ACT
        var solutionModel = new SolutionModel();

        var actualSyntax = await sut.GenerateAsync(solutionModel);

        // ASSERT
        Assert.Equal(expectedSyntax, actualSyntax);
    }
}
