// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.DotNet.Exceptions;

public class SettingsNotFoundException : Exception
{
    public SettingsNotFoundException()
        : base("Settings Not Found.")
    {
    }
}
