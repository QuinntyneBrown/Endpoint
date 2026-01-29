// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Services;

namespace Endpoint.DotNet.Extensions;

public static class TemplateLocatorExtensions
{
    public static string GetCopyright(this ITemplateLocator templateLocator)
    {
        return templateLocator.Get("Copyright");
    }
}
