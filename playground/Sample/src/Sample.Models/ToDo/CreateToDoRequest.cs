// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using MediatR;

namespace Sample.Models.ToDo;

public class CreateToDoRequest: IRequest<CreateToDoResponse>
{
    public string Title { get; set; }
    public string IsComplete { get; set; }
}

