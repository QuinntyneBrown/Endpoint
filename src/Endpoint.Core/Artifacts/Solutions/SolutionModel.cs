// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Endpoint.Core.Artifacts.Files;
using Endpoint.Core.Artifacts.Folders;
using Endpoint.Core.Artifacts.Projects;
using Endpoint.Core.Syntax.Classes;

namespace Endpoint.Core.Artifacts.Solutions;

public class SolutionModel : ArtifactModel
{
    public SolutionModel()
    {
        DependOns = new List<DependsOnModel>();
        Projects = new List<ProjectModel>();
        Files = new List<FileModel>();
        Folders = new List<FolderModel>();
    }

    public SolutionModel(string name, string directory)
    {
        Name = name;
        Directory = directory;
        SolutionDirectory = $"{Directory}{Path.DirectorySeparatorChar}{Name}";
        Projects = new List<ProjectModel>();
        DependOns = new List<DependsOnModel>();
        Files = new List<FileModel>();
        Folders = new List<FolderModel>();
    }

    public SolutionModel(string name, string directory, string solutionDirectory)
    {
        Name = name;
        Directory = directory;
        SolutionDirectory = solutionDirectory;
    }

    public string Name { get; init; }

    public string Directory { get; init; }

    public List<FolderModel> Folders { get; set; }

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
        foreach (var folder in Folders)
        {
            foreach (var project in string.IsNullOrEmpty(serviceName) ? folder.Projects : folder.Projects.Where(x => x.Name == serviceName))
            {
                foreach (var file in project.Files)
                {
                    if (file is CodeFileModel<ClassModel> classFileModel)
                    {
                        if (classFileModel.Object.Name.Split('.').Last() == name)
                        {
                            return classFileModel.Object;
                        }
                    }
                }
            }
        }

        return null;
    }
}