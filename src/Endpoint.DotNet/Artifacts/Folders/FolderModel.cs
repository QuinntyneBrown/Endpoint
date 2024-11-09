// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using Endpoint.DotNet.Artifacts.Files;
using Endpoint.DotNet.Artifacts.Projects;

namespace Endpoint.DotNet.Artifacts.Folders;

public class FolderModel : ArtifactModel
{
    public FolderModel()
    {
    }

    public FolderModel(string name)
    {
        Name = name;
        SubFolders = new List<FolderModel>();
        Projects = new List<ProjectModel>();
        Files = new List<FileModel>();
    }

    public FolderModel(string name, FolderModel parent)
        : this(name, parent.Directory)
    {
        Name = name;
        Directory = Path.Combine(parent.Directory, name);
        parent.SubFolders.Add(this);
    }

    public FolderModel(string name, string parentDirectory)
    {
        Name = name;
        Directory = Path.Combine(parentDirectory, name);
        SubFolders = new List<FolderModel>();
        Projects = new List<ProjectModel>();
        Files = new List<FileModel>();
    }

    public int Priority { get; set; } = 0;

    public string Name { get; set; }

    public string Directory { get; set; }

    public List<ProjectModel> Projects { get; set; }

    public List<FolderModel> SubFolders { get; set; }

    public List<FileModel> Files { get; set; }

    public override IEnumerable<FolderModel> GetChildren()
    {
        foreach (var folder in SubFolders)
        {
            yield return folder;
        }
    }
}
