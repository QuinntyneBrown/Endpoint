// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint.Engineering.ALaCarte.Api.Features.ALaCarteRequests.Commands.CreateALaCarteRequest;
using Endpoint.Engineering.ALaCarte.Api.Features.ALaCarteRequests.Commands.DeleteALaCarteRequest;
using Endpoint.Engineering.ALaCarte.Api.Features.ALaCarteRequests.Commands.UpdateALaCarteRequest;
using Endpoint.Engineering.ALaCarte.Api.Features.ALaCarteRequests.Queries.GetALaCarteRequest;
using Endpoint.Engineering.ALaCarte.Api.Features.ALaCarteRequests.Queries.GetALaCarteRequests;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Endpoint.Engineering.ALaCarte.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ALaCarteRequestsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ALaCarteRequestsController> _logger;

    public ALaCarteRequestsController(IMediator mediator, ILogger<ALaCarteRequestsController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet]
    public async Task<ActionResult<GetALaCarteRequestsResponse>> GetAll(CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetALaCarteRequestsRequest(), cancellationToken);
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<CreateALaCarteRequestResponse>> Create(
        CreateALaCarteRequestRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = response.ALaCarteRequestId }, response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GetALaCarteRequestResponse>> Get(
        Guid id,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(new GetALaCarteRequestRequest { ALaCarteRequestId = id }, cancellationToken);
        if (response == null)
        {
            return NotFound();
        }
        return Ok(response);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(
        Guid id,
        UpdateALaCarteRequestRequest request,
        CancellationToken cancellationToken)
    {
        if (id != request.ALaCarteRequestId)
        {
            return BadRequest();
        }

        await _mediator.Send(request, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteALaCarteRequestRequest { ALaCarteRequestId = id }, cancellationToken);
        return NoContent();
    }
}
