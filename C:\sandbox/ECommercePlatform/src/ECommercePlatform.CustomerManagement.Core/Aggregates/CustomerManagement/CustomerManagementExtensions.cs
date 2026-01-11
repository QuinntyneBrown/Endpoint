// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ECommercePlatform.CustomerManagement.Core.Aggregates.CustomerManagement;

public static class CustomerManagementExtensions
{
    public static CustomerManagementDto ToDto(this CustomerManagement entity)
    {
        return new CustomerManagementDto
                {
                    CustomerManagementId = entity.CustomerManagementId,
                    Name = entity.Name,
                    Description = entity.Description,
                    Status = entity.Status,
                    CreatedAt = entity.CreatedAt,
                    ModifiedAt = entity.ModifiedAt
                };

    }

}

