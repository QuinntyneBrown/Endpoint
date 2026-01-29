// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using Endpoint.Services;
using Endpoint.DotNet.Syntax;
using Endpoint.DotNet.Syntax.Entities;
using Endpoint.DotNet.Syntax.Properties;
using Endpoint.DotNet.Syntax.Types;
using Xunit;

namespace Endpoint.UnitTests;

public class TemplateProcessorTests
{
    internal record MyModel(string title);

    [Fact]
    public void Process_ShouldRenderObject()
    {
        var template = "{{ Title }}";

        var sut = new LiquidTemplateProcessor();

        var result = sut.Process(template, new MyModel("Foo"));

        Assert.Contains("Foo", result);
    }

    [Fact]
    public void Process_ShouldComplextModel()
    {
        var entity = new EntityModel("User");

        entity.Properties = new List<PropertyModel>
        {
            new PropertyModel(entity, AccessModifier.Public, new Endpoint.DotNet.Syntax.Types.TypeModel() { Name = "string" }, "Name", PropertyAccessorModel.GetPrivateSet),
        };

        var template = new StringBuilder().AppendJoin(Environment.NewLine, new string[9]
        {
            "{% for using in Usings %}using {{ using }};",
            "{% endfor %}",
            "namespace {{ Namespace }}",
            "{",
            "    public class {{ namePascalCase }}",
            "    {",
            "{% for prop in Properties %}        {{ prop }}{% endfor %}",
            "    }",
            "}",
        }).ToString();

        var sut = new LiquidTemplateProcessor();

        var result = sut.Process(template, new { });

        Assert.NotNull(result);
    }
}
