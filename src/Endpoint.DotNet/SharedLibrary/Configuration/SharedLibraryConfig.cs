// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Endpoint.DotNet.SharedLibrary.Configuration;

/// <summary>
/// Root configuration for generating a shared library.
/// </summary>
public class SharedLibraryConfig
{
    /// <summary>
    /// Gets or sets the solution configuration.
    /// </summary>
    public SolutionConfig Solution { get; set; } = new();

    /// <summary>
    /// Gets or sets the protocols configuration.
    /// </summary>
    public ProtocolsConfig Protocols { get; set; } = new();

    /// <summary>
    /// Gets or sets the serializers configuration.
    /// </summary>
    public SerializersConfig Serializers { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of services.
    /// </summary>
    public List<ServiceConfig> Services { get; set; } = new();

    /// <summary>
    /// Gets or sets the domain configuration.
    /// </summary>
    public DomainConfig Domain { get; set; } = new();

    /// <summary>
    /// Gets or sets the CCSDS packets configuration.
    /// </summary>
    public List<CcsdsPacketConfig> CcsdsPackets { get; set; } = new();
}

/// <summary>
/// Solution-level configuration.
/// </summary>
public class SolutionConfig
{
    /// <summary>
    /// Gets or sets the solution name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the root namespace.
    /// </summary>
    public string Namespace { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the output path.
    /// </summary>
    public string OutputPath { get; set; } = "./";

    /// <summary>
    /// Gets or sets the target framework.
    /// </summary>
    public string TargetFramework { get; set; } = "net9.0";

    /// <summary>
    /// Gets or sets the library name prefix for generated projects.
    /// Defaults to "Shared" (e.g., Shared.Domain, Shared.Contracts).
    /// Can be changed to another name like "Common" (e.g., Common.Domain, Common.Contracts).
    /// </summary>
    public string LibraryName { get; set; } = "Shared";
}

/// <summary>
/// Domain configuration for strongly-typed IDs and value objects.
/// </summary>
public class DomainConfig
{
    /// <summary>
    /// Gets or sets the strongly-typed ID definitions.
    /// </summary>
    public List<StronglyTypedIdConfig> StronglyTypedIds { get; set; } = new();

    /// <summary>
    /// Gets or sets the value object definitions.
    /// </summary>
    public List<ValueObjectConfig> ValueObjects { get; set; } = new();
}

/// <summary>
/// Configuration for a strongly-typed ID.
/// </summary>
public class StronglyTypedIdConfig
{
    /// <summary>
    /// Gets or sets the name of the strongly-typed ID.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the underlying type (default: Guid).
    /// </summary>
    public string UnderlyingType { get; set; } = "Guid";
}

/// <summary>
/// Configuration for a value object.
/// </summary>
public class ValueObjectConfig
{
    /// <summary>
    /// Gets or sets the name of the value object.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the properties of the value object.
    /// </summary>
    public List<PropertyConfig> Properties { get; set; } = new();
}
