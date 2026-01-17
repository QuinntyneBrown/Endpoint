// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.ALaCarte.Api.Features.RepositoryConfigurations.Commands.CreateRepositoryConfiguration;
using Endpoint.Engineering.ALaCarte.Api.Features.RepositoryConfigurations.Commands.DeleteRepositoryConfiguration;
using Endpoint.Engineering.ALaCarte.Api.Features.RepositoryConfigurations.Commands.UpdateRepositoryConfiguration;
using Endpoint.Engineering.ALaCarte.Api.Features.RepositoryConfigurations.Queries.GetRepositoryConfiguration;
using Endpoint.Engineering.ALaCarte.Api.Features.RepositoryConfigurations.Queries.GetRepositoryConfigurations;
using MediatR;

namespace Endpoint.Engineering.ALaCarte.Api.UnitTests.Features.RepositoryConfigurations;

#region CreateRepositoryConfiguration Tests

public class CreateRepositoryConfigurationRequestTests
{
    [Fact]
    public void CreateRepositoryConfigurationRequest_DefaultValues()
    {
        // Arrange & Act
        var request = new CreateRepositoryConfigurationRequest();

        // Assert
        Assert.Equal(string.Empty, request.Url);
        Assert.Equal("main", request.Branch);
        Assert.Null(request.LocalDirectory);
    }

    [Fact]
    public void CreateRepositoryConfigurationRequest_ImplementsIRequest()
    {
        // Arrange
        var request = new CreateRepositoryConfigurationRequest();

        // Assert
        Assert.IsAssignableFrom<IRequest<CreateRepositoryConfigurationResponse>>(request);
    }

    [Fact]
    public void CreateRepositoryConfigurationRequest_CanSetAllProperties()
    {
        // Arrange & Act
        var request = new CreateRepositoryConfigurationRequest
        {
            Url = "https://github.com/user/repo",
            Branch = "develop",
            LocalDirectory = "/local/path"
        };

        // Assert
        Assert.Equal("https://github.com/user/repo", request.Url);
        Assert.Equal("develop", request.Branch);
        Assert.Equal("/local/path", request.LocalDirectory);
    }
}

public class CreateRepositoryConfigurationResponseTests
{
    [Fact]
    public void CreateRepositoryConfigurationResponse_DefaultValues()
    {
        // Arrange & Act
        var response = new CreateRepositoryConfigurationResponse();

        // Assert
        Assert.Equal(Guid.Empty, response.RepositoryConfigurationId);
    }

    [Fact]
    public void CreateRepositoryConfigurationResponse_CanSetId()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var response = new CreateRepositoryConfigurationResponse
        {
            RepositoryConfigurationId = id
        };

        // Assert
        Assert.Equal(id, response.RepositoryConfigurationId);
    }
}

#endregion

#region UpdateRepositoryConfiguration Tests

public class UpdateRepositoryConfigurationRequestTests
{
    [Fact]
    public void UpdateRepositoryConfigurationRequest_DefaultValues()
    {
        // Arrange & Act
        var request = new UpdateRepositoryConfigurationRequest();

        // Assert
        Assert.Equal(Guid.Empty, request.RepositoryConfigurationId);
        Assert.Equal(string.Empty, request.Url);
        Assert.Equal("main", request.Branch);
        Assert.Null(request.LocalDirectory);
    }

    [Fact]
    public void UpdateRepositoryConfigurationRequest_ImplementsIRequest()
    {
        // Arrange
        var request = new UpdateRepositoryConfigurationRequest();

        // Assert
        Assert.IsAssignableFrom<IRequest>(request);
    }

    [Fact]
    public void UpdateRepositoryConfigurationRequest_CanSetAllProperties()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var request = new UpdateRepositoryConfigurationRequest
        {
            RepositoryConfigurationId = id,
            Url = "https://github.com/user/updated-repo",
            Branch = "feature",
            LocalDirectory = "/new/path"
        };

        // Assert
        Assert.Equal(id, request.RepositoryConfigurationId);
        Assert.Equal("https://github.com/user/updated-repo", request.Url);
        Assert.Equal("feature", request.Branch);
        Assert.Equal("/new/path", request.LocalDirectory);
    }
}

#endregion

#region DeleteRepositoryConfiguration Tests

public class DeleteRepositoryConfigurationRequestTests
{
    [Fact]
    public void DeleteRepositoryConfigurationRequest_DefaultValues()
    {
        // Arrange & Act
        var request = new DeleteRepositoryConfigurationRequest();

        // Assert
        Assert.Equal(Guid.Empty, request.RepositoryConfigurationId);
    }

    [Fact]
    public void DeleteRepositoryConfigurationRequest_ImplementsIRequest()
    {
        // Arrange
        var request = new DeleteRepositoryConfigurationRequest();

        // Assert
        Assert.IsAssignableFrom<IRequest>(request);
    }

