// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ECommercePlatform.CustomerManagement.Core.Aggregates.CustomerManagement;

public class CustomerManagementDto
{
    public required string CustomerManagementId { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string Status { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required DateTime ModifiedAt { get; set; }
}

