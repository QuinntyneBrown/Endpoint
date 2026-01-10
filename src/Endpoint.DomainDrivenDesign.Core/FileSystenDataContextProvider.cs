// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Endpoint.DomainDrivenDesign.Core;

public class FileSystenDataContextProvider : IDataContextProvider
{
    private readonly ILogger<FileSystenDataContextProvider> _logger;
    private readonly FileSystenDataContextProviderOptions _options;
    private IDataContext _context = default!;

    public FileSystenDataContextProvider(ILogger<FileSystenDataContextProvider> logger, IOptions<FileSystenDataContextProviderOptions> optionsAccessor)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(optionsAccessor?.Value, nameof(FileSystenDataContextProviderOptions));

        _logger = logger;
        _options = optionsAccessor.Value;
    }

    public async Task<IDataContext> GetAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("GetAsync");

        if (_context == null)
        {

        }

        return _context;

    }

}


public class FileSystenDataContextProviderOptions
{
    public string Path { get; set; } = string.Empty;
}
