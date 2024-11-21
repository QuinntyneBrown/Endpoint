// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Core.Internal;
using Endpoint.DotNet.Services;
using MediatR;

namespace Endpoint.DotNet.Artifacts.Projects.Strategies;

public class ConsoleMicroserviceArtifactGenerationStrategy : IArtifactGenerationStrategy<ConsoleMicroserviceProjectModel>
{
    private readonly IFileSystem fileSytem;
    private readonly Observable<INotification> notificationListener;

    public ConsoleMicroserviceArtifactGenerationStrategy(

        IFileSystem fileSystem,
        Observable<INotification> notificationListener)
    {
        fileSytem = fileSystem;
        this.notificationListener = notificationListener;
    }

    public async Task GenerateAsync( ConsoleMicroserviceProjectModel model)
    {
        throw new NotImplementedException();
    }
}
