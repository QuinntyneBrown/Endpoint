// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Endpoint.DotNet;
using Endpoint.Core.Artifacts;
using Endpoint.DotNet.Artifacts.Files;
using Endpoint.DotNet.Syntax;
using Endpoint.DotNet.Syntax.Classes;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Endpoint.UnitTests;

public class ClassObjectFileArtifactGenerationStrategyTests
{
    public ClassObjectFileArtifactGenerationStrategyTests()
    {
    }

    [Fact]
    public async Task Test()
    {
        var services = new ServiceCollection();

        services.AddLogging();

        services.AddCoreServices<CodeGeneratorApplication>();

        var container = services.BuildServiceProvider();

        var sut = container.GetRequiredService<IArtifactGenerator>();

        var classModel = new ClassModel("Foo");

        var objectFileModel = new CodeFileModel<ClassModel>(classModel, new List<UsingModel>()
        {
            new UsingModel() { Name = "Sytem" },
        }, "Foo", "directory", "extension");

        await sut.GenerateAsync(objectFileModel);
    }
}
