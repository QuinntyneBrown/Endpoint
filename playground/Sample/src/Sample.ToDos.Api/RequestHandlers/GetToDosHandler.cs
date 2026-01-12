// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using MediatR;
using Sample.Models.ToDo;
using MediatR;
using Sample.Models.ToDo;

namespace Sample.ToDos.Api.RequestHandlers;

public class GetToDosHandler: IRequestHandler<GetToDosRequest, GetToDosResponse>
{
    private readonly ILogger<GetToDosHandler> _logger;

    private readonly IToDosDbContext _context;

    public GetToDosHandler(ILogger<GetToDosHandler> logger,IToDosDbContext context){
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(context);

        _logger = logger;
        _context = context;

    }

    public async Task<GetToDosResponse> Handle(GetToDosRequest request,CancellationToken cancellationToken)
    {
        return new GetToDosResponse()
        {
            ToDos = _context.ToDos
            .Select(x => x.ToDto())
            .ToList()
        };

    }

}

