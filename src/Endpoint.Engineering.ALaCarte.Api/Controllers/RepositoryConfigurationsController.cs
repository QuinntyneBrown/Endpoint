// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.ALaCarte.Api.Features.RepositoryConfigurations.Commands.CreateRepositoryConfiguration;
using Endpoint.Engineering.ALaCarte.Api.Features.RepositoryConfigurations.Commands.DeleteRepositoryConfiguration;
using Endpoint.Engineering.ALaCarte.Api.Features.RepositoryConfigurations.Commands.UpdateRepositoryConfiguration;
using Endpoint.Engineering.ALaCarte.Api.Features.RepositoryConfigurations.Queries.GetRepositoryConfiguration;
using Endpoint.Engineering.ALaCarte.Api.Features.RepositoryConfigurations.Queries.GetRepositoryConfigurations;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Endpoint.Engineering.ALaCarte.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RepositoryConfigurationsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<RepositoryConfigurationsController> _logger;

    public RepositoryConfigurationsController(IMediator mediator, ILogger<RepositoryConfigurationsController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet]
    public async Task<ActionResult<GetRepositoryConfigurationsResponse>> GetAll(CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetRepositoryConfigurationsRequest(), cancellationToken);
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<CreateRepositoryConfigurationResponse>> Create(
        CreateRepositoryConfigurationRequest request, 
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = response.RepositoryConfigurationId }, response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GetRepositoryConfigurationResponse>> Get(
        Guid id, 
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetRepositoryConfigurationRequest { RepositoryConfigurationId = id }, cancellationToken);
        if (response == null)
        {
            return NotFound();
        }
        return Ok(response);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(
        Guid id, 
        UpdateRepositoryConfigurationRequest request, 
        CancellationToken cancellationToken)
    {
        if (id != request.RepositoryConfigurationId)
        {
            return BadRequest();
        }

        await _mediator.Send(request, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteRepositoryConfigurationRequest { RepositoryConfigurationId = id }, cancellationToken);
        return NoContent();
    }
}
