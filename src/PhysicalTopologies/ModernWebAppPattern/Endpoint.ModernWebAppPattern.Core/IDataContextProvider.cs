// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text.Json;

namespace Endpoint.ModernWebAppPattern.Core;

public interface IDataContextProvider
{
    Task<IDataContext> GetAsync(string path = "", CancellationToken cancellationToken = default);

    Task<IDataContext> GetAsync(JsonElement jsonElement, CancellationToken cancellationToken = default);

}

