// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using FluentValidation;

namespace Sample.Models.ToDo;

public class CreateToDoRequestValidator: AbstractValidator<CreateToDoRequest>
{
    public CreateToDoRequestValidator(){
        RuleFor(x => x.Title).NotNull().NotEmpty();
        RuleFor(x => x.IsComplete).NotNull().NotEmpty();

    }

}

