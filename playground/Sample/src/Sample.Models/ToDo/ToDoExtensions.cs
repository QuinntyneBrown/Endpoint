// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Sample.Models.ToDo;

public static class ToDoExtensions
{
    public static ToDoDto ToDto(this ToDo toDo)
    {
        return new ToDoDto()
        {
            ToDoId = toDo.ToDoId,
            Title = toDo.Title,
            IsComplete = toDo.IsComplete,
        };

    }

}

