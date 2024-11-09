// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DotNet.Syntax.Classes;
using Endpoint.DotNet.Syntax.Documents;

namespace Endpoint.DotNet.Syntax.Units.Factories;

public interface IDocumentFactory
{
    Task<DocumentModel> CreateCommandAsync(ClassModel aggregate, RouteType routeType);

    Task<DocumentModel> CreateQueryAsync(ClassModel aggregate, RouteType routeType);
}
