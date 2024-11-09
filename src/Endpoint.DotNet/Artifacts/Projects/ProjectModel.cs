// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using Endpoint.DotNet.Artifacts.Files;
using Endpoint.DotNet.Artifacts.Folders;
using Endpoint.DotNet.Artifacts.Projects.Enums;
using static System.IO.Path;

namespace Endpoint.DotNet.Artifacts.Projects;

public class ProjectModel : ArtifactModel
{
    public ProjectModel(string dotNetProjectType, string name, string parentDirectory, List<string> references = null)
        : this(
            dotNetProjectType switch
            {
                "web" => DotNetProjectType.Web,
                "webapi" => DotNetProjectType.WebApi,
                "classlib" => DotNetProjectType.ClassLib,
                "worker" => DotNetProjectType.Worker,
                "xunit" => DotNetProjectType.XUnit,
                _ => DotNetProjectType.Console
            }, name, parentDirectory, references)
    {
        Folders = new List<FolderModel>();
        Packages = new List<PackageModel>()
        {
            // new ("StyleCop.Analyzers", "1.1.118"),
        };
    }

    public ProjectModel(DotNetProjectType dotNetProjectType, string name, string parentDirectory, List<string> references = null)
    {
        DotNetProjectType = dotNetProjectType;
        Name = name;
        Extension = dotNetProjectType == DotNetProjectType.TypeScriptStandalone ? ".esproj" : ".csproj";
        Directory = Combine(parentDirectory, name);
        References = references ?? new List<string>();
        Folders = new List<FolderModel>();
        Packages = new List<PackageModel>()
        {
            // new ("StyleCop.Analyzers", "1.1.118"),
        };
    }

    public ProjectModel(string name, string parentDirectory)
    {
        DotNetProjectType = DotNetProjectType.ClassLib;
        Name = name;
        Directory = Combine(parentDirectory, name);
        References = new List<string>();
        Folders = new List<FolderModel>();
        Packages = new List<PackageModel>()
        {
            // new ("StyleCop.Analyzers", "1.1.118"),
        };
    }

    public ProjectModel()
    {
        References = new List<string>();
        Folders = new List<FolderModel>();
        Packages = new List<PackageModel>()
        {
            // new ("StyleCop.Analyzers", "1.1.118"),
        };
    }

    public string Name { get; init; }

    public string Directory { get; init; }

    public string Path => Combine(Directory, $"{Name}{Extension}");

    public string Namespace => Name;

    public string Extension { get; set; } = ".csproj";

    public DotNetProjectType DotNetProjectType { get; set; }

    public List<FileModel> Files { get; init; } = new List<FileModel>();

    public List<PackageModel> Packages { get; init; } = new ();

    public bool HasSecrets { get; init; }

    public bool IsNugetPackage { get; init; }

    public int Order { get; set; } = 0;

    public bool GenerateDocumentationFile { get; set; }

    public List<string> Metadata { get; set; } = new List<string>();

    public List<string> References { get; set; }

    public List<string> NoWarn { get; set; } = new ()
    {
        "1998",
        "4014,",
        "SA1101",
        "SA1600,",
        "SA1200",
        "SA1633",
        "1591",
    };

    public List<FolderModel> Folders { get; set; }

    public string GetApplicationUrl(IFileSystem fileSystem)
    {
        var launchSettingsPath = System.IO.Path.Combine(Directory, "Properties", "launchSettings.json");

        var json = JsonSerializer.Deserialize<JsonNode>(fileSystem.File.ReadAllText(launchSettingsPath));

        var applicationUrl = $"{json["profiles"]["https"]["applicationUrl"]}".Split(";").First();

        return $"{applicationUrl}/";
    }
}
