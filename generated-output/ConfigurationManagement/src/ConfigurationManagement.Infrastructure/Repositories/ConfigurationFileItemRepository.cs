// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using ConfigurationManagement.Core.Entities;
using ConfigurationManagement.Core.Interfaces;
using ConfigurationManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ConfigurationManagement.Infrastructure.Repositories;

public class ConfigurationFileItemRepository : IConfigurationFileItemRepository
{
    private readonly ConfigurationManagementDbContext context;

    public ConfigurationFileItemRepository(ConfigurationManagementDbContext context)
    {
        this.context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<ConfigurationFileItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.ConfigurationFileItems.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<ConfigurationFileItem>> GetByFileIdAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        return await context.ConfigurationFileItems
            .Where(x => x.ConfigurationFileId == fileId)
            .ToListAsync(cancellationToken);
    }

    public async Task<ConfigurationFileItem> AddAsync(ConfigurationFileItem item, CancellationToken cancellationToken = default)
    {
        await context.ConfigurationFileItems.AddAsync(item, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return item;
    }

    public async Task UpdateAsync(ConfigurationFileItem item, CancellationToken cancellationToken = default)
    {
        context.ConfigurationFileItems.Update(item);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await context.ConfigurationFileItems.FindAsync(new object[] { id }, cancellationToken);
        if (item != null)
        {
            context.ConfigurationFileItems.Remove(item);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}