// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Endpoint.Engineering.AI.Services;

public class CodeParser: ICodeParser
{
    private readonly ILogger<CodeParser> _logger;

    public CodeParser(ILogger<CodeParser> logger){
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;

    }

    public async Task DoWorkAsync()
    {
        _logger.LogInformation("DoWorkAsync");

    }

}

