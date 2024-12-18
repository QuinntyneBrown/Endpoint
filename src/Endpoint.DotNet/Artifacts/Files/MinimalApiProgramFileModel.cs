// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using Endpoint.DotNet.Syntax.Entities;
using Endpoint.DotNet.Syntax.WebApplications;

namespace Endpoint.DotNet.Artifacts.Files;

public class MinimalApiProgramFileModel : FileModel
{
    public MinimalApiProgramFileModel(string title, string directory, string apiNamespace, string dbContextName, List<EntityModel> entities)
        : base("Program", directory, ".cs")
    {
        WebApplicationBuilder = new WebApplicationBuilderModel(title, dbContextName);

        WebApplication = new WebApplicationModel(title, dbContextName, entities);

        Usings = new List<string>()
        {
            "Microsoft.EntityFrameworkCore",
            "Microsoft.OpenApi.Models",
            "System.Reflection",
        };
        ApiNamespace = apiNamespace;
        DbContextName = dbContextName;
    }

    public WebApplicationModel WebApplication { get; set; }

    public WebApplicationBuilderModel WebApplicationBuilder { get; set; }

    public string DbContextName { get; private set; }

    public string ApiNamespace { get; set; }

    public List<string> Usings { get; private set; }
}
