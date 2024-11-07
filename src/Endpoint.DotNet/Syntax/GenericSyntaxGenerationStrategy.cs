﻿// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.DotNet.Syntax;

public abstract class GenericSyntaxGenerationStrategy<T> : IGenericSyntaxGenerationStrategy<T>
{
    public abstract Task<string> GenerateAsync(ISyntaxGenerator generator, T model);

    public async virtual Task<string> GenerateAsync(ISyntaxGenerator generator, object target)
    {
        if (target is T model)
        {
            return await GenerateAsync(generator, model);
        }

        return null;
    }

    public virtual int GetPriority() => 0;
}
