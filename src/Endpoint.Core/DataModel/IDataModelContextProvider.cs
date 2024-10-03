// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Core.DataModel;

public interface IDataModelContextProvider<T>
    where T : IDataModelContext, new()
{
    Task<T> GetAsync();
}