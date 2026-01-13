// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using ConfigurationManagement.Core.Entities;
using ConfigurationManagement.Core.Interfaces;
using ConfigurationManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ConfigurationManagement.Infrastructure.Repositories;

public class ConfigurationFileRepository : IConfigurationFileRepository
{
    private readonly ConfigurationManagementDbContext context;

    public ConfigurationFileRepository(ConfigurationManagementDbContext context)
    {
        this.context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<ConfigurationFile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.ConfigurationFiles
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.ConfigurationFileId == id, cancellationToken);
    }

    public async Task<ConfigurationFile?> GetByPathAsync(string path, CancellationToken cancellationToken = default)
    {
        return await context.ConfigurationFiles
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Path == path, cancellationToken);
    }

    public async Task<IEnumerable<ConfigurationFile>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.ConfigurationFiles
            .Include(x => x.Items)
            .ToListAsync(cancellationToken);
    }

    public async Task<ConfigurationFile> AddAsync(ConfigurationFile configurationFile, CancellationToken cancellationToken = default)
    {
        await context.ConfigurationFiles.AddAsync(configurationFile, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return configurationFile;
    }

    public async Task UpdateAsync(ConfigurationFile configurationFile, CancellationToken cancellationToken = default)
    {
        context.ConfigurationFiles.Update(configurationFile);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var configurationFile = await context.ConfigurationFiles.FindAsync(new object[] { id }, cancellationToken);
        if (configurationFile != null)
        {
            context.ConfigurationFiles.Remove(configurationFile);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}