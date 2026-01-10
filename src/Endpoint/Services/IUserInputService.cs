// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text.Json;

namespace Endpoint.Services;

public interface IUserInputService
{
    Task<JsonElement> ReadJsonAsync(string defaultTemplate);
}