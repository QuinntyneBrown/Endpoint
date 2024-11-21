// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Core.Syntax;

namespace Endpoint.Angular.Syntax;

public class FunctionModel : SyntaxModel
{
    public FunctionModel()
    {
        Imports = new List<ImportModel>();
    }

    public string Name { get; set; }

    public string Body { get; set; }

    public List<ImportModel> Imports { get; set; }
}
