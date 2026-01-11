// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Endpoint.Cli.Commands;
using Endpoint.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Endpoint.Cli.UnitTests;

public class OpenApiCreateTests
{
    [Fact]
    public void OpenApiCreateRequest_ShouldHaveDefaultDirectory()
    {
        var request = new OpenApiCreateRequest();

        Assert.Equal(System.Environment.CurrentDirectory, request.Directory);
    }

    [Fact]
    public void OpenApiCreateRequestHandler_ShouldNotBeNull()
    {
        var logger = new Mock<ILogger<OpenApiCreateRequestHandler>>();
        var fileProvider = new Mock<IFileProvider>();

        var handler = new OpenApiCreateRequestHandler(logger.Object, fileProvider.Object);

        Assert.NotNull(handler);
    }

    [Fact]
    public async Task Handle_ShouldLogError_WhenSolutionFileNotFound()
    {
        var logger = new Mock<ILogger<OpenApiCreateRequestHandler>>();
        var fileProvider = new Mock<IFileProvider>();
        fileProvider.Setup(x => x.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .Returns(string.Empty);

        var handler = new OpenApiCreateRequestHandler(logger.Object, fileProvider.Object);
        var request = new OpenApiCreateRequest { Directory = "/tmp/nonexistent" };

        await handler.Handle(request, CancellationToken.None);

        logger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No solution file found")),
                It.IsAny<System.Exception>(),
                It.IsAny<System.Func<It.IsAnyType, System.Exception, string>>()),
            Times.Once);
    }
}
