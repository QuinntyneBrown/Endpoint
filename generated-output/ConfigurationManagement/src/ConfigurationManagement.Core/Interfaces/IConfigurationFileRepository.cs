// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using ConfigurationManagement.Core.Entities;

namespace ConfigurationManagement.Core.Interfaces;

public interface IConfigurationFileRepository
{
    Task<ConfigurationFile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ConfigurationFile?> GetByPathAsync(string path, CancellationToken cancellationToken = default);
    Task<IEnumerable<ConfigurationFile>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ConfigurationFile> AddAsync(ConfigurationFile configurationFile, CancellationToken cancellationToken = default);
    Task UpdateAsync(ConfigurationFile configurationFile, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}