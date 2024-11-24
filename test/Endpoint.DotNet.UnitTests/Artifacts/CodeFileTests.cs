// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Endpoint.Core;
using Endpoint.Core.Services;
using Endpoint.DotNet.Artifacts.Files;
using Endpoint.DotNet.Artifacts.Files.Strategies;
using Endpoint.DotNet.Syntax.Classes;
using Microsoft.Extensions.DependencyInjection;

namespace Endpoint.DotNet.UnitTests.Artifacts;

public class CodeFileTests
{
    [Fact]
    public async Task ClassCodeFileArtifactGenerationStrategy_returns_syntax_given_Class_Code_File_model()
    {
        // ARRANGE
        var expected = """
            // Copyright (c) Quinntyne Brown. All Rights Reserved.
            // Licensed under the MIT License. See License.txt in the project root for license information.

            public class Foo { }
            
            """;

        var mockFileSystem = new MockFileSystem();

        var services = new ServiceCollection();

        services.AddLogging();

        services.AddCoreServices(typeof(ClassCodeFileArtifactGenerationStrategy).Assembly);

        services.AddDotNetServices();

        services.AddSingleton<IFileSystem>(mockFileSystem);

        services.AddSingleton<ICommandService, NoOpCommandService>();

        var serviceProvider = services.BuildServiceProvider();

        var classModel = new ClassModel() { Name = "Foo" };

        var model = new CodeFileModel<ClassModel>(classModel, classModel.Name, "C:\\", ".cs");

        var artifactGenerator = ActivatorUtilities.CreateInstance<ClassCodeFileArtifactGenerationStrategy>(serviceProvider);

        // ACT
        await artifactGenerator.GenerateAsync(model);

        // ASSERT
        var actual = mockFileSystem.FileSystem.File.ReadAllText("C:\\Foo.cs");

        Assert.Equal(expected.RemoveTrivia(), actual.RemoveTrivia());
    }

}
