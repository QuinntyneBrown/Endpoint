// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.Testing.Cli.Models;

namespace Endpoint.Engineering.Testing.Cli.Services;

public interface ITypeScriptParser
{
    TypeScriptFileInfo Parse(string filePath, string content);
}
