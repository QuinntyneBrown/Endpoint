// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using MediatR;
using Sample.Models.ToDo;
using MediatR;
using Sample.Models.ToDo;

namespace Sample.ToDos.Api.RequestHandlers;

public class GetToDoByIdHandler: IRequestHandler<GetToDoByIdRequest, GetToDoByIdResponse>
{
    private readonly ILogger<GetToDoByIdHandler> _logger;

    private readonly IToDosDbContext _context;

    public GetToDoByIdHandler(ILogger<GetToDoByIdHandler> logger,IToDosDbContext context){
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(context);

        _logger = logger;
        _context = context;

    }

    public async Task<GetToDoByIdResponse> Handle(GetToDoByIdRequest request,CancellationToken cancellationToken)
    {
        return new GetToDoByIdResponse()
        {
            ToDo = _context.ToDos
            .SingleOrDefault(x => x.ToDoId == request.ToDoId)?
            .ToDto()
        };

    }

}

