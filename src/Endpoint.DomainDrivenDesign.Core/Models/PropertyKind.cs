// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Text.Json.Serialization;

namespace Endpoint.DomainDrivenDesign.Core.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PropertyKind
{
    Guid,

    String,
    
    Int,
    
    Bool,

    Json,

    DateTime,

    Array,

    List
}
