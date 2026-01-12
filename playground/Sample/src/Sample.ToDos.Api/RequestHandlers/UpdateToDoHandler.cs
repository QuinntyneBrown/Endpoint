// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using MediatR;
using Sample.Models.ToDo;

namespace Sample.ToDos.Api.RequestHandlers;

public class UpdateToDoHandler: IRequestHandler<UpdateToDoRequest, UpdateToDoResponse>
{
    private readonly ILogger<UpdateToDoHandler> _logger;

    private readonly IToDosDbContext _context;

    public UpdateToDoHandler(ILogger<UpdateToDoHandler> logger,IToDosDbContext context){
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(context);

        _logger = logger;
        _context = context;

    }

    public async Task<UpdateToDoResponse> Handle(UpdateToDoRequest request,CancellationToken cancellationToken)
    {
        var toDo = _context.ToDos
        .Single(x => x.ToDoId == request.ToDoId);
        toDo.Title = request.Title;
        toDo.IsComplete = request.IsComplete;
        await _context.SaveChangesAsync(cancellationToken);

        return new()
        {
            ToDo = toDo.ToDto()
        };

    }

}

