// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DotNet.Syntax.Classes;

namespace Endpoint.DotNet.Syntax.Types;

public interface ITypeFactory
{
    Task<TypeModel> Create(ClassModel @class);
}
