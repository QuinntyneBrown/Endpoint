// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using MediatR;

namespace Sample.Models.ToDo;

public class GetToDoByIdRequest: IRequest<GetToDoByIdResponse>
{
    public Guid ToDoId { get; set; }
}

