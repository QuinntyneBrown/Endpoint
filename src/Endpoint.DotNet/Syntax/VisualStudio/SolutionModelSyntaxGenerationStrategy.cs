// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text;

namespace Endpoint.DotNet.Syntax.VisualStudio.Strategies;

public class SolutionModelSyntaxGenerationStrategy : GenericSyntaxGenerationStrategy<SolutionModel>
{
    public override async Task<string> GenerateAsync(ISyntaxGenerator syntaxGenerator, SolutionModel model)
    {
        var stringBuilder = new StringBuilder();

        return stringBuilder.ToString();
    }
}
