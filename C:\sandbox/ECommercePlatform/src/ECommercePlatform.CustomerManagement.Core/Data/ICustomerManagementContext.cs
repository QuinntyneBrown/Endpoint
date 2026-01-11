// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;
using CustomerManagementEntity = ECommercePlatform.CustomerManagement.Core.Aggregates.CustomerManagement.CustomerManagement;

namespace ECommercePlatform.CustomerManagement.Core.Data;

public interface ICustomerManagementContext
{
    DbSet<CustomerManagementEntity> CustomerManagements { get; set; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
