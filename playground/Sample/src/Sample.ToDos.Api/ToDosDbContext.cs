// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;
using Sample.Models.ToDo;

namespace Sample.ToDos.Api;

public class ToDosDbContext: DbContext,IToDosDbContext
{
    public ToDosDbContext(DbContextOptions<ToDosDbContext> options)    : base(options)
    {


    }

    public DbSet<ToDo> ToDos { get; set; }
}

