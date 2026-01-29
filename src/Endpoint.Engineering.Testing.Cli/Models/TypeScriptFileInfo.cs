// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Engineering.Testing.Cli.Models;

public class TypeScriptFileInfo
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public AngularFileType FileType { get; set; }
    public List<string> Imports { get; set; } = [];
    public List<TypeScriptMethod> Methods { get; set; } = [];
    public List<TypeScriptProperty> Properties { get; set; } = [];
    public List<string> Dependencies { get; set; } = [];
    public string? Selector { get; set; }
    public string? TemplateUrl { get; set; }
    public string? StyleUrls { get; set; }
    public bool IsStandalone { get; set; }
    public string Content { get; set; } = string.Empty;
}

public class TypeScriptMethod
{
    public string Name { get; set; } = string.Empty;
    public string AccessModifier { get; set; } = "public";
    public string ReturnType { get; set; } = "void";
    public List<TypeScriptParameter> Parameters { get; set; } = [];
    public bool IsAsync { get; set; }
    public bool IsStatic { get; set; }
    public string Body { get; set; } = string.Empty;
}

public class TypeScriptProperty
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "any";
    public string AccessModifier { get; set; } = "public";
    public bool IsReadonly { get; set; }
    public bool IsOptional { get; set; }
    public string? DefaultValue { get; set; }
    public string? Decorator { get; set; }
}

public class TypeScriptParameter
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "any";
    public bool IsOptional { get; set; }
    public string? DefaultValue { get; set; }
}
