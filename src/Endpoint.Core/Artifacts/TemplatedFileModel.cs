// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Core.Syntax;

namespace Endpoint.Core.Artifacts;

public class TemplatedFileModel : FileModel
{
    public TemplatedFileModel(string templateName, string name, string directory, string extension, Dictionary<string, object> tokens = null)
        : base(name, directory, extension)
    {
        Tokens = new Dictionary<string, object>();
        Template = templateName;

        if (tokens != null)
        {
            foreach (var token in tokens)
            {
                Tokens.TryAdd(token.Key, token.Value);
            }
        }
    }

    public string Template { get; init; }

    public Dictionary<string, object> Tokens { get; init; } = new ();
}
