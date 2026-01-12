// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using Sample.Models.ToDo;

namespace Sample.ToDos.Api.Controllers;

[ApiController]
[Route("api/todos")]
public class ToDosController: Controller
{
    private readonly ILogger<ToDosController> _logger;

    private readonly IMediator _mediator;

    public ToDosController(ILogger<ToDosController> logger,IMediator mediator){
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediator);

        _logger = logger;
        _mediator = mediator;

    }

    [HttpPost(Name = "Create")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAsync([FromBody]CreateToDoRequest request)
    {
        var response = await _mediator.Send(request);

        return Ok(response);

    }

    [HttpPut(Name = "Update")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateAsync([FromBody]UpdateToDoRequest request)
    {
        var response = await _mediator.Send(request);

        return Ok(response);

    }

    [HttpDelete("{toDoId}", Name = "Delete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteAsync([FromRoute]DeleteToDoRequest request)
    {
        var response = await _mediator.Send(request);

        return Ok(response);

    }

    [HttpGet(Name = "Get")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAsync([FromRoute]GetToDosRequest request)
    {
        var response = await _mediator.Send(request);

        return Ok(response);

    }

    [HttpGet("{toDoId}", Name = "GetById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByIdAsync([FromRoute]GetToDoByIdRequest request)
    {
        var response = await _mediator.Send(request);

        if(response.ToDo == null)
        {
            return NotFound(request.ToDoId);
        }

        return Ok(response);

    }

}

