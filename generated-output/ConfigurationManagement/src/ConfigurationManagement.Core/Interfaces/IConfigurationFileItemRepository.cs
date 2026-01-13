// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using ConfigurationManagement.Core.Entities;

namespace ConfigurationManagement.Core.Interfaces;

public interface IConfigurationFileItemRepository
{
    Task<ConfigurationFileItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ConfigurationFileItem>> GetByFileIdAsync(Guid fileId, CancellationToken cancellationToken = default);
    Task<ConfigurationFileItem> AddAsync(ConfigurationFileItem item, CancellationToken cancellationToken = default);
    Task UpdateAsync(ConfigurationFileItem item, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}