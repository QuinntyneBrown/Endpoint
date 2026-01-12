// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using MediatR;
using Sample.Models.ToDo;

namespace Sample.ToDos.Api.RequestHandlers;

public class DeleteToDoHandler: IRequestHandler<DeleteToDoRequest, DeleteToDoResponse>
{
    private readonly ILogger<DeleteToDoHandler> _logger;

    private readonly IToDosDbContext _context;

    public DeleteToDoHandler(ILogger<DeleteToDoHandler> logger,IToDosDbContext context){
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(context);

        _logger = logger;
        _context = context;

    }

    public async Task<DeleteToDoResponse> Handle(DeleteToDoRequest request,CancellationToken cancellationToken)
    {
        var toDo = await _context.ToDos.FindAsync(request.ToDoId);

        _context.ToDos.Remove(toDo);

        await _context.SaveChangesAsync(cancellationToken);

        return new();

    }

}

