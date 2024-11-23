// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Endpoint.DotNet.Artifacts.Files;
using Endpoint.DotNet.Artifacts.Projects;
using Endpoint.DotNet.Syntax.Classes;

namespace Endpoint.DotNet.Artifacts.Solutions;

public class SolutionModel : ArtifactModel
{
    public SolutionModel()
    {
        DependOns = [];
        Projects = [];
        Files = [];
        Folders = [];
    }

    public SolutionModel(string name, string directory)
    {
        Name = name;
        Directory = directory;
        SolutionDirectory = $"{Directory}{Path.DirectorySeparatorChar}{Name}";
        Projects = [];
        DependOns = [];
        Files = [];
        Folders = [];
    }

    public SolutionModel(string name, string directory, string solutionDirectory)
    {
        Name = name;
        Directory = directory;
        SolutionDirectory = solutionDirectory;
    }

    public string Name { get; init; }

    public string Directory { get; init; }

    public List<dynamic> Folders { get; set; }

    public List<DependsOnModel> DependOns { get; set; }

    public List<ProjectModel> Projects { get; private set; }

    public List<FileModel> Files { get; set; }

    public string SrcDirectoryName { get; private set; } = "src";

    public string TestDirectoryName { get; private set; } = "tests";

    public string SrcDirectory => $"{SolutionDirectory}{Path.DirectorySeparatorChar}{SrcDirectoryName}";

    public string TestDirectory => $"{SolutionDirectory}{Path.DirectorySeparatorChar}{TestDirectoryName}";

    public string SolutionPath => $"{SolutionDirectory}{Path.DirectorySeparatorChar}{Name}.sln";

    public string SolutionDirectory { get; set; }

    public string SolultionFileName => $"{Name}.sln";

    public ProjectModel DefaultProject => Folders.First().Projects.First();

    public void RemoveAllServices()
    {
        Folders.Clear();
    }

    public ClassModel GetClass(string name, string serviceName)
    {
        throw new NotImplementedException();
    }
}