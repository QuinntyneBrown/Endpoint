// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using FluentValidation;

namespace Sample.Models.ToDo;

public class DeleteToDoRequestValidator: AbstractValidator<DeleteToDoRequest>
{
    public DeleteToDoRequestValidator(){
        RuleFor(x => x.ToDoId).NotNull();

    }

}

