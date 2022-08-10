﻿using Endpoint.Core.Models;
using Endpoint.Core.Services;
using Endpoint.Core.ViewModels;
using System.Collections.Generic;
using Xunit;

namespace Endpoint.UnitTests
{

    public class TemplateProcessorTests
    {
        internal record MyModel(string Title);

        [Fact]
        public void Process_ShouldRenderObject()
        {
            var template = new string[1] { "{{ Title }}" };

            var sut = new LiquidTemplateProcessor();

            var result = sut.Process(template, new MyModel("Foo"));

            Assert.Contains("Foo", result);
        }

        [Fact]
        public void Process_ShouldComplextModel()
        {
            var entity = new Entity("User", new List<ClassProperty>
            {
                new ClassProperty("public","string","Name", ClassPropertyAccessor.GetPrivateSet)
            })
            {
                Namespace = "Endpoint.Domain.Models"
            };

            entity.Usings.Add("System");
            entity.Usings.Add("System.Collection.Generic");

            var template = new string[9] {
                "{% for using in Usings %}using {{ using }};",
                "{% endfor %}",
                "namespace {{ Namespace }}",
                "{",
                "    public class {{ namePascalCase }}",
                "    {",
                "{% for prop in Properties %}        {{ prop }}{% endfor %}",
                "    }",
                "}"
            };

            var sut = new LiquidTemplateProcessor();

            var result = sut.Process(template, entity.ToViewModel());

            System.IO.File.WriteAllText(@"C:\reference-architecture\foo.txt", result);

            Assert.Contains("User", result);
        }
    }
}
