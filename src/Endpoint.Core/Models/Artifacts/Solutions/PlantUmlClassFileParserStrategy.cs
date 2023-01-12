﻿using Endpoint.Core.Models.Artifacts.Files;
using Endpoint.Core.Models.Syntax.Classes;
using Endpoint.Core.Models.Syntax.Methods;
using Endpoint.Core.Models.Syntax.Properties;
using System.Linq;

namespace Endpoint.Core.Models.Artifacts.Solutions;

public class PlantUmlClassFileParserStrategy : PlantUmlParserStrategyBase<ObjectFileModel<ClassModel>>
{
    public PlantUmlClassFileParserStrategy(IServiceProvider serviceProvider) 
        : base(serviceProvider)
    { }

    public override bool CanHandle(string plantUml) => plantUml.StartsWith("class");

    protected override ObjectFileModel<ClassModel> Create(IPlantUmlParserStrategyFactory factory, string plantUml, dynamic context= null)
    {
        var plantUmlLines = plantUml.Split(Environment.NewLine);

        var name = plantUmlLines[0].Replace("class", string.Empty).Replace("{", string.Empty).Trim();

        var classModel = new ClassModel(name);

        for (var i = 1; i < plantUmlLines.Count(); i++)
        {
            var o = factory.CreateFor(plantUmlLines[i], new { TypeDeclarationModel = classModel });

            if(o is MethodModel method)
                classModel.Methods.Add(method);

            if(o is PropertyModel property)
                classModel.Properties.Add(property);
        }
        

        return new ObjectFileModel<ClassModel>(classModel, classModel.UsingDirectives, classModel.Name, context.Project.Directory, "cs");
    }
}
