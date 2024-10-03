// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.DotNet.Syntax;

public class SyntaxReferenceModel
{
    public SyntaxReferenceModel(string syntax)
    {
        Syntax = syntax;
    }

    public string Syntax { get; set; }
}