// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace Endpoint.DotNet.Artifacts.PlantUml.Models;

public class PlantUmlSolutionModel
{
    public PlantUmlSolutionModel()
    {
        Documents = [];
    }

    public string Name { get; set; }

    public string SourcePath { get; set; }

    public List<PlantUmlDocumentModel> Documents { get; set; }

    public IEnumerable<PlantUmlClassModel> GetAllClasses()
    {
        foreach (var doc in Documents)
        {
            foreach (var cls in doc.Classes)
            {
                yield return cls;
            }

            foreach (var pkg in doc.Packages)
            {
                foreach (var cls in pkg.Classes)
                {
                    yield return cls;
                }
            }
        }
    }

    public IEnumerable<PlantUmlEnumModel> GetAllEnums()
    {
        foreach (var doc in Documents)
        {
            foreach (var enm in doc.Enums)
            {
                yield return enm;
            }

            foreach (var pkg in doc.Packages)
            {
                foreach (var enm in pkg.Enums)
                {
                    yield return enm;
                }
            }
        }
    }

    public IEnumerable<PlantUmlRelationshipModel> GetAllRelationships()
    {
        return Documents.SelectMany(d => d.Relationships);
    }

    public IEnumerable<PlantUmlClassModel> GetAggregates()
    {
        return GetAllClasses().Where(c => c.IsAggregate);
    }

    public IEnumerable<PlantUmlClassModel> GetEntities()
    {
        return GetAllClasses().Where(c => c.IsEntity);
    }

    public IEnumerable<PlantUmlComponentModel> GetAllComponents()
    {
        return Documents.SelectMany(d => d.Components);
    }
}
