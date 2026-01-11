// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text.Json.Serialization;

namespace Endpoint.Engineering.DomainDrivenDesign.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RequestKind
{

    Get,
    GetById,
    Create,
    Update,
    Delete,
}
