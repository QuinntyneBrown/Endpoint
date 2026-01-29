// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Engineering.Testing.Cli.Models;

public class TestGenerationResult
{
    public bool Generated { get; set; }
    public string SourceFilePath { get; set; } = string.Empty;
    public string TestFilePath { get; set; } = string.Empty;
    public string? SkipReason { get; set; }
    public string? TestContent { get; set; }
}
