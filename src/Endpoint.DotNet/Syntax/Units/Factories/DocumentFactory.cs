// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Endpoint.DotNet.Services;
using Endpoint.DotNet.Syntax.Classes;
using Endpoint.DotNet.Syntax.Classes.Factories;
using Endpoint.DotNet.Syntax.Documents;
using Humanizer;
using Microsoft.Extensions.Logging;

namespace Endpoint.DotNet.Syntax.Units.Factories;

public class DocumentFactory : IDocumentFactory
{
    private readonly ILogger<DocumentFactory> logger;
    private readonly IClassFactory classFactory;
    private readonly IContext context;

    public DocumentFactory(ILogger<DocumentFactory> logger, IClassFactory classFactory, IContext context)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.classFactory = classFactory ?? throw new ArgumentNullException(nameof(classFactory));
        this.context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<DocumentModel> CreateCommandAsync(ClassModel aggregate, RouteType routeType)
    {
        throw new NotImplementedException();
    }

    public async Task<DocumentModel> CreateQueryAsync(ClassModel aggregate, RouteType routeType)
    {
        throw new NotImplementedException();
    }
}
