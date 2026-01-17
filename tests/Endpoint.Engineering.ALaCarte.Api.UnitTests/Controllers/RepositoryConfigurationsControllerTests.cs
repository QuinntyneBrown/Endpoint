// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.ALaCarte.Api.Controllers;
using Endpoint.Engineering.ALaCarte.Api.Features.RepositoryConfigurations.Commands.CreateRepositoryConfiguration;
using Endpoint.Engineering.ALaCarte.Api.Features.RepositoryConfigurations.Commands.DeleteRepositoryConfiguration;
using Endpoint.Engineering.ALaCarte.Api.Features.RepositoryConfigurations.Commands.UpdateRepositoryConfiguration;
using Endpoint.Engineering.ALaCarte.Api.Features.RepositoryConfigurations.Queries.GetRepositoryConfiguration;
using Endpoint.Engineering.ALaCarte.Api.Features.RepositoryConfigurations.Queries.GetRepositoryConfigurations;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Endpoint.Engineering.ALaCarte.Api.UnitTests.Controllers;

public class RepositoryConfigurationsControllerTests
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<RepositoryConfigurationsController>> _mockLogger;
    private readonly RepositoryConfigurationsController _controller;

    public RepositoryConfigurationsControllerTests()
    {
        _mockMediator = new Mock<IMediator>();
        _mockLogger = new Mock<ILogger<RepositoryConfigurationsController>>();
        _controller = new RepositoryConfigurationsController(_mockMediator.Object, _mockLogger.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullMediator_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new RepositoryConfigurationsController(null!, _mockLogger.Object));
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new RepositoryConfigurationsController(_mockMediator.Object, null!));
    }

    #endregion

    #region GetAll Tests

    [Fact]
    public async Task GetAll_ReturnsOkResult_WithRepositoryConfigurations()
    {
        // Arrange
        var expectedResponse = new GetRepositoryConfigurationsResponse
        {
            RepositoryConfigurations = new List<RepositoryConfigurationDto>
            {
                new RepositoryConfigurationDto
                {
                    RepositoryConfigurationId = Guid.NewGuid(),
                    Url = "https://github.com/user/repo",
                    Branch = "main"
                }
            }
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetRepositoryConfigurationsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetAll(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<GetRepositoryConfigurationsResponse>(okResult.Value);
        Assert.Single(response.RepositoryConfigurations);
    }

    [Fact]
    public async Task GetAll_ReturnsEmptyList_WhenNoConfigurations()
    {
        // Arrange
        var expectedResponse = new GetRepositoryConfigurationsResponse
        {
            RepositoryConfigurations = new List<RepositoryConfigurationDto>()
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetRepositoryConfigurationsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetAll(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<GetRepositoryConfigurationsResponse>(okResult.Value);
        Assert.Empty(response.RepositoryConfigurations);
    }

    #endregion

    #region Get Tests

    [Fact]
    public async Task Get_ReturnsOkResult_WhenConfigurationExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var expectedResponse = new GetRepositoryConfigurationResponse
        {
            RepositoryConfigurationId = id,
            Url = "https://github.com/user/repo",
            Branch = "main"
        };

        _mockMediator
            .Setup(m => m.Send(It.Is<GetRepositoryConfigurationRequest>(r => r.RepositoryConfigurationId == id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.Get(id, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<GetRepositoryConfigurationResponse>(okResult.Value);
        Assert.Equal(id, response.RepositoryConfigurationId);
    }

    [Fact]
    public async Task Get_ReturnsNotFound_WhenConfigurationDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();

        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetRepositoryConfigurationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetRepositoryConfigurationResponse?)null);

        // Act
        var result = await _controller.Get(id, CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_ReturnsCreatedAtAction_WithNewConfiguration()
    {
        // Arrange
        var request = new CreateRepositoryConfigurationRequest
        {
            Url = "https://github.com/user/repo",
            Branch = "main"
        };
        var expectedResponse = new CreateRepositoryConfigurationResponse
        {
            RepositoryConfigurationId = Guid.NewGuid()
        };

        _mockMediator
            .Setup(m => m.Send(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.Create(request, CancellationToken.None);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(_controller.Get), createdResult.ActionName);
        var response = Assert.IsType<CreateRepositoryConfigurationResponse>(createdResult.Value);
        Assert.Equal(expectedResponse.RepositoryConfigurationId, response.RepositoryConfigurationId);
    }

    [Fact]
    public async Task Create_SendsCorrectRequest_ToMediator()
    {
        // Arrange
        var request = new CreateRepositoryConfigurationRequest
        {
            Url = "https://github.com/user/repo",
            Branch = "develop",
            LocalDirectory = "/local/path"
        };
        var expectedResponse = new CreateRepositoryConfigurationResponse
        {
            RepositoryConfigurationId = Guid.NewGuid()
        };

        _mockMediator
            .Setup(m => m.Send(It.IsAny<CreateRepositoryConfigurationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        await _controller.Create(request, CancellationToken.None);

        // Assert
        _mockMediator.Verify(m => m.Send(
            It.Is<CreateRepositoryConfigurationRequest>(r =>
                r.Url == request.Url &&
                r.Branch == request.Branch &&
                r.LocalDirectory == request.LocalDirectory),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_ReturnsNoContent_WhenSuccessful()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = new UpdateRepositoryConfigurationRequest
        {
            RepositoryConfigurationId = id,
            Url = "https://github.com/user/updated-repo",
            Branch = "main"
        };

        _mockMediator
            .Setup(m => m.Send(request, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Update(id, request, CancellationToken.None);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Update_ReturnsBadRequest_WhenIdMismatch()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = new UpdateRepositoryConfigurationRequest
        {
            RepositoryConfigurationId = Guid.NewGuid(), // Different ID
            Url = "https://github.com/user/repo",
            Branch = "main"
        };

        // Act
        var result = await _controller.Update(id, request, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task Update_SendsCorrectRequest_ToMediator()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = new UpdateRepositoryConfigurationRequest
        {
            RepositoryConfigurationId = id,
            Url = "https://github.com/user/repo",
            Branch = "feature"
        };

        // Act
        await _controller.Update(id, request, CancellationToken.None);

        // Assert
        _mockMediator.Verify(m => m.Send(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenSuccessful()
    {
        // Arrange
        var id = Guid.NewGuid();

        _mockMediator
            .Setup(m => m.Send(It.Is<DeleteRepositoryConfigurationRequest>(r => r.RepositoryConfigurationId == id), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Delete(id, CancellationToken.None);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_SendsCorrectRequest_ToMediator()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        await _controller.Delete(id, CancellationToken.None);

        // Assert
        _mockMediator.Verify(m => m.Send(
            It.Is<DeleteRepositoryConfigurationRequest>(r => r.RepositoryConfigurationId == id),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion
}
