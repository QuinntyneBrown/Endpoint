// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.DomainDrivenDesign.Core;

public interface IDataContextProvider
{
    Task<IDataContext> GetAsync(CancellationToken cancellationToken);

}