    [Fact]
    public void DeleteRepositoryConfigurationRequest_CanSetId()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var request = new DeleteRepositoryConfigurationRequest
        {
            RepositoryConfigurationId = id
        };

        // Assert
        Assert.Equal(id, request.RepositoryConfigurationId);
    }
}

#endregion

#region GetRepositoryConfiguration Tests

public class GetRepositoryConfigurationRequestTests
{
    [Fact]
    public void GetRepositoryConfigurationRequest_DefaultValues()
    {
        // Arrange & Act
        var request = new GetRepositoryConfigurationRequest();

        // Assert
        Assert.Equal(Guid.Empty, request.RepositoryConfigurationId);
    }

    [Fact]
    public void GetRepositoryConfigurationRequest_ImplementsIRequest()
    {
        // Arrange
        var request = new GetRepositoryConfigurationRequest();

        // Assert
        Assert.IsAssignableFrom<IRequest<GetRepositoryConfigurationResponse?>>(request);
    }

    [Fact]
    public void GetRepositoryConfigurationRequest_CanSetId()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var request = new GetRepositoryConfigurationRequest
        {
            RepositoryConfigurationId = id
        };

        // Assert
        Assert.Equal(id, request.RepositoryConfigurationId);
    }
}

public class GetRepositoryConfigurationResponseTests
{
    [Fact]
    public void GetRepositoryConfigurationResponse_DefaultValues()
    {
        // Arrange & Act
        var response = new GetRepositoryConfigurationResponse();

        // Assert
        Assert.Equal(Guid.Empty, response.RepositoryConfigurationId);
        Assert.Equal(string.Empty, response.Url);
        Assert.Equal(string.Empty, response.Branch);
        Assert.Null(response.LocalDirectory);
    }

    [Fact]
    public void GetRepositoryConfigurationResponse_CanSetAllProperties()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var response = new GetRepositoryConfigurationResponse
        {
            RepositoryConfigurationId = id,
            Url = "https://github.com/user/repo",
            Branch = "main",
            LocalDirectory = "/local/path"
        };

        // Assert
        Assert.Equal(id, response.RepositoryConfigurationId);
        Assert.Equal("https://github.com/user/repo", response.Url);
        Assert.Equal("main", response.Branch);
        Assert.Equal("/local/path", response.LocalDirectory);
    }
}

#endregion

#region GetRepositoryConfigurations Tests

public class GetRepositoryConfigurationsRequestTests
{
    [Fact]
    public void GetRepositoryConfigurationsRequest_ImplementsIRequest()
    {
        // Arrange
        var request = new GetRepositoryConfigurationsRequest();

        // Assert
        Assert.IsAssignableFrom<IRequest<GetRepositoryConfigurationsResponse>>(request);
    }
}

public class GetRepositoryConfigurationsResponseTests
{
    [Fact]
    public void GetRepositoryConfigurationsResponse_DefaultValues()
    {
        // Arrange & Act
        var response = new GetRepositoryConfigurationsResponse();

        // Assert
        Assert.NotNull(response.RepositoryConfigurations);
        Assert.Empty(response.RepositoryConfigurations);
    }

    [Fact]
    public void GetRepositoryConfigurationsResponse_CanAddConfigurations()
    {
        // Arrange
        var response = new GetRepositoryConfigurationsResponse();

        // Act
        response.RepositoryConfigurations.Add(new RepositoryConfigurationDto
        {
            RepositoryConfigurationId = Guid.NewGuid(),
            Url = "https://github.com/user/repo",
            Branch = "main"
        });

        // Assert
        Assert.Single(response.RepositoryConfigurations);
    }
}

public class RepositoryConfigurationDtoTests
{
    [Fact]
    public void RepositoryConfigurationDto_DefaultValues()
    {
        // Arrange & Act
        var dto = new RepositoryConfigurationDto();

        // Assert
        Assert.Equal(Guid.Empty, dto.RepositoryConfigurationId);
        Assert.Equal(string.Empty, dto.Url);
        Assert.Equal(string.Empty, dto.Branch);
        Assert.Null(dto.LocalDirectory);
    }

    [Fact]
    public void RepositoryConfigurationDto_CanSetAllProperties()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var dto = new RepositoryConfigurationDto
        {
            RepositoryConfigurationId = id,
            Url = "https://github.com/user/repo",
            Branch = "develop",
            LocalDirectory = "/path"
        };

        // Assert
        Assert.Equal(id, dto.RepositoryConfigurationId);
        Assert.Equal("https://github.com/user/repo", dto.Url);
        Assert.Equal("develop", dto.Branch);
        Assert.Equal("/path", dto.LocalDirectory);
    }
}

#endregion
