// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Extensions.Logging;
using System.IO.Abstractions;
using System.Text.Json;

namespace Endpoint.ModernWebAppPattern;

public class FileSystemDataContextProvider : IDataContextProvider
{
    private readonly ILogger<FileSystemDataContextProvider> _logger;
    private readonly IFileSystem _fileSystem;
    private IDataContext _context = default!;

    public FileSystemDataContextProvider(ILogger<FileSystemDataContextProvider> logger, IFileSystem fileSystem)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(fileSystem);

        _logger = logger;
        _fileSystem = fileSystem;
    }

    public async Task<IDataContext> GetAsync(string path = "", CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetAsync");

        if (_context == null)
        {
            var json = _fileSystem.File.ReadAllText(path);

            return await GetAsync(JsonSerializer.Deserialize<JsonElement>(json), cancellationToken).ConfigureAwait(false);
        }

        return _context;
    }

    public async Task<IDataContext> GetAsync(JsonElement jsonElement, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetAsync");

        if (_context == null)
        {
            _context = JsonSerializer.Deserialize<DataContext>(jsonElement, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            })!;

            foreach (var boundedContext in _context.BoundedContexts)
            {
                boundedContext.ProductName = _context.ProductName;

                foreach (var aggregate in boundedContext.Aggregates)
                {
                    aggregate.BoundedContext = boundedContext;

                    foreach (var request in aggregate.Commands)
                    {
                        request.Aggregate = aggregate;
                        request.ProductName = _context.ProductName;
                    }

                    foreach (var request in aggregate.Queries)
                    {
                        request.Aggregate = aggregate;
                        request.ProductName = _context.ProductName;
                    }
                }
            }

        }

        return _context;
    }
}
