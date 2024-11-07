// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.DotNet.Options;

namespace Endpoint.DotNet.Services;

public interface ISettingsProvider
{
    dynamic Get(string directory = null);
}
