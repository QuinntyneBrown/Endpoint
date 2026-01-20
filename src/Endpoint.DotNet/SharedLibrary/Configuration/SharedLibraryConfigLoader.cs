// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO.Abstractions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Endpoint.DotNet.SharedLibrary.Configuration;

/// <summary>
/// Loads shared library configuration from YAML files.
/// </summary>
public class SharedLibraryConfigLoader : ISharedLibraryConfigLoader
{
    private readonly IFileSystem _fileSystem;
    private readonly IDeserializer _deserializer;

    public SharedLibraryConfigLoader(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
    }

    /// <inheritdoc />
    public async Task<SharedLibraryConfig> LoadFromFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
        }

        if (!_fileSystem.File.Exists(filePath))
        {
            throw new FileNotFoundException($"Configuration file not found: {filePath}", filePath);
        }

        var content = await _fileSystem.File.ReadAllTextAsync(filePath, cancellationToken);
        return LoadFromString(content);
    }

    /// <inheritdoc />
    public SharedLibraryConfig LoadFromString(string yamlContent)
    {
        if (string.IsNullOrWhiteSpace(yamlContent))
        {
            throw new ArgumentException("YAML content cannot be null or empty.", nameof(yamlContent));
        }

        var config = _deserializer.Deserialize<SharedLibraryConfig>(yamlContent);
        return config ?? new SharedLibraryConfig();
    }

    /// <inheritdoc />
    public IReadOnlyList<string> Validate(SharedLibraryConfig config)
    {
        var errors = new List<string>();

        if (config == null)
        {
            errors.Add("Configuration cannot be null.");
            return errors;
        }

        // Validate solution config
        if (string.IsNullOrWhiteSpace(config.Solution.Name))
        {
            errors.Add("Solution name is required.");
        }

        // Validate services
        foreach (var service in config.Services)
        {
            if (string.IsNullOrWhiteSpace(service.Name))
            {
                errors.Add("Service name is required.");
                continue;
            }

            // Validate events
            foreach (var evt in service.Events)
            {
                if (string.IsNullOrWhiteSpace(evt.Name))
                {
                    errors.Add($"Event name is required in service '{service.Name}'.");
                }

                ValidateProperties(evt.Properties, $"event '{evt.Name}' in service '{service.Name}'", errors);
            }

            // Validate commands
            foreach (var cmd in service.Commands)
            {
                if (string.IsNullOrWhiteSpace(cmd.Name))
                {
                    errors.Add($"Command name is required in service '{service.Name}'.");
                }

                ValidateProperties(cmd.Properties, $"command '{cmd.Name}' in service '{service.Name}'", errors);
            }
        }

        // Validate CCSDS packets
        var apids = new HashSet<int>();
        foreach (var packet in config.CcsdsPackets)
        {
            if (string.IsNullOrWhiteSpace(packet.Name))
            {
                errors.Add("CCSDS packet name is required.");
                continue;
            }

            if (packet.Apid < 0 || packet.Apid > 2047)
            {
                errors.Add($"CCSDS packet '{packet.Name}' has invalid APID: {packet.Apid}. Must be 0-2047.");
            }

            if (!apids.Add(packet.Apid))
            {
                errors.Add($"Duplicate APID {packet.Apid} found for packet '{packet.Name}'.");
            }

            // Validate fields
            foreach (var field in packet.Fields)
            {
                if (string.IsNullOrWhiteSpace(field.Name))
                {
                    errors.Add($"Field name is required in CCSDS packet '{packet.Name}'.");
                }

                if (field.BitSize <= 0)
                {
                    errors.Add($"Field '{field.Name}' in packet '{packet.Name}' has invalid bit size: {field.BitSize}.");
                }
            }
        }

        // Validate domain types
        foreach (var id in config.Domain.StronglyTypedIds)
        {
            if (string.IsNullOrWhiteSpace(id.Name))
            {
                errors.Add("Strongly-typed ID name is required.");
            }
        }

        foreach (var vo in config.Domain.ValueObjects)
        {
            if (string.IsNullOrWhiteSpace(vo.Name))
            {
                errors.Add("Value object name is required.");
            }

            ValidateProperties(vo.Properties, $"value object '{vo.Name}'", errors);
        }

        return errors;
    }

    private void ValidateProperties(List<PropertyConfig> properties, string context, List<string> errors)
    {
        var keys = new HashSet<int>();
        foreach (var prop in properties)
        {
            if (string.IsNullOrWhiteSpace(prop.Name))
            {
                errors.Add($"Property name is required in {context}.");
            }

            if (string.IsNullOrWhiteSpace(prop.Type))
            {
                errors.Add($"Property type is required for '{prop.Name}' in {context}.");
            }

            if (prop.Key.HasValue)
            {
                if (prop.Key.Value < 0)
                {
                    errors.Add($"Property '{prop.Name}' in {context} has invalid key: {prop.Key.Value}.");
                }

                if (!keys.Add(prop.Key.Value))
                {
                    errors.Add($"Duplicate key {prop.Key.Value} found for property '{prop.Name}' in {context}.");
                }
            }
        }
    }
}
