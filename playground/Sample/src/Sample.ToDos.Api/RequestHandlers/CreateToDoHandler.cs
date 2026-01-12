// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using MediatR;
using Sample.Models.ToDo;

namespace Sample.ToDos.Api.RequestHandlers;

public class CreateToDoHandler: IRequestHandler<CreateToDoRequest, CreateToDoResponse>
{
    private readonly ILogger<CreateToDoHandler> _logger;

    private readonly IToDosDbContext _context;

    public CreateToDoHandler(ILogger<CreateToDoHandler> logger,IToDosDbContext context){
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(context);

        _logger = logger;
        _context = context;

    }

    public async Task<CreateToDoResponse> Handle(CreateToDoRequest request,CancellationToken cancellationToken)
    {
        var toDo = new ToDo()
        {
            Title = request.Title,    IsComplete = request.IsComplete
        };

        _context.ToDos.Add(toDo);

        await _context.SaveChangesAsync(cancellationToken);

        return new() { 

            ToDo = toDo.ToDto(),
        };

    }

}

