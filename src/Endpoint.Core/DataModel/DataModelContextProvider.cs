// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.Core.DataModel;

public class DataModelContextProvider<T> : IDataModelContextProvider<T>
    where T : IDataModelContext, new()
{
    private readonly T _context = new();

    public void Configure(Action<IDataModelContext> configure)
    {
        configure.Invoke(_context);
    }

    public Task<T> GetAsync()
    {
        return Task.FromResult(_context);
    }

}
